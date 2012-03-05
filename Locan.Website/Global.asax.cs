namespace Locan.Website
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;
    using Locan.Website.Models.Data;
    using Locan.Website.Services;
    using Microsoft.ApplicationServer.Http.Activation;
    using Microsoft.ApplicationServer.Http.Description;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            // routes.IgnoreRoute("Service/{*pathInfo}");

            routes.MapRoute("Crowdsource", "crowdsource/{fileid}", new { controller = "crowdsource", action = "Index" });
            routes.MapRoute("CrowdsourceSave", "crowdsource/save/{fileid}", new { controller = "crowdsource", action = "Save" });
            routes.MapRoute("Translate", "translate/{isocode}/{fileid}", new { controller = "translate", action = "Index" });
            routes.MapRoute("TranslateSave", "translate/save/{isocode}/{fileid}", new { controller = "translate", action = "save" });

            InitalizeWebApi();
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            

        }

        private static void InitalizeWebApi()
        {
            // System.Data.Entity.Database.SetInitializer(new LocanDataContext.DbInitalizer());
            using (LocanDataContext ctx = new LocanDataContext())
            {

            }

            RouteTable.Routes.MapServiceRoute<LocanService>("Service");
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}