using Quang.Auth.Api.Models;

using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System.Security.Claims;
using Quang.Auth.Api.Dto;
using System.Web;
using Quang.Auth.Api.BusinessLogic;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private IUserBll _userBll;
        private ITermBll _termBll;
        private IGroupBll _groupBll;
        private IPermissionBll _permissionBll;
        private ILoginHistoryBll _loginHistoryBll;

        public ApplicationUserManager UserManager
        {
            get
            {
                return this._userManager ?? OwinContextExtensions.GetUserManager<ApplicationUserManager>(OwinHttpRequestMessageExtensions.GetOwinContext(this.Request));
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
                return this._roleManager ?? OwinContextExtensions.GetUserManager<ApplicationRoleManager>(OwinHttpRequestMessageExtensions.GetOwinContext(this.Request));
            }
            private set
            {
                this._roleManager = value;
            }
        }

        public UserController()
        {
        }

        public UserController(IUserBll userBll, ITermBll termBll, IGroupBll groupBll, IPermissionBll permissionBll, ILoginHistoryBll loginHistoryBll)
        {
            this._userBll = userBll;
            this._groupBll = groupBll;
            this._permissionBll = permissionBll;
            this._termBll = termBll;
            this._loginHistoryBll = loginHistoryBll;
        }

        [Route("GetAll")]
        [HttpPost]
        [AppAuthorize(Roles = "100")]
        public async Task<DanhSachUserOutput> GetAll(FilterUserInput filter)
        {
            return await this._userBll.GetAll(filter);
        }

        [Route("GetOneUser")]
        [AppAuthorize(Roles = "100")]
        [HttpPost]
        public async Task<GetOneUserOutput> GetOneUser(GetOneUserInput input)
        {
            return await this._userBll.GetOneUser(input.Id, true);
        }

        [HttpPost]
        [Authorize]
        [Route("GetCurrentUser")]
        public async Task<GetOneUserOutput> GetCurrentUser()
        {
            int userId = IdentityExtensions.GetUserId<int>(this.User.Identity);
            GetOneUserOutput user = await this._userBll.GetOneUser(userId.ToString());
            return user;
        }

        [Route("CreateUser")]
        [AppAuthorize(Roles = "100")]
        [HttpPost]
        public async Task<CreateUserOutput> CreateUser(CreateUserInput input)
        {
            CreateUserOutput result = await this._userBll.CreateUser(input);
            if (result.Status == 0)
            {
                int num = await this._permissionBll.GenerateRolesForUser(input.Id);
            }
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = "100")]
        [Route("CreateMobileUser")]
        public async Task<CreateMobileUserOutput> CreateMobileUser(CreateMobileUserInput input)
        {
            CreateMobileUserOutput result = await this._userBll.CreateMobileUser(input);
            if (result.Status == 0)
            {
                ApplicationUser user = await this.UserManager.FindByNameAsync(input.Mobile);
                if (user != null)
                {
                    int num = await this._permissionBll.GenerateRolesForUser(user.Id);
                }
            }
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = "100")]
        [Route("SetMobilePassword")]
        public async Task<SetMobilePasswordOutput> SetMobilePassword(SetMobilePasswordInput input)
        {
            SetMobilePasswordOutput result = await this._userBll.SetMobilePassword(input);
            if (result.Status == 0)
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.Success, IdentityExtensions.GetUserName(this.User.Identity), input.Mobile, (string)null, (string)null);
            else
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, IdentityExtensions.GetUserName(this.User.Identity), input.Mobile, (string)null, (string)null);
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = "100")]
        [Route("CreateListUsers")]
        public async Task<int> CreateListUsers(List<CreateUserInput> inputs)
        {
            foreach (CreateUserInput input in inputs)
            {
                ApplicationUser user = await this.UserManager.FindByNameAsync(input.UserName);
                if (user == null)
                {
                    CreateUserOutput result = await this._userBll.CreateUser(input);
                    if (result.Status == 0)
                    {
                        int num = await this._permissionBll.GenerateRolesForUser(input.Id);
                    }
                }
            }
            return 0;
        }

        [HttpPost]
        [Route("UpdateUser")]
        [AppAuthorize(Roles = "100")]
        public async Task<UpdateUserOutput> UpdateUser(UpdateUserInput input)
        {
            UpdateUserOutput result = await this._userBll.UpdateUser(input, true);
            if (result.Status == 0)
            {
                int num = await this._permissionBll.GenerateRolesForUser(input.Id);
                if (!string.IsNullOrEmpty(input.Password))
                    await this.LogHistory(LoginType.ChangePassword, LoginStatus.Success, IdentityExtensions.GetUserName(this.User.Identity), input.UserName, (string)null, (string)null);
            }
            else if (result.Status != 0 && !string.IsNullOrEmpty(input.Password))
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, IdentityExtensions.GetUserName(this.User.Identity), input.UserName, (string)null, (string)null);
            return result;
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateCurrentUser")]
        public async Task<UpdateUserOutput> UpdateCurrentUser(UpdateUserInput input)
        {
            input.Id = IdentityExtensions.GetUserId<int>(this.User.Identity);
            input.Password = (string)null;
            input.ConfirmPassword = (string)null;
            input.UserGroups = (IEnumerable<Group>)null;
            UpdateUserOutput result = await this._userBll.UpdateUser(input);
            return result;
        }

        [AppAuthorize(Roles = "100")]
        [Route("DeleteUser")]
        [HttpPost]
        public async Task<DeleteUserOutput> DeleteUser(DeleteUserInput input)
        {
            DeleteUserOutput result = await this._userBll.DeleteUser(input);
            return result;
        }

        [HttpPost]
        [Authorize]
        [Route("CheckUserName")]
        public async Task<CheckUserExistOutput> CheckUserName(CheckUserExistInput input)
        {
            return await this._userBll.CheckExistUserName(input.UserName, input.Id);
        }

        [HttpPost]
        [Authorize]
        [Route("CheckEmail")]
        public async Task<CheckUserExistOutput> CheckEmail(CheckUserExistInput input)
        {
            return await this._userBll.CheckExistEmail(input.Email, input.Id);
        }

        [AppAuthorize(Roles = "150")]
        [HttpPost]
        [Route("GetUserClientApp")]
        public async Task<GetOneUserAppOutput> GetUserClientApp(GetByIdInput input)
        {
            return new GetOneUserAppOutput()
            {
                UserApp = await this._userBll.GetUserApp(input.Id, AppApiType.ClientApi)
            };
        }

        [HttpPost]
        [AppAuthorize(Roles = "150")]
        [Route("UpdateUserClientApp")]
        public async Task<ResultUpdateOutput> UpdateUserClientApp(UpdateUserAppInput input)
        {
            ResultUpdateOutput result = await this._userBll.UpdateUserApp(input, AppApiType.ClientApi);
            return result;
        }

        [Route("GenerateUserAppApiKey")]
        [HttpPost]
        [AppAuthorize(Roles = "150")]
        public async Task<GenerateUserAppApiKeyOutput> GenerateUserAppApiKey()
        {
            return new GenerateUserAppApiKeyOutput()
            {
                ApiKey = ClientApiProvider.GenerateApiKey(),
                ApiSecret = ClientApiProvider.GenerateApiSecret()
            };
        }

        private async Task LogHistory(LoginType loginType, LoginStatus status, string ownerUsername, string username, string device, string apiKey)
        {
            string clientIp = SecurityUtils.GetClientIPAddress();
            string clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            int num = await this._loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput()
            {
                Type = loginType.GetHashCode(),
                UserName = username,
                LoginTime = DateTime.Now,
                LoginStatus = status.GetHashCode(),
                AppId = string.IsNullOrEmpty(ownerUsername) ? (string)null : ownerUsername,
                ClientUri = clientUri,
                ClientIP = clientIp,
                ClientUA = HttpContext.Current.Request.UserAgent,
                ClientApiKey = apiKey,
                ClientDevice = device
            });
        }
    }
}
