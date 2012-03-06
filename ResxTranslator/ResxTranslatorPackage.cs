using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.ResxTranslator.TranslatorService;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Shell;
using Locan.Translate;
using Locan.Translate.IO;
using Microsoft.ResxTranslator.Properties;

namespace Microsoft.ResxTranslator
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]   
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidResxTranslatorPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    public sealed class ResxTranslatorPackage : Package
    {
        private DTE2 dte;
        private static string[] languages;
        private int count;
        private ProjectItem fileNode;
        private static TranslateOptions options = new TranslateOptions() { Category = "tech", ContentType = "text/plain" };
        private string apiKey;

        private TranslationManager TranslationManager { get; set; }
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ResxTranslatorPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

            this.TranslationManager = new TranslationManager();
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)) as DTE2;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidResxTranslatorCmdSet, (int)PkgCmdIDList.cmdidOneClickLocalization);
                OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += new EventHandler(menuItem_BeforeQueryStatus);

                // Don't wire up the crowdSource command since it is not functional now
                //CommandID crowdSourceCommandID = new CommandID(GuidList.guidResxTranslatorCrowdSource, (int)PkgCmdIDList.cmdidOneClickLocalization);
                //OleMenuCommand crowdSourceItem = new OleMenuCommand(CrowdSourceCallback, crowdSourceCommandID);
                //mcs.AddCommand(crowdSourceItem);
                //crowdSourceItem.BeforeQueryStatus += new EventHandler(menuItem_BeforeQueryStatus);
            }
        }

        void menuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            var files = GetSelectedItemPaths().ToArray();
            fileNode = files.Length > 0 ? files[0] : null;
            bool isVisible = false;

            if (fileNode != null)
            {
                string file = fileNode.Properties.Item("FullPath").Value.ToString();
                isVisible = Path.GetExtension(file).Equals(".resx", StringComparison.OrdinalIgnoreCase);
            }

            menuCommand.Visible = isVisible;
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>

        private void MenuItemCallback(object sender, EventArgs e) {
            apiKey = GetApiKey() ?? AskForApiKey();
            if (!string.IsNullOrEmpty(apiKey)) {
                this.TranslationManager.Translate(apiKey, this.GetActiveProject(), fileNode, dte);
                
                // This is temporarily here should be removed at some point
            //    ProjectItem pi = fileNode as ProjectItem;
            //    Guid fileId = this.TranslationManager.UpdloadFileForSharing(
            //        pi.Properties.Item("FullPath").Value.ToString(),
            //        apiKey,
            //        pi.ContainingProject.Name);
            }
        }

        private void CrowdSourceCallback(object sender, EventArgs e)
        {
            apiKey = GetApiKey() ?? AskForApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                //this.TranslationManager.Translate(apiKey, this.GetActiveProject(), fileNode, dte);

                // This is temporarily here should be removed at some point
                ProjectItem pi = fileNode as ProjectItem;
                Guid fileId = this.TranslationManager.UpdloadFileForSharing(
                    pi.Properties.Item("FullPath").Value.ToString(),
                    apiKey,
                    pi.ContainingProject.Name);

                System.Diagnostics.Process.Start("http://localhost:64245/crowdsource/" + fileId);
            }
        }
        
        private void MenuItemCallback_OLD(object sender, EventArgs e)
        {
            apiKey = GetApiKey() ?? AskForApiKey();
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                count = 0;
                dte.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);

                string file = fileNode.Properties.Item("FullPath").Value.ToString();

                if (file != null)
                {
                    List<string> keys = new List<string>();
                    List<string> values = new List<string>();

                    GetLanguages();
                    ReadResxFile(file, keys, values);
                    Translate(file, keys, values);
                }
            }
        }

        private string AskForApiKey()
        {
            System.Diagnostics.Process.Start("http://www.bing.com/developers/appids.aspx");
            string input = Interaction.InputBox("Please enter the Bing Translator API key", "Bing Translator", string.Empty).Trim();
            SetApiKey(input);
            return input;
        }

        private static void ReadResxFile(string file, List<string> keys, List<string> values)
        {
            using (ResXResourceReader reader = new ResXResourceReader(file))
            {
                foreach (DictionaryEntry d in reader)
                {
                    keys.Add(d.Key.ToString());
                    values.Add(d.Value.ToString());
                }
            }
        }

        private void Translate(string file, List<string> keys, List<string> values)
        {
            string currentLanguage = DetectLanguage(string.Join(" ", values.ToArray()));
            var translationLanguages = languages.Where(l => l != currentLanguage);

            foreach (string language in translationLanguages)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string fileName = file.Replace(".", "." + language + ".");

                using (TranslatorService.SoapService client = new TranslatorService.SoapService())
                {
                    client.TranslateArrayAsync(apiKey, values.ToArray(), currentLanguage, language, options, new object[] { fileName, dic, keys });
                    client.TranslateArrayCompleted += new TranslateArrayCompletedEventHandler(client_TranslateArrayCompleted);
                }
            }
        }

        private void client_TranslateArrayCompleted(object sender, TranslateArrayCompletedEventArgs e)
        {
            object[] state = (object[])e.UserState;
            var fileName = state[0].ToString();
            var dic = (Dictionary<string, string>)state[1];
            var keys = (List<string>)state[2];

            for (var i = 0; i < e.Result.Length; i++)
            {
                dic.Add(keys[i], e.Result[i].TranslatedText);
            }

            CreateResxFile(fileName, dic);
        }

        private void CreateResxFile(string fileName, Dictionary<string, string> values)
        {
            using (ResXResourceWriter resxWriter = new ResXResourceWriter(fileName))
            {
                foreach (string key in values.Keys)
                {
                    resxWriter.AddResource(key, values[key]);
                }
            }

            count++;
            dte.StatusBar.Progress(true, fileName, count, languages.Length - 1);
            var ost = fileNode.Collection.Parent as ProjectItem;
            ost.ProjectItems.AddFromFile(fileName);

            if (count == languages.Length - 1)
            {
                dte.StatusBar.Progress(false);
                dte.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
            }
        }

        private string DetectLanguage(string text)
        {
            using (TranslatorService.SoapService client = new TranslatorService.SoapService())
            {
                return client.Detect(apiKey, text);
            }
        }

        private void GetLanguages()
        {
            if (languages == null)
            {
                using (TranslatorService.SoapService client = new TranslatorService.SoapService())
                {
                    languages = client.GetLanguagesForTranslate(apiKey);
                }
            }
        }

        private IEnumerable<ProjectItem> GetSelectedItemPaths()
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
            foreach (UIHierarchyItem selItem in items)
            {
                var item = selItem.Object as ProjectItem;
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        private Project GetActiveProject() {
            Project activeProject = null;
            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;

            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0) {
                activeProject = activeSolutionProjects.GetValue(0) as Project;
            }

            return activeProject;
        }
        
        private string GetApiKey()
        {
            string apiKey = null;
            Project activeProject = this.GetActiveProject();

            if (activeProject != null) {
                apiKey = this.TranslationManager.GetApiKeyfor(activeProject);
            }

            return apiKey;
        }

        private void SetApiKey(string apiKey)
        {
            this.TranslationManager.SetApiKey(apiKey, this.GetActiveProject());
        }
    }
}
