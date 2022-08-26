using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using ElectroShop.App_Start;
using System.Web.SessionState;

namespace ElectroShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configuration.EnsureInitialized();
        }
        protected void Session_Start()
        {
            Session["Notification"] = "";
            Session["Message"] = "";
            
            // Administrators
            Session["Admin_Name"] = null;
            Session["Admin_ID"] = null;
            Session["Admin_Images"] = null;
            Session["Admin_Address"] = null;
            Session["Admin_Email"] = null;
            Session["Admin_Created_at"] = null;
            // Customer
            Session["User_Name"] = null;
            Session["User_ID"] = null;
            Session["User_Images"] = null;


            Session["Cart"] = null;
            Session["keywords"] = null;
            Session["Status"] = null;
        }
 /*       protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

        private bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(WebApiConfig.UrlPrefixRelative);
        }*/
    }
}
