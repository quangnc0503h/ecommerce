using Quang.Auth.Api.Models;
using Quang.Auth.BusinessLogic;
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

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("GetAll")]
        public async Task<DataSourceResultModel> GetAll(FilterUserModel filter)
        {
            try
            {
                var model = new DataSourceResultModel
                {
                    Total = await UserBll.GetTotal(filter.GroupId,filter.Keyword),
                    Data = await UserBll.GetPaging(filter.PageSize, filter.PageNumber, filter.GroupId, filter.Keyword)
                };
                return model;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("GetOneUser")]
        public async Task<UserModel> GetOneUser(GetOneInputModel input)
        {
            try
            {
                var result = await UserBll.GetOneUser(input.Id);
                return new UserModel { Id = result.Id, UserName = result.UserName, DisplayName = result.DisplayName, UserGroups = result.UserGroups, Email = result.Email, PhoneNumber = result.PhoneNumber };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new UserModel();
            }
            
        }

        [HttpPost]
        [Authorize]
        [Route("GetCurrentUser")]
        public async Task<UserModel> GetCurrentUser()
        {
            try
            {
                var userId =  User.Identity.GetUserId<long>();
                var result = await UserBll.GetOneUser(userId);
                return new UserModel { Id = result.Id, UserName = result.UserName, DisplayName = result.DisplayName, UserGroups = result.UserGroups, Email = result.Email, PhoneNumber = result.PhoneNumber };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new UserModel();
            }
                        
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("CreateUser")]
        public async Task<NotificationResultModel> CreateUser(UserModel input)
        {
            try
            {
                var entity = new Quang.Auth.Entities.User
                {
                    UserName = input.UserName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                    DisplayName = input.DisplayName,
                    PasswordHash = input.PasswordHash,
                    Password= input.Password
                };
                var result = await UserBll.CreateUser(entity);
                if (result == 0)
                {
                    //await _termBll.ReUpdateUserRole(input.Id);
                    await PermissionBll.GenerateRolesForUser(input.Id);
                }
                return new NotificationResultModel { Status = (int)result };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
        
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("UpdateUser")]
        public async Task<NotificationResultModel> UpdateUser(UserModel input)
        {
            try
            {
                var result = new NotificationResultModel { Status = 1 };
                var entity = new Quang.Auth.Entities.User
                {
                    UserName = input.UserName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                    DisplayName = input.DisplayName,
                    PasswordHash = input.PasswordHash,
                    Password = input.Password
                };
                result.Status = await UserBll.UpdateUser(entity, true);
                if (result.Status == 0)
                {
                    //await _termBll.ReUpdateUserRole(input.Id);
                    await PermissionBll.GenerateRolesForUser(input.Id);
                }
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
           
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateCurrentUser")]
        public async Task<NotificationResultModel> UpdateCurrentUser(UserModel input)
        {
            try
            {
                var result = new NotificationResultModel { Status = 1 };
                //input.Id = User.Identity.GetUserId<int>();
                //input.Password = null;
                //input.ConfirmPassword = null;
                //input.UserGroups = null;
                var entity = new Quang.Auth.Entities.User
                {
                    UserName = input.UserName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                    DisplayName = input.DisplayName,
                    PasswordHash = input.PasswordHash,
                    Password = input.Password,
                    Id = User.Identity.GetUserId<int>()
                };
                result.Status = await UserBll.UpdateUser(entity, true);
                // var result = await _userBll.UpdateUser(input);

                return result;
                
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
           
        }
        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("DeleteUser")]
        public async Task<NotificationResultModel> DeleteUser(DeleteInputModel input)
        {
            try
            {
                var result = new NotificationResultModel { Status = 1 };
                result.Status = await UserBll.DeleteUser(input.Ids);

                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
         
        }

        [HttpPost]
        //[Authorize]
        [Route("CheckUserName")]
        public async Task<CheckUserExistOutput> CheckUserName(CheckUserExistInput input)
        {
            try
            {
                return new CheckUserExistOutput { Check= await UserBll.CheckExistUserName(input.UserName, input.Id) };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new CheckUserExistOutput();
            }
            
        }

        [HttpPost]
        //[Authorize]
        [Route("CheckEmail")]
        public async Task<CheckUserExistOutput> CheckEmail(CheckUserExistInput input)
        {
            try
            {
                return new CheckUserExistOutput { Check = await UserBll.CheckExistEmail(input.Email, input.Id) };
                
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new CheckUserExistOutput();
            }
            
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        [Route("GetUserClientApp")]
        public async Task<UserAppModel> GetUserClientApp(GetOneInputModel input)
        {
            var result = new UserAppModel();
            try
            {
                var entity = await UserBll.GetUserApp(input.Id, AppApiType.ClientApi);
                result.Id = entity.Id;
                result.IsActive = entity.IsActive;
                result.UserId = entity.UserId;
                result.AppIps = entity.AppIps;
                result.AppHosts = entity.AppHosts;
                result.ApiSecret = entity.ApiSecret;
                result.ApiName = entity.ApiName;
                result.ApiKey = entity.ApiKey;
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return result;
            }
           
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        [Route("UpdateUserClientApp")]
        public async Task<NotificationResultModel> UpdateUserClientApp(UserAppModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var entity = new UserApp();
                entity.Id = input.Id;
                entity.IsActive = input.IsActive;
                entity.UserId = input.UserId;
                entity.AppIps = input.AppIps;
                entity.AppHosts = input.AppHosts;
                entity.ApiSecret = input.ApiSecret;
                entity.ApiName = input.ApiName;
                entity.ApiKey = input.ApiKey;
                entity.ApiType = AppApiType.ClientApi;
                result.Status =(int) await UserBll.UpdateUserApp(entity); 

                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
            
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        [Route("GenerateUserAppApiKey")]
        public async Task<GenerateUserAppApiKeyModel> GenerateUserAppApiKey()
        {
            var result = new GenerateUserAppApiKeyModel();
            result.ApiKey = ClientApiProvider.GenerateApiKey();
            result.ApiSecret = ClientApiProvider.GenerateApiSecret();

            return result;
        }
    }
}
