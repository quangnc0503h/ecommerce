using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Quang.Auth.BusinessLogic
{
    public static class UserBll
    {
        public async static Task<IEnumerable<User>> GetAllUsers()
        {
            return await UserDal.GetAllUsers();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetUsersByGroup(long groupId)
        {
            return await UserDal.GetUsersByGroup(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetUsersByRole(long roleId)
        {
            return await UserDal.GetUsersByRole(roleId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<User> GetOneUser(long userId)
        {
            return await UserDal.GetOneUser(userId);
        }

        public static async Task<User> GetOneUser(long userId, bool getGroups)
        {
            var user = await  UserDal.GetOneUser(userId);
            
            
            if (getGroups)
            {
                user.UserGroups = await UserDal.GetGroupsByUser(userId);
            }
            

            var claims = await UserDal.GetClaimsAsync(user.Id);
            var displayName = claims.FirstOrDefault(m => m.Type == "displayName");
            if (displayName != null)
            {
                user.DisplayName = displayName.Value;
            }

            return user;
        }

        public static async Task<long> CreateUser(User input)
        {
            var result = 1;
       
            var res = await UserDal.Insert(input);
            result = 1;
            if (res > 0)
            {
                input.Id = input.Id;
                if (!string.IsNullOrEmpty(input.Password))
                {
                    //res = await UserManager.AddPasswordAsync(user.Id, input.Password);
                    if (res > 0)
                    {
                        // Update display name
                        res = await UserClaimsDal.Insert( new Claim("displayName", input.DisplayName), input.Id);

                        // Update groups
                        if (input.UserGroups != null)
                        {
                            foreach (var group in input.UserGroups)
                            {
                                await UserDal.AddUserToGroup(group.Id, input.Id);
                            }
                        }
                        result = 0;
                    }
                }
                else
                {
                    result = 0;
                }
            }

            return result;
        }
        public static async Task<int> UpdateUser(User input)
        {
            return await UpdateUser(input, false);
        }

        public static async Task<int> UpdateUser(User input, bool updateGroups)
        {
            var result =   1 ;
            var user = await UserDal.GetOneUser(input.Id);
            if (user != null)
            {
                bool canUpdate = true;
                if (user.UserName != input.UserName)
                {
                    var currentUser = (await UserDal.GetUserByName(input.UserName))[0];
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
                        //user.PasswordHash = UserDal.HashPassword(input.Password);
                    }
                    var res = await UserDal.Update(user);
                    if (res > 0)
                    {
                        // Update display name
                        var scopeClaims = new string[] { "displayName" };
                        var claims = (await UserClaimsDal.GetByUserId(user.Id)).Claims.ToList();
                        foreach (var claim in claims.Where(m => scopeClaims.Contains(m.Type)))
                        {
                            res = await UserClaimsDal.Delete(user.Id, claim);
                        }
                        res = await UserClaimsDal.Insert( new Claim("displayName", input.DisplayName),user.Id);

                        if (updateGroups)
                        {
                            // Update groups
                            var currentGroups = (await UserDal.GetGroupsByUser(user.Id)).Select(m => m.Id).ToArray();
                            if (input.UserGroups != null)
                            {
                                foreach (var newGroup in input.UserGroups.Where(m => !currentGroups.Contains(m.Id)))
                                {
                                    await UserDal.AddUserToGroup(newGroup.Id, user.Id);
                                }
                                foreach (var oldGroup in currentGroups.Where(m => !input.UserGroups.Select(n => n.Id).Contains(m)))
                                {
                                    await UserDal.RemoveUserFromGroup(oldGroup, user.Id);
                                }
                            }
                        }
                        result = 0;
                    }
                }
            }
            return result;
        }
        public async static Task<int> DeleteUser(List<long> Ids)
        {
            var result = 1;
            if (Ids != null && Ids.Count() > 0)
            {
                bool success = false;
                foreach (var id in Ids)
                {
                    var user = await UserDal.GetOneUser(id);
                    if (user != null)
                    {
                        var res = await UserDal.Delete(user);
                        if (res > 0)
                        {
                            // Remove all groups from user
                            var groups = await UserDal.GetGroupsByUser(user.Id);
                            foreach (var group in groups)
                            {
                                await UserBll.RemoveUserFromGroup(group.Id, user.Id);
                            }

                            // Remove all term from user
                            var terms = await TermBll.GetTermsByUser(user.Id);
                            foreach (var term in terms)
                            {
                                await TermBll.RemoveTermFromUser(user.Id, term.Key.Id);
                            }

                            // Remove all permission from group
                            await PermissionBll.DeleteUserPermissions(user.Id);

                            // Set success result
                            if (!success)
                            {
                                success = res ==0;
                            }
                        }
                    }
                }
                if (success)
                {
                    result = 0;
                }
            }
            return result;
        }
        public static async Task<int> CheckExistUserName(string userName, long id)
        {
            var result =   0 ;
            var user = (await UserDal.GetUserByName(userName))[0];
            if (user != null)
            {
                if (id > 0)
                {
                    if (user.Id != id)
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = 1;
                }
            }
            return result;
        }

        public static async Task<int> CheckExistEmail(string email, long id)
        {
            var result =  0 ;
            var user = (await UserDal.GetUserByEmail(email))[0];
            if (user != null)
            {
                if (id > 0)
                {
                    if (user.Id != id)
                    {
                        result = 1;
                    }
                }
                else
                {
                    result = 1;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(long? groupId, string keyword)
        {
            return await UserDal.GetTotal(groupId, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetPaging(int pageSize, int pageNumber, long? groupId, string keyword)
        {
            IEnumerable<User> users = await UserDal.GetPaging(pageSize, pageNumber, groupId, keyword);
            foreach (var user in users)
            {
                var claims = (await UserDal.GetClaimsAsync(user.Id));
                var displayName = claims.FirstOrDefault(m => m.Type == "displayName");
                if (displayName != null)
                {
                    user.DisplayName = displayName.Value;
                }
            }
            return users;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Group>> GetGroupsByUser(long userId)
        {
            return await UserDal.GetGroupsByUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<long> AddUserToGroup(long groupId, long userId)
        {
            return await UserDal.AddUserToGroup(groupId, userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<long> RemoveUserFromGroup(long groupId, long userId)
        {
            return await UserDal.RemoveUserFromGroup(groupId, userId);
        }
        public static async Task<UserApp> GetUserApp(string userApiKey)
        {
            return await GetUserApp(userApiKey, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public async static Task<UserApp> GetUserApp(long userId, AppApiType appType)
        {
            return await UserDal.GetUserApp(userId, appType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApiKey"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async static Task<UserApp> GetUserApp(string userApiKey, bool? isActive)
        {
            return await UserDal.GetUserApp(userApiKey, isActive);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApp"></param>
        /// <returns></returns>
        public async static Task<long> InsertUserApp(UserApp userApp)
        {
            return await UserDal.InsertUserApp(userApp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApp"></param>
        /// <returns></returns>
        public async static Task<long> UpdateUserApp(UserApp userApp)
        {
            return await UserDal.UpdateUserApp(userApp);
        }

        public static async Task<IEnumerable<User>> GetUsersByGroup(params long[] groupIds)
        {
            IList<User> users = new List<User>();
            foreach (var groupId in groupIds)
            {
                var tmp = await GetUsersByGroup(groupId);
                foreach (var user in tmp)
                {
                    if (!users.Any(m => m.Id == user.Id))
                    {
                        users.Add(user);
                    }
                }
            }
            return (users);
        }
        public async static Task<List<string>> GetRolesByUserId(long userId)
        {
            return await UserRolesDal.GetRolesByUserId(userId);
        }

        public async static Task<IList<Claim>> GetClaimsAsync(long userId)
        {
            return (await UserClaimsDal.GetByUserId(userId)).Claims.ToList();
        }

        public static async Task<User> FindUserAsync(string loginProvider, string providerKey)
        {
            return await UserDal.FindUserAsync(loginProvider, providerKey);
        }
        public static async Task<long> InsertUserLogIn(long userId, string loginProvider, string providerKey)
        {
            return await UserLoginDal.Insert(userId, loginProvider, providerKey);
        }

        public async static Task<long> InsertUserClaim(string claimValue, string claimType, long userId)
        {
            return await UserClaimsDal.Insert(claimValue, claimType, userId);
        }
    }
}
