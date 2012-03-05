using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Locan.Website.Models;
using System.Net.Mail;
using Locan.Website.Models.Data;
namespace Locan.Website.Controllers
{
    public class CrowdSourceController : Controller
    {
        public ViewResult Index(Guid fileId)
        {
            var dal = new LocanDal();
            var apiKey = dal.GetUserByFileId(fileId).ApiKey;

            return View(Language.GetLanguages(apiKey));
        }

        [HttpPost]
        public ActionResult Save(Guid fileId)
        {
            List<Language> languages = new List<Language>();
            foreach (string key in Request.Form)
            {
                string[] emails = Request.Form[key].Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var language = new Language() { IsoCode = key, Emails = emails };
                languages.Add(language);
                SendMail("http://" + Request.Url.Authority + "/translate/" + key + "/" + fileId, emails);
            }

            return View("Done");
        }

        private void SendMail(string url, IEnumerable<string> emails)
        {
            foreach (string email in emails)
            {
                using (var mail = new MailMessage())
                {
                    mail.To.Add(email);
                    mail.Subject = "Translate";
                    mail.Body = "Help translate the website \n\n" + url;
                    mail.IsBodyHtml = true;

                    var smtp = new SmtpClient();
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}