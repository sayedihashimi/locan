namespace Microsoft.ResxTranslator {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using Locan.Translate;
    using Locan.Translate.Common;
    using Locan.Translate.IO;
    using Microsoft.ResxTranslator.Properties;
    using System.Net.Http;
    using System.Web.Script.Serialization;
    using System.Text;
    using Microsoft.ApplicationServer.Http;
    using System.Net.Http.Headers;
    using System.Xml.Linq;

    internal class TranslationManager {
        internal TranslationManager() {
            this.PreserveUpdates = true;
        }

        private bool PreserveUpdates { get; set; }
        internal string GetApiKeyfor(Project project) {
            if (project == null) { throw new ArgumentNullException("project"); }
            string locanSettingsPath = this.GetPathToLocanSettingsFile(project);

            ILocanSettings settings = this.GetProjectSettings(project);
            return settings.DefaultApiKey;
        }

        internal void SetApiKey(string apiKey, Project project) {
            if (project == null) { throw new ArgumentNullException("project"); }

            ILocanSettings settings = this.GetProjectSettings(project);
            settings.DefaultApiKey = apiKey;
            this.SaveProjetSettings(settings, project);
        }

        internal ILocanSettings GetProjectSettings(Project project) {
            if (project == null) { throw new ArgumentNullException("project"); }
            string filepath = this.GetPathToLocanSettingsFile(project);
            
            ILocanSettings settings = new LocanSettings();

            if (File.Exists(filepath)) {
                settings = LocanSettings.Load(filepath);
            }

            return settings;
        }

        internal void SaveProjetSettings(ILocanSettings settings, Project project) {
            if (settings == null) { throw new ArgumentNullException("settings"); }
            if (project == null) { throw new ArgumentNullException("project"); }

            // save the file
            string filepath = this.GetPathToLocanSettingsFile(project);
            settings.SaveAs(filepath);

            // add the file to the project
            project.ProjectItems.AddFromFile(filepath);
        }

        internal string GetPathToLocanSettingsFile(Project project) {
            if (project == null) { throw new ArgumentNullException("project"); }

            // the path is {ProjectRoot}\locan.settings
            FileInfo projectFileInfo = new FileInfo(project.FullName);
            string settingsPath = Path.Combine(projectFileInfo.DirectoryName, Consts.LocanSettingsFilename);
            return settingsPath;
        }

        internal void Translate(string apiKey, Project project, ProjectItem selectedItem, EnvDTE80.DTE2 dte) {
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }
            if (project == null) { throw new ArgumentNullException("project"); }
            if (selectedItem == null) { throw new ArgumentNullException("selectedItem"); }
            if (dte == null) { throw new ArgumentNullException("dte"); }

            // get the file path which should be translated
            string filePath = selectedItem.Properties.Item("FullPath").Value.ToString();

            dte.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationGeneral);

            ILocanTranslator bingTranslator = new BingLocanTranslator(apiKey);
            IList<ILocanRow> rowsToTranslate = this.ReadResxFileForTranslation(filePath);

            var stringsToTranslate = from r in rowsToTranslate
                                     select r.StringToTranslate;

            IList<ILanguage> languages = bingTranslator.GetSupportedLanguages().ToList();

            int currentLanguageIndex = 1;
            int totalCountOfLanguages = languages.Count;
            ILanguage sourceLanguage = bingTranslator.DetectLanguage(stringsToTranslate.First());
            foreach (ILanguage destLang in languages) {
                if (this.PreserveUpdates) {
                    // TODO: Look to see if there is an existing file at {Filename}.{language}.resx.cache
                    this.SendTranslationUpdatesToBing(filePath, rowsToTranslate, sourceLanguage, destLang, bingTranslator);
                }

                ProjectItem addedResxProjectItem = null;
                bingTranslator
                    .Translate(stringsToTranslate, destLang, sourceLanguage)
                    .OnTranslationComplete((payload, translations) => {
                        string destFile = this.GetDestFilename(filePath, payload.DestLanguage);
                        this.UpdateProgressBar(destFile, currentLanguageIndex++, totalCountOfLanguages, dte);
                        using (ILocanWriter writer = LocanReaderWriterFactory.Instance.GetWriter(new { filepath = destFile })) {
                            // it is not reliable to use any variables declared outside of this scope
                            // because this is happening async the loop may change the values outside of this scope
                            int currentIndex = 0;
                            foreach (ITranslation translation in translations) {
                                // get source row
                                ILocanRow sourceRow = rowsToTranslate[currentIndex];
                                ILocanRow translatedRow = new LocanRow(id: sourceRow.Id, translatedString: translation.TrnaslatedString);
                                writer.WriteRow(translatedRow);

                                // addedResxProjectItem = this.AddFileToProject(selectedItem, destFile);
                                addedResxProjectItem = this.AddFileToProjectAsChild(selectedItem, destFile);
                                currentIndex++;
                            }
                        }

                        if (this.PreserveUpdates) {
                            // now copy this file to the cache location so that we can compute difference next time.
                            string cacheFile = this.GetDestCacheFilename(filePath, payload.DestLanguage);
                            File.Copy(destFile, cacheFile, true);
                            this.AddFileToProjectAsChild(addedResxProjectItem, cacheFile);
                        }
                    });
            }
        }

        /// <summary>
        /// This method will look at {Filename}.resx and compare it to {Filename}.resx.cache to see if
        /// the user has customized {Filename}.resx and if so it will send the updated values to bing
        /// </summary>
        private void SendTranslationUpdatesToBing(string resxFilePath, IList<ILocanRow> rowsToTranslate, ILanguage sourceLanguage, ILanguage destLanguage, ILocanTranslator translator) {
            if (string.IsNullOrWhiteSpace(resxFilePath)) { throw new ArgumentNullException("resxFile"); }
            if (rowsToTranslate == null) { throw new ArgumentNullException("rowsToTranslate"); }
            if (sourceLanguage == null) { throw new ArgumentNullException("sourceLanguage"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }
            if (translator == null) { throw new ArgumentNullException("translator"); }

            string resxTranslationFile = this.GetDestFilename(resxFilePath, destLanguage);
            FileInfo translatedResxFileInfo = new FileInfo(resxTranslationFile);
            string cacheFile = string.Format("{0}.cache", translatedResxFileInfo.FullName);
            FileInfo cacheFileInfo = new FileInfo(cacheFile);

            if (translatedResxFileInfo.Exists && cacheFileInfo.Exists) {
                IList<ILocanRow> currentTranslatedResxContent = this.ReadResxFileTranslated(translatedResxFileInfo.FullName);

                // convert resxContent to a dictionary
                IDictionary<string, string> translateDictionary = this.ConvertTranslatedStringsToDictionary(currentTranslatedResxContent);
                IList<ILocanRow> cacheContent = this.ReadResxFileTranslated(cacheFileInfo.FullName);

                IDictionary<string, string> customTranslations = new Dictionary<string, string>();
                // now compute the difference
                foreach (ILocanRow cacheRow in cacheContent) {
                    if (!string.IsNullOrWhiteSpace(cacheRow.Id)) {
                        string currentValue = null;
                        translateDictionary.TryGetValue(cacheRow.Id, out currentValue);
                        if (currentValue != null && string.Compare(currentValue, cacheRow.TranslatedString) != 0) {
                            // the values are different must have been customized by the user

                            // we have to look up the original string in rowsToTranslate
                            string originalText = null;
                            foreach (ILocanRow row in rowsToTranslate) {
                                // TODO: Consider making a dictionary for the rowsToTranslate, if there are a bunch of customizations
                                //          then this may take a while.
                                //          But there should not be too many updates because Bing should always have the latest,
                                //          so in 90% of cases the changes should be small.
                                if (string.Compare(cacheRow.Id, row.Id, StringComparison.OrdinalIgnoreCase) == 0) {
                                    originalText = row.StringToTranslate;
                                    break;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(originalText)) {
                                string message = string.Format("Unable to find original text for id: [{0}]", cacheRow.Id);
                                throw new UnexpectedStateException(message);
                            }

                            customTranslations.Add(originalText, currentValue);
                        }
                    }
                }

                if (customTranslations.Count > 0) {
                    translator.AddCustomTranslations(sourceLanguage, destLanguage, customTranslations);
                }
            }
        }

        private IList<ILocanRow> ReadResxFileForTranslation(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException("filePath"); }

            List<ILocanRow> rowsToTranslate = null;

            using (ILocanReader reader = LocanReaderWriterFactory.Instance.GetReader(new { filepath = filePath })) {
                rowsToTranslate = reader.GetRowsToBeTranslated().ToList();
            }

            return rowsToTranslate;
        }

        private IList<ILocanRow> ReadResxFileTranslated(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException("filePath"); }

            List<ILocanRow> rowsToTranslate = null;

            using (ILocanReader reader = LocanReaderWriterFactory.Instance.GetReader(new { filepath = filePath })) {
                rowsToTranslate = reader.GetRowsTranslated().ToList();
            }

            return rowsToTranslate;
        }

        private IDictionary<string, string> ConvertStringsToTraslateToDictionary(IEnumerable<ILocanRow> rows) {
            if (rows == null) { throw new ArgumentNullException("rows"); }

            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (ILocanRow row in rows) {
                result.Add(row.Id, row.StringToTranslate);
            }
            return result;
        }

        private IDictionary<string, string> ConvertTranslatedStringsToDictionary(IEnumerable<ILocanRow> rows) {
            if (rows == null) { throw new ArgumentNullException("rows"); }

            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (ILocanRow row in rows) {
                result.Add(row.Id, row.TranslatedString);
            }
            return result;
        }

        private string GetDestFilename(string sourceFile, ILanguage destLanguage) {
            if (string.IsNullOrEmpty(sourceFile)) { throw new ArgumentNullException("sourceFile"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }
            FileInfo fileInfo = new FileInfo(sourceFile);
            string destFile = string.Format("{0}.{1}{2}",
                            fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length),
                            destLanguage.Language,
                            fileInfo.Extension);

            return destFile;
        }

        private string GetDestCacheFilename(string sourceFile, ILanguage destLanguage) {
            string destResxFile = this.GetDestFilename(sourceFile, destLanguage);
            string destCacheFile = string.Format("{0}.cache", destResxFile);
            return destCacheFile;
        }

        private void UpdateProgressBar(string filename, int currentCount, int overallCount, EnvDTE80.DTE2 dte) {
            if (string.IsNullOrEmpty(filename)) { throw new ArgumentNullException("filename"); }
            if (dte == null) { throw new ArgumentNullException("dte"); }

            string message = string.Format("[{1}/{2}]: Translating into file: {0}", filename, currentCount, overallCount);
            dte.StatusBar.Progress(true, message, currentCount, overallCount);
            if (currentCount >= overallCount) {
                dte.StatusBar.Progress(false);
                dte.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationGeneral);
            }
        }

        private ProjectItem AddFileToProject(ProjectItem selectedItem, string filename) {
            if (selectedItem == null) { throw new ArgumentNullException("selectedItem"); }
            if (string.IsNullOrEmpty(filename)) { throw new ArgumentNullException("filename"); }

            return selectedItem.ContainingProject.ProjectItems.AddFromFile(filename);
        }

        private ProjectItem AddFileToProjectAsChild(ProjectItem parentItem, string fileToAdd) {
            if (parentItem == null) { throw new ArgumentNullException("parentItem"); }
            if (string.IsNullOrWhiteSpace(fileToAdd)) { throw new ArgumentNullException("fileToAdd"); }
            
            ProjectItem result = null;

            if (parentItem.ProjectItems != null) {
                result = parentItem.ProjectItems.AddFromFile(fileToAdd);
            }
            else {
                // cannot do the nesting here for some reason so just add the file itself
                result = parentItem.ContainingProject.ProjectItems.AddFromFile(fileToAdd);
            }

            return result;
        }

        public Guid UpdloadFileForSharing(string filename,string apiKey, string projectName /*,string ownerEmail*/) {
            if (string.IsNullOrWhiteSpace(filename)) { throw new ArgumentNullException("filename"); }
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }
            if (string.IsNullOrWhiteSpace(projectName)) { throw new ArgumentNullException("projectName"); }
            // if (string.IsNullOrWhiteSpace(ownerEmail)) { throw new ArgumentNullException("ownerEmail"); }
            if (!File.Exists(filename)) {
                throw new FileNotFoundException(filename);
            }

            string baseUrl = Settings.Default.LocanServiceBaseUrl;

            string urlForSharing = string.Format(
                "{0}/{1}",
                baseUrl,
                Consts.UrlAddPhrasesForTranslation);

            Guid userId = this.GetUserIdForApiKey(apiKey);
            LocanWebFile webFile = this.GetTranslationFile(apiKey, filename, userId, projectName);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            byte[]bytes = Encoding.UTF8.GetBytes(jsonSerializer.Serialize(webFile));

            

            using (HttpClient client = new HttpClient())
            using(MemoryStream stream = new MemoryStream(bytes)){
                StreamContent streamContent = new StreamContent(stream);
                HttpResponseMessage response = client.Post(urlForSharing, streamContent);
                response.EnsureSuccessStatusCode();
                string guidString = response.Content.ReadAsString();
                // Result looks like: <?xml version="1.0" encoding="utf-8"?><guid>2158e8e5-ae6c-4b9a-a7ab-3169fff9750d</guid>
                XDocument doc = XDocument.Parse(guidString);
                return new Guid(doc.Root.Value);
            }
        }

        private Guid GetUserIdForApiKey(string apiKey) {
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }

            string baseUrl = Settings.Default.LocanServiceBaseUrl;
            string url = string.Format(
                "{0}/{1}/{2}",
                baseUrl,
                Consts.UrlRegiserUser,
                apiKey);

            var textHeader = new MediaTypeWithQualityHeaderValue(Consts.HeaderText);
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Accept.Add(textHeader);
                HttpResponseMessage response = client.Get(url);
                response.EnsureSuccessStatusCode();
                string guidString = response.Content.ReadAsString();
                // Result looks like: <?xml version="1.0" encoding="utf-8"?><guid>2158e8e5-ae6c-4b9a-a7ab-3169fff9750d</guid>
                XDocument doc = XDocument.Parse(guidString);
                Guid id = new Guid(doc.Root.Value);
                return id;
            }
        }

        internal LocanWebFile GetTranslationFile(string apiKey, string filename,Guid userId,string projectName) {
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }
            if (string.IsNullOrWhiteSpace(filename)) { throw new ArgumentNullException("filename"); }
            if (string.IsNullOrWhiteSpace(projectName)) { throw new ArgumentNullException("projectName"); }
            if (!File.Exists(filename)) {
                throw new FileNotFoundException("File passed to GetTranslationFile not found", filename);
            }

            List<LocanRow> rows = new List<LocanRow>();
            using (ILocanReader reader = LocanReaderWriterFactory.Instance.GetReader(new { filepath = filename })) {
                reader.GetRowsToBeTranslated().ToList().ForEach(row => {
                    rows.Add(row as LocanRow);
                });
            }

            LocanWebFile file = new LocanWebFile {
                ApiKey = apiKey,
                Rows=rows,
                Filename=filename,
                UserId = userId,
                ProjectName = projectName
            };

            return file;
        }

        internal static class Consts {
            public const string LocanSettingsFilename = "locan.xml";
            public const string UrlAddPhrasesForTranslation = @"Service/AddPhrasesForTranslation";
            public const string UrlRegiserUser = @"Service/RegisterUser";
            public const string HeaderText = @"text/plain";
        }
    }
}
