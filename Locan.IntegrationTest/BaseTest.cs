namespace Locan.IntegrationTest {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using Locan.Translate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class BaseTest {
        protected IList<string> FilesToDeleteAfterTest { get; set; }

        public BingLocanTranslator GetNewBingLocanTranslator() {
            return new BingLocanTranslator(this.BingApiKey);
        }

        protected string BingApiKey {
            get {
                return ConfigurationManager.AppSettings[Consts.BingApiKey];
            }
        }


        [TestInitialize]
        public virtual void SetupFilesToDeleteList() {
            this.FilesToDeleteAfterTest = new List<string>();
        }

        [TestCleanup]
        public virtual void CleanUpFilesToDeleteList() {
            if (this.FilesToDeleteAfterTest != null && this.FilesToDeleteAfterTest.Count > 0) {
                foreach (string filename in this.FilesToDeleteAfterTest) {
                    if (File.Exists(filename)) {
                        File.Delete(filename);
                    }
                }
            }

            this.FilesToDeleteAfterTest = null;
        }

        protected virtual string WriteTextToTempFile(string content,string fileExtension = null) {
            if (string.IsNullOrEmpty(content)) { throw new ArgumentNullException("content"); }

            string tempFile = this.GetTempFilename(true,fileExtension);
            File.WriteAllText(tempFile, content);
            return tempFile;
        }
       
        protected virtual string GetTempFilename(bool ensureFileDoesntExist,string extension = null) {
            string path = Path.GetTempFileName();

            if (!string.IsNullOrWhiteSpace(extension)) {
                // delete the file at path and then add the extension to it
                if (File.Exists(path)) {
                    File.Delete(path);

                    extension = extension.Trim();
                    if (!extension.StartsWith(".")) {
                        extension = "." + extension;
                    }

                    path += extension;
                }
            }
            
            if (ensureFileDoesntExist && File.Exists(path)) {
                File.Delete(path);
            }

            this.FilesToDeleteAfterTest.Add(path);
            return path;
        }

        private class Consts {
            public const string BingApiKey = @"BingApiKey";
        }
    }
}
