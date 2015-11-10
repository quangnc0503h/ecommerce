using Quang.Auth.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using Quang.Auth.Api.Dto;
using System.Web;
using Quang.Auth.Api.BusinessLogic;
using StackExchange.Exceptional;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : BaseApiController
    {
        
        private readonly IUserBll _userBll;
        private readonly IPermissionBll _permissionBll;
        private readonly ILoginHistoryBll _loginHistoryBll;

     
        public UserController()
        {
        }

        public UserController(IUserBll userBll, IPermissionBll permissionBll, ILoginHistoryBll loginHistoryBll)
        {
            _userBll = userBll;
            _permissionBll = permissionBll;
            _loginHistoryBll = loginHistoryBll;
        }

        [Route("GetAll")]
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        public async Task<DanhSachUserOutput> GetAll(FilterUserInput filter)
        {
            return await _userBll.GetAll(filter);
        }

        [Route("GetOneUser")]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [HttpPost]
        public async Task<GetOneUserOutput> GetOneUser(GetOneUserInput input)
        {
            return await _userBll.GetOneUser(input.Id, true);
        }

        [HttpPost]
        [Authorize]
        [Route("GetCurrentUser")]
        public async Task<GetOneUserOutput> GetCurrentUser()
        {
            int userId = this.User.Identity.GetUserId<int>();
            GetOneUserOutput user = await this._userBll.GetOneUser(userId.ToString());
            return user;
        }

        [Route("CreateUser")]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [HttpPost]
        public async Task<CreateUserOutput> CreateUser(CreateUserInput input)
        {
            CreateUserOutput result = await _userBll.CreateUser(input);
            if (result.Status == 0)
            {
                int num = await this._permissionBll.GenerateRolesForUser(input.Id);
            }
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("CreateMobileUser")]
        public async Task<CreateMobileUserOutput> CreateMobileUser(CreateMobileUserInput input)
        {
            CreateMobileUserOutput result = await _userBll.CreateMobileUser(input);
            if (result.Status == 0)
            {
                ApplicationUser user = await UserManager.FindByNameAsync(input.Mobile);
                if (user != null)
                {
                    int num = await _permissionBll.GenerateRolesForUser(user.Id);
                }
            }
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("SetMobilePassword")]
        public async Task<SetMobilePasswordOutput> SetMobilePassword(SetMobilePasswordInput input)
        {
            SetMobilePasswordOutput result = await this._userBll.SetMobilePassword(input);
            if (result.Status == 0)
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.Success, this.User.Identity.GetUserName(), input.Mobile, (string)null, (string)null);
            else
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, this.User.Identity.GetUserName(), input.Mobile, (string)null, (string)null);
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("CreateListUsers")]
        public async Task<int> CreateListUsers(List<CreateUserInput> inputs)
        {
            foreach (CreateUserInput input in inputs)
            {
                ApplicationUser user = await this.UserManager.FindByNameAsync(input.UserName);
                if (user == null)
                {
                    CreateUserOutput result = await _userBll.CreateUser(input);
                    if (result.Status == 0)
                    {
                        int num = await _permissionBll.GenerateRolesForUser(input.Id);
                    }
                }
            }
            return 0;
        }

        [HttpPost]
        [Route("UpdateUser")]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        public async Task<UpdateUserOutput> UpdateUser(UpdateUserInput input)
        {
            try
            {
                var result = await _userBll.UpdateUser(input, true);
                if (result.Status == 0)
                {
                    await _permissionBll.GenerateRolesForUser(input.Id);
                    if (!string.IsNullOrEmpty(input.Password))
                        await LogHistory(LoginType.ChangePassword, LoginStatus.Success, User.Identity.GetUserName(), input.UserName, null, null);
                }
                else if (result.Status != 0 && !string.IsNullOrEmpty(input.Password))
                    await LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, User.Identity.GetUserName(), input.UserName, null, null);
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new UpdateUserOutput();
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateCurrentUser")]
        public async Task<UpdateUserOutput> UpdateCurrentUser(UpdateUserInput input)
        {
            input.Id = User.Identity.GetUserId<int>();
            input.Password = null;
            input.ConfirmPassword = null;
            input.UserGroups = null;
            UpdateUserOutput result = await _userBll.UpdateUser(input);
            return result;
        }

        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
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

        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
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
        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        [Route("UpdateUserClientApp")]
        public async Task<ResultUpdateOutput> UpdateUserClientApp(UpdateUserAppInput input)
        {
            ResultUpdateOutput result = await _userBll.UpdateUserApp(input, AppApiType.ClientApi);
            return result;
        }

        [Route("GenerateUserAppApiKey")]
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        public async Task<GenerateUserAppApiKeyOutput> GenerateUserAppApiKey()
        {
            return await Task.FromResult( new GenerateUserAppApiKeyOutput()
            {
                ApiKey = ClientApiProvider.GenerateApiKey(),
                ApiSecret = ClientApiProvider.GenerateApiSecret()
            });
        }

        private async Task LogHistory(LoginType loginType, LoginStatus status, string ownerUsername, string username, string device, string apiKey)
        {
            string clientIp = SecurityUtils.GetClientIPAddress();
            string clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            int num = await _loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput
                                                                {
                Type = loginType.GetHashCode(),
                UserName = username,
                LoginTime = DateTime.Now,
                LoginStatus = status.GetHashCode(),
                AppId = string.IsNullOrEmpty(ownerUsername) ? null : ownerUsername,
                ClientUri = clientUri,
                ClientIP = clientIp,
                ClientUA = HttpContext.Current.Request.UserAgent,
                ClientApiKey = apiKey,
                ClientDevice = device
            });
        }
    }
}
