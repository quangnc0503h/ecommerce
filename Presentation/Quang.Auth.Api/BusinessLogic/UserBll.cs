using AspNet.Identity.MySQL;
using AutoMapper;
using Quang.Auth.Api;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using Quang.Auth.Api.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class UserBll : IUserBll
    {
        private IUserTable _userTable;
        private IGroupTable _groupTable;
        private IPermissionTable _permissionTable;
        private ITermTable _termTable;

        public ApplicationUserManager UserManager
        {
            get
            {
                return OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return OwinContextExtensions.GetUserManager<ApplicationRoleManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            }
        }

        public MySQLDatabase Database { get; private set; }

        public UserBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._userTable = (IUserTable)new UserTable(this.Database);
            this._groupTable = (IGroupTable)new GroupTable(this.Database);
            this._permissionTable = (IPermissionTable)new PermissionTable(this.Database);
            this._termTable = (ITermTable)new TermTable(this.Database);
        }

        public async Task<DanhSachUserOutput> GetAll(FilterUserInput input)
        {
            int totalCount = this._userTable.GetTotal(input.GroupId, input.Keyword);
            IEnumerable<User> users = this._userTable.GetPaging(input.PageSize, input.PageNumber, input.GroupId, input.Keyword);
            foreach (User user in users)
            {
                IList<Claim> claims = await this.UserManager.GetClaimsAsync(user.Id);
                Claim displayName = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "displayName"));
                if (displayName != null)
                    user.DisplayName = displayName.Value;
            }
            DanhSachUserOutput result = new DanhSachUserOutput()
            {
                DanhSachUsers = users,
                TotalCount = (long)totalCount
            };
            return result;
        }

        public async Task<GetOneUserOutput> GetOneUser(string userId)
        {
            return await this.GetOneUser(userId, false);
        }

        public async Task<GetOneUserOutput> GetOneUser(string userId, bool getGroups)
        {
            GetOneUserOutput result = new GetOneUserOutput();
            ApplicationUser user = (ApplicationUser)null;
            if (user == null && !string.IsNullOrEmpty(userId) && Regex.IsMatch(userId, "^[1-9]([0-9]*)$"))
                user = await this.UserManager.FindByIdAsync(int.Parse(userId));
            if (user == null && !string.IsNullOrEmpty(userId))
                user = await this.UserManager.FindByNameAsync(userId);
            if (user != null)
            {
                Mapper.CreateMap<ApplicationUser, User>();
                result.User = Mapper.Map<User>((object)user);
                if (getGroups)
                    result.User.UserGroups = this._userTable.GetGroupsByUser(user.Id);
                result.User.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                IList<Claim> claims = await this.UserManager.GetClaimsAsync(user.Id);
                Claim displayName = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "displayName"));
                if (displayName != null)
                    result.User.DisplayName = displayName.Value;
            }
            return result;
        }

        private string GeneratePassword(int length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();
            while (0 < length--)
                stringBuilder.Append("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"[random.Next("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Length)]);
            return stringBuilder.ToString();
        }

        public async Task<SetMobilePasswordOutput> SetMobilePassword(SetMobilePasswordInput input)
        {
            SetMobilePasswordOutput output = new SetMobilePasswordOutput()
            {
                Status = 1
            };
            ApplicationUser currentUser = await this.UserManager.FindByNameAsync(input.Mobile);
            if (currentUser != null && currentUser.Id > 0)
            {
                string password = this.GeneratePassword(6);
                IdentityResult res = await this.UserManager.AddPasswordAsync(currentUser.Id, password);
                if (res.Succeeded)
                {
                    output.Status = 0;
                    output.MobileMsg = string.Format(ConfigurationManager.AppSettings["SmsMsgMkErrorSuccess"], (object)password);
                }
                else
                    output.MobileMsg = ConfigurationManager.AppSettings["SmsMsgMkErrorUnknown"];
            }
            else
                output.MobileMsg = ConfigurationManager.AppSettings["SmsMsgMkErrorNotExist"];
            return output;
        }

        public async Task<GetMobileProfileOutput> GetMobileProfile(int userId)
        {
            GetMobileProfileOutput res = (GetMobileProfileOutput)null;
            ApplicationUser currentUser = await this.UserManager.FindByIdAsync(userId);
            if (currentUser != null)
            {
                res = new GetMobileProfileOutput();
                res.Email = currentUser.Email;
                IList<Claim> claims = await this.UserManager.GetClaimsAsync(userId);
                Claim name = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "displayName"));
                if (name != null && !string.IsNullOrEmpty(name.Value))
                    res.DisplayName = name.Value;
                Claim cmnd = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "profile:cmnd"));
                if (cmnd != null && !string.IsNullOrEmpty(cmnd.Value))
                    res.Cmnd = cmnd.Value;
                Claim cty = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "profile:cty"));
                if (cty != null && !string.IsNullOrEmpty(cty.Value))
                    res.CongTy = cty.Value;
                Claim diachi = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "profile:diachi"));
                if (diachi != null && !string.IsNullOrEmpty(diachi.Value))
                    res.DiaChi = diachi.Value;
                Claim mst = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "profile:mst"));
                if (mst != null && !string.IsNullOrEmpty(mst.Value))
                    res.MaSoThue = mst.Value;
                Claim avartar = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "profile:avartar"));
                if (avartar != null && !string.IsNullOrEmpty(avartar.Value))
                    res.Avatar = avartar.Value;
            }
            return res;
        }

        public async Task<UpdateMobileProfileOutput> UpdateMobileProfile(int userId, UpdateMobileProfileInput input)
        {
            UpdateMobileProfileOutput output = (UpdateMobileProfileOutput)null;
            ApplicationUser user = await this.UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                output = new UpdateMobileProfileOutput()
                {
                    Status = 1
                };
                user.Email = input.Email;
                IdentityResult res = await this.UserManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    string[] scopeClaims = new string[6]
                    {
            "displayName",
            "profile:cmnd",
            "profile:avartar",
            "profile:cty",
            "profile:diachi",
            "profile:mst"
                    };
                    IList<Claim> claims = await this.UserManager.GetClaimsAsync(user.Id);
                    foreach (Claim claim in Enumerable.Where<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => Enumerable.Contains<string>((IEnumerable<string>)scopeClaims, m.Type))))
                    {
                        IdentityResult identityResult = await this.UserManager.RemoveClaimAsync(user.Id, claim);
                    }
                    if (!string.IsNullOrEmpty(input.DisplayName))
                    {
                        IdentityResult identityResult1 = await this.UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));
                    }
                    if (!string.IsNullOrEmpty(input.Cmnd))
                    {
                        IdentityResult identityResult2 = await this.UserManager.AddClaimAsync(user.Id, new Claim("profile:cmnd", input.Cmnd));
                    }
                    if (!string.IsNullOrEmpty(input.CongTy))
                    {
                        IdentityResult identityResult3 = await this.UserManager.AddClaimAsync(user.Id, new Claim("profile:cty", input.CongTy));
                    }
                    if (!string.IsNullOrEmpty(input.DiaChi))
                    {
                        IdentityResult identityResult4 = await this.UserManager.AddClaimAsync(user.Id, new Claim("profile:diachi", input.DiaChi));
                    }
                    if (!string.IsNullOrEmpty(input.MaSoThue))
                    {
                        IdentityResult identityResult5 = await this.UserManager.AddClaimAsync(user.Id, new Claim("profile:mst", input.MaSoThue));
                    }
                    if (!string.IsNullOrEmpty(input.Avatar))
                    {
                        IdentityResult identityResult6 = await this.UserManager.AddClaimAsync(user.Id, new Claim("profile:avartar", input.Avatar));
                    }
                    output.Status = 0;
                }
            }
            return output;
        }

        public async Task<CreateMobileUserOutput> CreateMobileUser(CreateMobileUserInput minput)
        {
            CreateMobileUserOutput output = new CreateMobileUserOutput()
            {
                Status = 1
            };
            ApplicationUser currentUser = await this.UserManager.FindByNameAsync(minput.Mobile);
            if (currentUser != null && currentUser.Id > 0)
            {
                output.MobileMsg = ConfigurationManager.AppSettings["SmsMsgRegistErrorExist"];
            }
            else
            {
                string password = this.GeneratePassword(6);
                CreateUserInput input = new CreateUserInput()
                {
                    UserName = minput.Mobile,
                    DisplayName = minput.Mobile,
                    PhoneNumber = minput.Mobile,
                    Password = password,
                    ConfirmPassword = password
                };
                CreateUserOutput res = await this.CreateUser(input);
                if (res.Status == 0)
                {
                    output.Status = 0;
                    output.MobileMsg = string.Format(ConfigurationManager.AppSettings["SmsMsgRegistSuccess"], (object)password);
                }
                else
                    output.MobileMsg = ConfigurationManager.AppSettings["SmsMsgRegistErrorUnknown"];
            }
            return output;
        }

        public async Task<CreateUserOutput> CreateUser(CreateUserInput input)
        {
            CreateUserOutput result = new CreateUserOutput();
            result.Status = 1;
            ApplicationUser user = new ApplicationUser();
            user.Email = input.Email;
            user.UserName = input.UserName;
            user.PhoneNumber = input.PhoneNumber;
            IdentityResult validateUserResult = await this.UserManager.UserValidator.ValidateAsync(user);
            IdentityResult validatePassResult = await this.UserManager.PasswordValidator.ValidateAsync(input.Password);
            if (validateUserResult.Succeeded && validatePassResult.Succeeded)
            {
                IdentityResult res = await this.UserManager.CreateAsync(user);
                if (res.Succeeded)
                {
                    input.Id = user.Id;
                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        res = await this.UserManager.AddPasswordAsync(user.Id, input.Password);
                        if (res.Succeeded)
                        {
                            if (!string.IsNullOrEmpty(input.DisplayName))
                                res = await this.UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));
                            if (input.UserGroups != null)
                            {
                                foreach (Quang.Auth.Entities.Group group in input.UserGroups)
                                    this._userTable.addUserToGroup(group.Id, user.Id);
                            }
                            result.Status = 0;
                        }
                        else
                        {
                            IdentityResult identityResult = await this.UserManager.DeleteAsync(user);
                        }
                    }
                    else
                        result.Status = 0;
                }
            }
            return result;
        }

        public async Task<UpdateUserOutput> UpdateUser(UpdateUserInput input)
        {
            return await this.UpdateUser(input, false);
        }

        public async Task<UpdateUserOutput> UpdateUser(UpdateUserInput input, bool updateGroups)
        {
            UpdateUserOutput result = new UpdateUserOutput()
            {
                Status = 1
            };
            ApplicationUser user = await this.UserManager.FindByIdAsync(input.Id);
            if (user != null)
            {
                bool canUpdate = true;
                if (user.UserName != input.UserName)
                {
                    ApplicationUser currentUser = await this.UserManager.FindByNameAsync(input.UserName);
                    if (currentUser != null && currentUser.Id > 0 && currentUser.Id != user.Id)
                        canUpdate = false;
                }
                if (canUpdate)
                {
                    user.UserName = input.UserName;
                    user.Email = input.Email;
                    user.PhoneNumber = input.PhoneNumber;
                    IdentityResult validateResult = await this.UserManager.UserValidator.ValidateAsync(user);
                    if (validateResult.Succeeded && !string.IsNullOrEmpty(input.Password))
                        validateResult = await this.UserManager.PasswordValidator.ValidateAsync(input.Password);
                    if (validateResult.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(input.Password))
                            user.PasswordHash = this.UserManager.PasswordHasher.HashPassword(input.Password);
                        IdentityResult res = await this.UserManager.UpdateAsync(user);
                        if (res.Succeeded)
                        {
                            string[] scopeClaims = new string[1]
                            {
                "displayName"
                            };
                            IList<Claim> claims = await this.UserManager.GetClaimsAsync(user.Id);
                            foreach (Claim claim in Enumerable.Where<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => Enumerable.Contains<string>((IEnumerable<string>)scopeClaims, m.Type))))
                                res = await this.UserManager.RemoveClaimAsync(user.Id, claim);
                            res = await this.UserManager.AddClaimAsync(user.Id, new Claim("displayName", input.DisplayName));
                            if (updateGroups)
                            {
                                int[] currentGroups = Enumerable.ToArray<int>(Enumerable.Select<Quang.Auth.Entities.Group, int>(this._userTable.GetGroupsByUser(user.Id), (Func<Quang.Auth.Entities.Group, int>)(m => m.Id)));
                                if (input.UserGroups != null)
                                {
                                    foreach (Quang.Auth.Entities.Group group in Enumerable.Where<Quang.Auth.Entities.Group>(input.UserGroups, (Func<Quang.Auth.Entities.Group, bool>)(m => !Enumerable.Contains<int>((IEnumerable<int>)currentGroups, m.Id))))
                                        this._userTable.addUserToGroup(group.Id, user.Id);
                                    foreach (int groupId in Enumerable.Where<int>((IEnumerable<int>)currentGroups, (Func<int, bool>)(m => !Enumerable.Contains<int>(Enumerable.Select<Quang.Auth.Entities.Group, int>(input.UserGroups, (Func<Quang.Auth.Entities.Group, int>)(n => n.Id)), m))))
                                        this._userTable.removeUserFromGroup(groupId, user.Id);
                                }
                            }
                            result.Status = 0;
                        }
                    }
                }
            }
            return result;
        }

        public async Task<DeleteUserOutput> DeleteUser(DeleteUserInput input)
        {
            DeleteUserOutput result = new DeleteUserOutput()
            {
                Status = 1
            };
            if (input.Ids != null && Enumerable.Count<int>((IEnumerable<int>)input.Ids) > 0)
            {
                bool success = false;
                foreach (int userId in input.Ids)
                {
                    ApplicationUser user = await this.UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        IdentityResult res = await this.UserManager.DeleteAsync(user);
                        if (res.Succeeded)
                        {
                            foreach (Quang.Auth.Entities.Group group in this._userTable.GetGroupsByUser(user.Id))
                                this._userTable.removeUserFromGroup(group.Id, user.Id);
                            foreach (KeyValuePair<Term, bool> keyValuePair in (IEnumerable<KeyValuePair<Term, bool>>)this._termTable.GetTermsByUser(user.Id))
                                this._termTable.removeTermFromUser(user.Id, keyValuePair.Key.Id);
                            this._permissionTable.DeleteUserPermissions(user.Id);
                            if (!success)
                                success = res.Succeeded;
                        }
                    }
                }
                if (success)
                    result.Status = 0;
            }
            return result;
        }

        public async Task<CheckUserExistOutput> CheckExistUserName(string userName, int id)
        {
            CheckUserExistOutput result = new CheckUserExistOutput()
            {
                Check = 0
            };
            ApplicationUser user = await this.UserManager.FindByNameAsync(userName);
            if (user != null)
            {
                if (id > 0)
                {
                    if (user.Id != id)
                        result.Check = 1;
                }
                else
                    result.Check = 1;
            }
            return result;
        }

        public async Task<CheckUserExistOutput> CheckExistEmail(string email, int id)
        {
            CheckUserExistOutput result = new CheckUserExistOutput()
            {
                Check = 0
            };
            ApplicationUser user = await this.UserManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (id > 0)
                {
                    if (user.Id != id)
                        result.Check = 1;
                }
                else
                    result.Check = 1;
            }
            return result;
        }

        public Task<UserApp> GetUserApp(string userApiKey)
        {
            return this.GetUserApp(userApiKey, new bool?(true));
        }

        public Task<ResultUpdateOutput> UpdateUserApp(UpdateUserAppInput input, AppApiType appType)
        {
            Mapper.CreateMap<UpdateUserAppInput, UserApp>();
            UserApp userApp = Mapper.Map<UserApp>((object)input);
            userApp.ApiType = appType;
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            if (this._userTable.UpdateUserApp(userApp) > 0)
                result.Status = 0;
            return Task.FromResult<ResultUpdateOutput>(result);
        }

        public Task<UserApp> GetUserApp(string userApiKey, bool? isActive)
        {
            return Task.FromResult<UserApp>(this._userTable.GetUserApp(userApiKey, isActive));
        }

        public Task<UserApp> GetUserApp(int userId, AppApiType appType)
        {
            return Task.FromResult<UserApp>(this._userTable.GetUserApp(userId, appType));
        }

        public Task<IEnumerable<User>> GetUsersByGroup(params int[] groupIds)
        {
            IList<User> list = (IList<User>)new List<User>();
            foreach (int groupId in groupIds)
            {
                foreach (User user1 in this._userTable.GetUsersByGroup(groupId))
                {
                    User user = user1;
                    if (!Enumerable.Any<User>((IEnumerable<User>)list, (Func<User, bool>)(m => m.Id == user.Id)))
                        list.Add(user);
                }
            }
            return Task.FromResult<IEnumerable<User>>((IEnumerable<User>)list);
        }
    }
}