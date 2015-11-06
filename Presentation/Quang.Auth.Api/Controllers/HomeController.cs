using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StackExchange.Exceptional;
using StackExchange.Exceptional.MySQL;
using Microsoft.AspNet.Identity.Owin;

namespace Quang.Auth.Api.Controllers
{
    //[WebAuthorize(Roles = "001")]
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return this._userManager ?? OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContextBaseExtensions.GetOwinContext(this.Request));
            }
            private set
            {
                this._userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return this._roleManager ?? OwinContextExtensions.GetUserManager<ApplicationRoleManager>(HttpContextBaseExtensions.GetOwinContext(this.Request));
            }
            private set
            {
                this._roleManager = value;
            }
        }

        public HomeController()
        {
        }

        public HomeController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public ActionResult Index()
        {
           
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
           
            return (ActionResult)this.View();
        }
    }
    //[Authorize]
  
}
