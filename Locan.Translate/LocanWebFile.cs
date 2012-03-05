namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Locan.Translate.IO;
    using System.Web.Script.Serialization;

    /// <summary>
    /// This representation should stricly be used for uploading the file content to the web server
    /// </summary>
    [Serializable]
    public class LocanWebFile {
        public string ApiKey { get; set; }

        public List<LocanRow> Rows { get; set; }
        public string ProjectName { get; set; }
        public string Filename { get; set; }
        public Guid UserId { get; set; }

        public byte[] GetAsBtyesForWeb() {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            byte[] result = Encoding.UTF8.GetBytes(jsonSerializer.Serialize(this));
            return result;
        }

        public static LocanWebFile BuildFrom(string stringRepresentation) {
            if (string.IsNullOrWhiteSpace(stringRepresentation)) { throw new ArgumentNullException("stringRepresentation"); }

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            return jsonSerializer.Deserialize(stringRepresentation, typeof(LocanWebFile)) as LocanWebFile;
        }
    }
}
