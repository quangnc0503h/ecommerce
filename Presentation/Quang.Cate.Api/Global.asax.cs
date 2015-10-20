using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using StackExchange.Exceptional;

namespace Quang.Cate.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            SetupErrorStore();
        }
        private void SetupErrorStore()
        {


            ErrorStore.jQueryURL = "//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js";

            //ErrorStore.AddJSInclude("~/Content/errors.js");
            ErrorStore.OnBeforeLog += (sender, args) =>
            {
                args.Error.Message += " - This was appended in the OnBeforeLog handler.";
                //args.Abort = true; - you could stop the exception from being logged here
            };
            ErrorStore.OnAfterLog += (sender, args) =>
            {

                // optionally var e = args.GetError() to fetch the actual error from the store
            };
        }

        /// <summary>
        /// Example method to log an exception to the log...that' not shown to the user.
        /// </summary>
        /// <param name="e">The exception to log</param>
        public static void LogException(Exception e)
        {
            // Note: When dealing with non-web applications, or logging from background threads, 
            // you would pass, null in instead of a HttpContext object.
            ErrorStore.LogException(e, HttpContext.Current);
        }
    }
}
