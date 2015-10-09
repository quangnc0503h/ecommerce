using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{
    public class BaseApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
             set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();

            }
            set
            {
                _roleManager = value;
            }
        }
    }
}
