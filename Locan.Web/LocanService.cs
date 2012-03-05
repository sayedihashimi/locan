namespace Locan.Web {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Microsoft.ApplicationServer.Http;

    [ServiceContract]
    public class LocanService {

        //public long GetId() {
        //}

        //[WebInvoke(UriTemplate = "RegisterUser/{userId}/{ownerEmail}")]
        //public Guid RegisterUser(Guid userId,string ownerEmail){
        //    return Guid.NewGuid();
        //}

        //[WebInvoke(UriTemplate="GetUserId/{apiKey}")]
        //public Guid GetUserId(string apiKey) {
        //    return Guid.NewGuid();
        //}

        //[WebInvoke(UriTemplate = "GetIdForSharing/{apiKey}/{user}/{projectName}/{fileName}/{ownerEmail}", Method = CommonConsts.Get)]
        //public Guid GetIdForSharing(string apiKey, string user, string projectName, string fileName) {
        //    if (string.IsNullOrEmpty(apiKey)) { throw new ArgumentNullException("apiKey"); }
        //    if (string.IsNullOrEmpty(user)) { throw new ArgumentNullException("user"); }
        //    if (string.IsNullOrEmpty(projectName)) { throw new ArgumentNullException("projectName"); }
        //    if (string.IsNullOrEmpty(fileName)) { throw new ArgumentNullException("fileName"); }

        //    return Guid.NewGuid();
        //}

        //[WebInvoke(UriTemplate="AddPhrasesForTranslation",Method=CommonConsts.Post)]
        //public void AddPhrasesForTranslation(Guid userId) {

        //}

    }
}