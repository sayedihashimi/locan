namespace Locan.IntegrationTest {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Locan.Translate;

    [TestClass]
    public class TestLocanSettings : BaseTest {

        [TestMethod]
        public void TestReadXml_4Languages() {
            // read the file from a file and then make sure the values are all correct
            string filepath = this.WriteTextToTempFile(Consts.SampleXml01);

            ILocanSettings settings = LocanSettings.Load(filepath);

            string expectedApiKey = @"864A5AE2-970A-47F1-94F7-CB9B02852D6E";
            bool expectedPreserveUpdates = true;
            IList<ILanguage> expectedLanguages = new List<ILanguage> {
                    new BaseLanguage("en"),
                    new BaseLanguage("pt"),
                    new BaseLanguage("pr"),
                    new BaseLanguage("vi")
                };

            Assert.AreEqual(expectedApiKey, settings.DefaultApiKey);
            Assert.AreEqual(expectedPreserveUpdates, settings.PreserveUpdates);
            CustomAssert.AreEqual<ILanguage>(expectedLanguages, settings.SupportedLanguages, CustomAssert.AreEqual);
        }

        [TestMethod]
        public void TestReadXml_OnlyApiKey() {
            string filepath = this.WriteTextToTempFile(Consts.SampleXmlOnlyDefaultApiKey);
            ILocanSettings settings = LocanSettings.Load(filepath);
            ILocanSettings defaultSettings = new LocanSettings();

            string expectedApiKey = @"5A2172FE-ED63-4708-A462-9041707454FF";
            Assert.AreEqual(expectedApiKey, settings.DefaultApiKey);
            
            Assert.AreEqual(defaultSettings.SupportedLanguages.Count, settings.SupportedLanguages.Count);
            Assert.AreEqual(defaultSettings.PreserveUpdates, settings.PreserveUpdates);
        }

        [TestMethod]
        public void TestWriteXml01_DefaultSupportedLanguages() {
            // write the settings out to a file and then read it back in
            string filepath = this.GetTempFilename(true);

            LocanSettings expectedSettings = new LocanSettings() {
                DefaultApiKey = Guid.NewGuid().ToString(),
                PreserveUpdates = false,
            };

            // write the file out
            expectedSettings.SaveAs(filepath);

            // read the file back in
            ILocanSettings writtenSettings = LocanSettings.Load(filepath);

            Assert.AreEqual(expectedSettings.DefaultApiKey, writtenSettings.DefaultApiKey);
            Assert.AreEqual(expectedSettings.PreserveUpdates, writtenSettings.PreserveUpdates);
            CustomAssert.AreEqual<ILanguage>(expectedSettings.SupportedLanguages, writtenSettings.SupportedLanguages,CustomAssert.AreEqual);
        }

        [TestMethod]
        public void TestWriteXml01_NonDefaultSupportedLanguages() {
            // write the settings out to a file and then read it back in
            string filepath = this.GetTempFilename(true);

            LocanSettings expectedSettings = new LocanSettings() {
                DefaultApiKey = Guid.NewGuid().ToString(),
                PreserveUpdates = false,
            };

            expectedSettings.SupportedLanguages.Add(new BaseLanguage("pt"));
            expectedSettings.SupportedLanguages.Add(new BaseLanguage("en"));
            expectedSettings.SupportedLanguages.Add(new BaseLanguage("vi"));

            // write the file out
            expectedSettings.SaveAs(filepath);

            // read the file back in
            ILocanSettings writtenSettings = LocanSettings.Load(filepath);

            Assert.AreEqual(expectedSettings.DefaultApiKey, writtenSettings.DefaultApiKey);
            Assert.AreEqual(expectedSettings.PreserveUpdates, writtenSettings.PreserveUpdates);
            CustomAssert.AreEqual(expectedSettings.SupportedLanguages, writtenSettings.SupportedLanguages, CustomAssert.AreEqual);
        }

        private static class Consts {
            public const string SampleXml01 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Locan xmlns=""http://schemas.microsoft.com/2011/04/Locan"">
  <Settings DefaultApiKey=""864A5AE2-970A-47F1-94F7-CB9B02852D6E"" PreserveUpdates=""true"" />
  <Languages>
    <Language Name=""en""/>
    <Language Name=""pt""/>
    <Language Name=""pr""/>
    <Language Name=""vi""/>
  </Languages>
</Locan>";
            public const string SampleXmlOnlyDefaultApiKey =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Locan xmlns=""http://schemas.microsoft.com/2011/04/Locan"">
  <Settings DefaultApiKey=""5A2172FE-ED63-4708-A462-9041707454FF"" />
    <Languages />
</Locan>";
        }
    }
}
