namespace Locan.Website.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Microsoft.ApplicationServer.Http;
    using System.Net.Http;
    using Locan.Website.Models.Data;
    using Locan.Translate;

    [ServiceContract]
    public class LocanService {
        [WebInvoke(UriTemplate = "GetId", Method = CommonConsts.Get)]
        public long GetId() {
            return 1234;
        }

        [WebInvoke(UriTemplate = "RegisterUser/{apiKey}", Method = CommonConsts.Get)]
        public Guid RegisterUser(string apiKey) {
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }
            
            User newUser = new User {
                ApiKey = apiKey
            };

            // see if the user exists if so get that ID if not create a new one
            LocanDal dal = new LocanDal();

            User existingUser = dal.GetUniqueUser(user => user.ApiKey == apiKey, false);
            if (existingUser == null) {
                existingUser = dal.AddUser(newUser);
            }
            
            return existingUser.Id;
        }

        [WebInvoke(UriTemplate = "GetUserId/{apiKey}", Method = CommonConsts.Get)]
        public Guid GetUserId(string apiKey) {
            if (string.IsNullOrWhiteSpace(apiKey)) { throw new ArgumentNullException("apiKey"); }
            LocanDal dal = new LocanDal();
            User user = dal.GetUniqueUser(u => string.Compare(apiKey, u.ApiKey, StringComparison.OrdinalIgnoreCase) == 0, true);
            return user.Id;
        }

        [WebInvoke(UriTemplate = "GetFileIdForSharing/{userId}/{projectName}/{filename}", Method = CommonConsts.Get)]
        public Guid GetFileIdForSharing(string userId, string projectName, string filename) {
            if (string.IsNullOrWhiteSpace(userId)) { throw new ArgumentNullException("userId"); }
            if (string.IsNullOrWhiteSpace(projectName)) { throw new ArgumentNullException("projectName"); }
            if (string.IsNullOrWhiteSpace(filename)) { throw new ArgumentNullException("filename"); }

            // TODO: See if there is already an id for this combo if so get & return it, if not create one
            return Guid.NewGuid();
        }

        [WebInvoke(UriTemplate = "GetFileUrlForEditing/{fileGuid}", Method = CommonConsts.Get)]
        public string GetFileUrlForEditing(string fileGuid) {
            // TODO: Return the URL which the user can share with friends for localization

            return "some url";
        }

        [WebInvoke(UriTemplate = "AddPhrasesForTranslation", Method = CommonConsts.Post)]
        public Guid AddPhrasesForTranslation(HttpRequestMessage request) {
            // From the POST message we need to pick off the following properties
            //  UserId
            //  FileId
            //  Array of TranslateKey / StringToTranslate / Comment

            string fileContents = request.Content.ReadAsString();
            LocanWebFile webFile = LocanWebFile.BuildFrom(fileContents);
            LocanDal dal = new LocanDal();
            Guid fileId = dal.GetOrCreateIdForFile(webFile.UserId, webFile.Filename, webFile.ProjectName);

            // delete all old phrases
            dal.DeleteAllPhrasesfor(fileId);
            
            // now add the phrases
            dal.AddPhrasesToFile(fileId, webFile.Rows);

            return fileId;
        }

    }

}