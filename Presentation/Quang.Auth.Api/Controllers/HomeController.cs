using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackExchange.Exceptional;
using StackExchange.Exceptional.MySQL;

namespace Quang.Auth.Api.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
          
           
            return View();
        }
        public ActionResult Exceptions()
        {
            var context = System.Web.HttpContext.Current;
            var page = new HandlerFactory().GetHandler(context, Request.RequestType, Request.Url.ToString(), Request.PathInfo);
            page.ProcessRequest(context);

            return null;
        }
    }
}
