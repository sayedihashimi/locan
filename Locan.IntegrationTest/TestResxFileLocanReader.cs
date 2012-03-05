namespace Locan.IntegrationTest {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Locan.Translate.IO;
    using System.Collections;
    using System.Resources;

    [TestClass]
    public class TestResxFileLocanReader : BaseTest {
        [TestMethod]
        public void TestReader01() {
            string filepath = this.WriteTextToTempFile(Consts.ResxSample01);

            using (ILocanReader reader = new ResxFileLocanReader(filepath)) {

                Dictionary<string, string> expectedValues = new Dictionary<string, string>();
                expectedValues.Add("Key01", "Value01");
                expectedValues.Add("Key02", "Value02");
                expectedValues.Add("Key03", "Value03");

                int currentIndex = 0;

                foreach (ILocanRow row in reader.GetRowsToBeTranslated()) {
                    string expectedKey = expectedValues.ElementAt(currentIndex).Key;
                    string expectedValue = expectedValues.ElementAt(currentIndex).Value as string;

                    Assert.AreEqual(expectedKey, row.Id);
                    Assert.AreEqual(expectedValue, row.StringToTranslate);

                    currentIndex++;
                }
            }
        }

        [TestMethod]
        public void TestReader_FromFactory() {
            string filepath = this.WriteTextToTempFile(Consts.ResxSample01, ".resx");

            using (ILocanReader reader = LocanReaderWriterFactory.Instance.GetReader(new { filepath = filepath })) {

                Dictionary<string, string> expectedValues = new Dictionary<string, string>();
                expectedValues.Add("Key01", "Value01");
                expectedValues.Add("Key02", "Value02");
                expectedValues.Add("Key03", "Value03");

                int currentIndex = 0;

                foreach (ILocanRow row in reader.GetRowsToBeTranslated()) {
                    string expectedKey = expectedValues.ElementAt(currentIndex).Key;
                    string expectedValue = expectedValues.ElementAt(currentIndex).Value as string;

                    Assert.AreEqual(expectedKey, row.Id);
                    Assert.AreEqual(expectedValue, row.StringToTranslate);

                    currentIndex++;
                }
            }
        }

        [TestMethod]
        public void TestWriter01() {
            List<ILocanRow> expectedValues = new List<ILocanRow> {
                new LocanRow(id:"Key01",translatedString:"Value01"),
                new LocanRow(id:"Key02",translatedString:"Value02"),
                new LocanRow(id:"Key03",translatedString:"Value03"),
            };

            string filepath = this.GetTempFilename(true, ".resx");
            using (ResxFileLocanWriter writer = new ResxFileLocanWriter(filepath)) {
                writer.WriteRows(expectedValues);
            }

            // now we need to read that file
            using (ResXResourceReader reader = new ResXResourceReader(filepath)) {
                int currentIndex = 0;
                foreach (DictionaryEntry de in reader) {
                    string expectedKey = expectedValues[currentIndex].Id;
                    string expectedValue = expectedValues[currentIndex].TranslatedString;

                    Assert.AreEqual(expectedKey, de.Key);
                    Assert.AreEqual(expectedValue, de.Value);

                    currentIndex++;
                }
            }
        }

        [TestMethod]
        public void TestWriter_FromFactory() {
            List<ILocanRow> expectedValues = new List<ILocanRow> {
                new LocanRow(id:"Key01",translatedString:"Value01"),
                new LocanRow(id:"Key02",translatedString:"Value02"),
                new LocanRow(id:"Key03",translatedString:"Value03"),
            };

            string filepath = this.GetTempFilename(true, ".resx");
            using (ILocanWriter writer = LocanReaderWriterFactory.Instance.GetWriter(new { filepath = filepath })) {
                writer.WriteRows(expectedValues);
            }

            // now we need to read that file
            using (ResXResourceReader reader = new ResXResourceReader(filepath)) {
                int currentIndex = 0;
                foreach (DictionaryEntry de in reader) {
                    string expectedKey = expectedValues[currentIndex].Id;
                    string expectedValue = expectedValues[currentIndex].TranslatedString;

                    Assert.AreEqual(expectedKey, de.Key);
                    Assert.AreEqual(expectedValue, de.Value);

                    currentIndex++;
                }
            }

        }

        private class Consts {
            public const string ResxSample01 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
    <xsd:element name=""root"" msdata:IsDataSet=""true"">
      <xsd:complexType>
        <xsd:choice maxOccurs=""unbounded"">
          <xsd:element name=""metadata"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
              <xsd:attribute name=""type"" type=""xsd:string"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""assembly"">
            <xsd:complexType>
              <xsd:attribute name=""alias"" type=""xsd:string"" />
              <xsd:attribute name=""name"" type=""xsd:string"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""data"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
                <xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
              <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""resheader"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
    <value>2.0</value>
  </resheader>
  <resheader name=""reader"">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""Key01"" xml:space=""preserve"">
    <value>Value01</value>
    <comment>ccc</comment>
  </data>
  <data name=""Key02"" xml:space=""preserve"">
    <value>Value02</value>
    <comment>dd</comment>
  </data>
  <data name=""Key03"" xml:space=""preserve"">
    <value>Value03</value>
    <comment>ddd</comment>
  </data>
</root>";
        }
    }
}
