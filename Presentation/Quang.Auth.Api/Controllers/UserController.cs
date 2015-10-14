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
using System.Security.Claims;

namespace Quang.Auth.Api.Controllers
{
    //[AppAuthorize(Roles =ActionRole.HeThong.Users)]
    [RoutePrefix("api/User")]
    public class UserController : BaseApiController
    {
       
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
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
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("GetOneUser")]
        public async Task<UserModel> GetOneUser(GetOneInputModel input)
        {
            try
            {
                var user = await UserManager.FindByIdAsync((int)input.Id);
                var claims = await UserManager.GetClaimsAsync(user.Id);
                var displayName = claims.FirstOrDefault(m => m.Type == "displayName");
               
                var groups = await UserBll.GetGroupsByUser(input.Id);
                return new UserModel { Id = user.Id, UserName = user.UserName, DisplayName = displayName != null ? displayName.Value : string.Empty, UserGroups = groups, Email = user.Email, PhoneNumber = user.PhoneNumber, HasPassword = !string.IsNullOrEmpty(user.PasswordHash) };
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
                var user = await UserManager.FindByIdAsync((int)userId);
                var claims = await UserManager.GetClaimsAsync(user.Id);
                var displayName = claims.FirstOrDefault(m => m.Type == "displayName");

                var groups = await UserBll.GetGroupsByUser(userId);
                return new UserModel { Id = user.Id, UserName = user.UserName, DisplayName = displayName != null ? displayName.Value : string.Empty, UserGroups = groups, Email = user.Email, PhoneNumber = user.PhoneNumber, HasPassword = !string.IsNullOrEmpty(user.PasswordHash) };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new UserModel();
            }
                        
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("CreateUser")]
        public async Task<NotificationResultModel> CreateUser(UserModel input)
        {
            try
            {
                var result = new NotificationResultModel();
                var user = new ApplicationUser();
                user.Email = input.Email;
                user.UserName = input.UserName;
                user.PhoneNumber = input.PhoneNumber;
                var res = await UserManager.CreateAsync(user);
                result.Status = 1;
                if (res.Succeeded)
                {
                    input.Id = user.Id;
                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        res = await UserManager.AddPasswordAsync(user.Id, input.Password);
                        if (res.Succeeded)
                        {
                            // Update display name
                            res = await UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));

                            // Update groups
                            if (input.UserGroups != null)
                            {
                                foreach (var group in input.UserGroups)
                                {
                                   await UserBll.AddUserToGroup(group.Id, user.Id);
                                }
                            }
                            result.Status = 0;
                        }
                    }
                    else
                    {
                        result.Status = 0;
                    }
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
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("UpdateUser")]
        public async Task<NotificationResultModel> UpdateUser(UserModel input)
        {
            try
            {
                var result = new NotificationResultModel { Status = 1 };
                
                var user = await UserManager.FindByIdAsync((int)input.Id);
                if (user != null)
                {
                    bool canUpdate = true;
                    if (user.UserName != input.UserName)
                    {
                        var currentUser = await UserManager.FindByNameAsync(input.UserName);
                        if (currentUser != null && currentUser.Id > 0 && currentUser.Id != user.Id)
                        {
                            canUpdate = false;
                        }
                    }
                    if (canUpdate)
                    {
                        user.UserName = input.UserName;
                        user.Email = input.Email;
                        user.PhoneNumber = input.PhoneNumber;
                        if (!string.IsNullOrEmpty(input.Password))
                        {
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(input.Password);
                        }
                        var res = await UserManager.UpdateAsync(user);
                        if (res.Succeeded)
                        {
                            // Update display name
                            var scopeClaims = new string[] { "displayName" };
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            foreach (var claim in claims.Where(m => scopeClaims.Contains(m.Type)))
                            {
                                res = await UserManager.RemoveClaimAsync(user.Id, claim);
                            }
                            res = await UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));

                            if (input.UpdateGroups)
                            {
                                // Update groups
                                var currentGroups = (await UserBll.GetGroupsByUser(user.Id)).Select(m => m.Id).ToArray();
                                if (input.UserGroups != null)
                                {
                                    foreach (var newGroup in input.UserGroups.Where(m => !currentGroups.Contains(m.Id)))
                                    {
                                      await   UserBll.AddUserToGroup(newGroup.Id, user.Id);
                                    }
                                    foreach (var oldGroup in currentGroups.Where(m => !input.UserGroups.Select(n => n.Id).Contains(m)))
                                    {
                                        await UserBll.RemoveUserFromGroup(oldGroup, user.Id);
                                    }
                                }
                            }
                            result.Status = 0;
                        }
                    }
                }
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
                input.Id = User.Identity.GetUserId<int>();
                input.UpdateGroups = false;
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    bool canUpdate = true;
                    if (user.UserName != input.UserName)
                    {
                        var currentUser = await UserManager.FindByNameAsync(input.UserName);
                        if (currentUser != null && currentUser.Id > 0 && currentUser.Id != user.Id)
                        {
                            canUpdate = false;
                        }
                    }
                    if (canUpdate)
                    {
                        user.UserName = input.UserName;
                        user.Email = input.Email;
                        user.PhoneNumber = input.PhoneNumber;
                        if (!string.IsNullOrEmpty(input.Password))
                        {
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(input.Password);
                        }
                        var res = await UserManager.UpdateAsync(user);
                        if (res.Succeeded)
                        {
                            // Update display name
                            var scopeClaims = new string[] { "displayName" };
                            var claims = await UserManager.GetClaimsAsync(user.Id);
                            foreach (var claim in claims.Where(m => scopeClaims.Contains(m.Type)))
                            {
                                res = await UserManager.RemoveClaimAsync(user.Id, claim);
                            }
                            res = await UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));

                            if (input.UpdateGroups)
                            {
                                // Update groups
                                var currentGroups = (await UserBll.GetGroupsByUser(user.Id)).Select(m => m.Id).ToArray();
                                if (input.UserGroups != null)
                                {
                                    foreach (var newGroup in input.UserGroups.Where(m => !currentGroups.Contains(m.Id)))
                                    {
                                        await UserBll.AddUserToGroup(newGroup.Id, user.Id);
                                    }
                                    foreach (var oldGroup in currentGroups.Where(m => !input.UserGroups.Select(n => n.Id).Contains(m)))
                                    {
                                        await UserBll.RemoveUserFromGroup(oldGroup, user.Id);
                                    }
                                }
                            }
                            result.Status = 0;
                        }
                    }
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
        [AppAuthorize(Roles = ActionRole.HeThong.Users)]
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
        [Authorize]
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
        [Authorize]
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
        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
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
        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
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
        [AppAuthorize(Roles = ActionRole.HeThong.UserApp)]
        [Route("GenerateUserAppApiKey")]
        public async Task<GenerateUserAppApiKeyModel> GenerateUserAppApiKey()
        {
            var result = new GenerateUserAppApiKeyModel();
            result.ApiKey = ClientApiProvider.GenerateApiKey();
            result.ApiSecret = ClientApiProvider.GenerateApiSecret();

            return await Task.FromResult<GenerateUserAppApiKeyModel>( result);
        }
    }
}
