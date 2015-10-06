
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Security.Claims;

namespace Quang.Auth.DataAccess
{
    
    public static class UserDal 
    {
        /// <summary>
        /// Returns a list of TUser instances given a user name
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <returns></returns>
        public async static Task<List<User>> GetUserByName(string userName)
        {
            List<User> users = new List<User>();
            string commandText = "Select * from Users where UserName = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", userName } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<User>(commandText, parameters);
                users = data.ToList();
            }

            return users;
        }

        public static async Task<User> FindUserAsync(string loginProvider, string providerKey)
        {
            List<User> users = new List<User>();
            var commandText = "select u.* from Users u inner join ExternalLogins l on l.UserId = u.UserId where l.LoginProvider = @loginProvider and l.ProviderKey = @providerKey";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("loginProvider", loginProvider);
            parameters.Add("providerKey", providerKey);
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<User>(commandText, parameters);
                users = data.ToList();
            }

            return users.FirstOrDefault();
        }
        /// <summary>
        /// Returns a list of TUser instances given a user name
        /// </summary>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetAllUsers()
        {
            List<User> users = new List<User>();
            string commandText = "Select * from Users ";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<User>(commandText, parameters);
                users = data.ToList();
            }


            return users;
        }

        public static Task<List<User>> GetUserByEmail(string email)
        {
            return null;
        }

        /// <summary>
        /// Return the user's password hash
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<string> GetPasswordHash(long userId)
        {
            string commandText = "Select PasswordHash from Users where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", userId);

            string results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<string>(commandText, parameters);
                results = id.First();
            }

            return results;
        }
        /// <summary>
        /// Sets the user's password hash
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public async static Task<int> SetPasswordHash(string userId, string passwordHash)
        {
            string commandText = "Update Users set PasswordHash = @pwdHash where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@pwdHash", passwordHash);
            parameters.Add("@id", userId);

            int results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (int)id.Single();
            }

            return results;
        }
        /// <summary>
        /// Returns the user's security stamp
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<string> GetSecurityStamp(long userId)
        {
            string commandText = "Select SecurityStamp from Users where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };
            string results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<string>(commandText, parameters);
                results = id.First();
            }

            return results;
        }
      
        /// <summary>
        /// Inserts a new user in the Users table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<long> Insert(User user)
        {
            string commandText = @"Insert into Users (UserName, Id, PasswordHash, SecurityStamp,Email,EmailConfirmed,PhoneNumber,PhoneNumberConfirmed, AccessFailedCount,LockoutEnabled,LockoutEndDateUtc,TwoFactorEnabled)
                values (@name, null, @pwdHash, @SecStamp,@email,@emailconfirmed,@phonenumber,@phonenumberconfirmed,@accesscount,@lockoutenabled,@lockoutenddate,@twofactorenabled)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", user.UserName);
            parameters.Add("@id", user.Id);
            parameters.Add("@pwdHash", user.PasswordHash);
            parameters.Add("@SecStamp", user.SecurityStamp);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);
            parameters.Add("@phonenumber", user.PhoneNumber);
            parameters.Add("@phonenumberconfirmed", user.PhoneNumberConfirmed);
            parameters.Add("@accesscount", user.AccessFailedCount);
            parameters.Add("@lockoutenabled", user.LockoutEnabled);
            parameters.Add("@lockoutenddate", user.LockoutEndDateUtc);
            parameters.Add("@twofactorenabled", user.TwoFactorEnabled);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// Deletes a user from the Users table
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<long> Delete(long userId)
        {
            string commandText = "Delete from Users where Id = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
                commandText = "Delete from UserRoles where UserId = @userId";
                await conn.QueryAsync<ulong>(commandText, parameters);
            }

            
            

            return results;
        }

        /// <summary>
        /// Deletes a user from the Users table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async static Task<long> Delete(User user)
        {
            return await Delete(user.Id);
        }
        public async static Task<long> Update(User user)
        {
            string commandText = @"Update Users set UserName = @userName, PasswordHash = @pswHash, SecurityStamp = @secStamp, 
                Email=@email, EmailConfirmed=@emailconfirmed, PhoneNumber=@phonenumber, PhoneNumberConfirmed=@phonenumberconfirmed,
                AccessFailedCount=@accesscount, LockoutEnabled=@lockoutenabled, LockoutEndDateUtc=@lockoutenddate, TwoFactorEnabled=@twofactorenabled  
                WHERE Id = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userName", user.UserName);
            parameters.Add("@pswHash", user.PasswordHash);
            parameters.Add("@secStamp", user.SecurityStamp);
            parameters.Add("@userId", user.Id);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);
            parameters.Add("@phonenumber", user.PhoneNumber);
            parameters.Add("@phonenumberconfirmed", user.PhoneNumberConfirmed);
            parameters.Add("@accesscount", user.AccessFailedCount);
            parameters.Add("@lockoutenabled", user.LockoutEnabled);
            parameters.Add("@lockoutenddate", user.LockoutEndDateUtc);
            parameters.Add("@twofactorenabled", user.TwoFactorEnabled);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        //public async static Task<IEnumerable<User>> GetAllUsers()
        //{
        //    IList<User> users = new List<User>();
        //    string commandText = "Select * from Users order by Name";


        //    using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
        //    {
        //        var data = await conn.QueryAsync<User>(commandText, new { });
        //        users = data.ToList();
        //    }

        //    return users;
        //}

        public async static Task<IEnumerable<User>> GetUsersByGroup(long groupId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = "select u.Id, u.UserName, u.Email, u.PhoneNumber from Users u where exists (select gu.* from GroupUsers gu where gu.GroupId = @groupId and gu.UserId = u.Id)";

            parameters.Add("@groupId", groupId);

            List<User> users = new List<User>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<User>(sql, parameters);
                users = data.ToList();
            }
            return users;
        }

        public async static Task<IEnumerable<User>> GetUsersByRole(long roleId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = "select u.Id, u.UserName, u.Email, u.PhoneNumber from UserRoles ur inner join Users u on u.Id = ur.UserId where ur.RoleId = @roleId";

            parameters.Add("@roleId", roleId);

            List<User> users = new List<User>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<User>(sql, parameters);
                users = data.ToList();
            }

            return users;
        }

        public async static Task<User> GetOneUser(long userId)
        {
            User user = null;
            string commandText = "Select * from Users where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<User>(commandText, parameters);
                user = data.ToList().First();
            }


            return user;
        }

        public async static Task<long> GetTotal(long? groupId, string keyword)
        {
            string sql = "select count(u.id) from Users u where (u.UserName like @param or u.Email like @param)";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            
            if (groupId.HasValue && groupId > 0)
            {
                sql += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                parameters.Add("@param1", groupId.Value);
            }

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                
                var id = await conn.QueryAsync<long>(sql, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<IEnumerable<User>> GetPaging(int pageSize, int pageNumber, long? groupId, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = "select u.* from Users u where (u.UserName like @param or u.Email like @param)";

            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");

            if (groupId.HasValue && groupId > 0)
            {
                sql += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                parameters.Add("@param1", groupId.Value);
            }

            sql += " order by u.UserName limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);

            List<User> users = new List<User>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<User>(sql, parameters);
                users = data.ToList();
            }

            return users;
        }

        public async static Task<IEnumerable<Group>> GetGroupsByUser(long userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = "";
            sql += "select g.* ";
            sql += "from GroupUsers gu ";
            sql += "inner join Users u on u.Id = gu.UserId ";
            sql += "inner join Groups g on g.Id = gu.GroupId ";
            sql += "where u.Id = @userId";

            parameters.Add("@userId", userId);

            IList<Group> groups = new List<Group>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {             
                var data = await conn.QueryAsync<Group>(sql, parameters);
                groups = data.ToList();
            }
            return groups;
        }

        public async static Task<long> AddUserToGroup(long groupId, long userId)
        {
            string commandText = "Insert into GroupUsers (GroupId, UserId) values (@groupId, @userId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            
            parameters.Add("@groupId", groupId);
            parameters.Add("@userId", userId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        public async static Task<long> RemoveUserFromGroup (long groupId, long userId)
        {
            string commandText = "Delete from GroupUsers where GroupId = @groupId and UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@groupId", groupId);
            parameters.Add("@userId", userId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        public async static Task<UserApp> GetUserApp(long userId, AppApiType appType)
        {
            UserApp userApp = null;
            string commandText = "";
            commandText += "select ua.* from UserApps ua ";
            commandText += "inner join Users u on u.id = ua.UserId ";
            commandText += "where u.Id = @userId ";
            commandText += "and ua.ApiType = @apiType";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);
            parameters.Add("@apiType", appType.GetHashCode());

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<UserApp>(commandText, parameters);
                userApp = data.ToList().First();
            }

            return userApp;
        }

        public async static Task<UserApp> GetUserApp(string userApiKey, bool? isActive)
        {
            UserApp userApp = null;
            string commandText = "";
            commandText += "select ua.* from UserApps ua ";
            commandText += "inner join Users u on u.id = ua.UserId ";
            commandText += "where ua.ApiKey = @apiKey";
            if (isActive.HasValue)
            {
                commandText += " and ua.IsActive = " + (isActive.Value ? 1 : 0);
            }
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@apiKey", userApiKey } };

            

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                
                var data = await conn.QueryAsync<UserApp>(commandText, parameters);
                userApp = data.ToList().First();
            }
            
            return userApp;
        }

        public async static Task<long> InsertUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
            {
                return 0;
            }
            if (userApp.Id > 0)
            {
                return await UpdateUserApp(userApp);
            }
            else
            {
                string commandText = "Insert Into UserApps  ";
                commandText += "(UserId,  IsActive,  ApiType,  ApiName,  ApiKey,  ApiSecret,  AppHosts,  AppIps) Values ";
                commandText += "(@userId, @isActive, @apiType, @apiName, @apiKey, @apiSecret, @appHosts, @appIps)";
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@userId", userApp.UserId);
                parameters.Add("@isActive", userApp.IsActive ? 1 : 0);
                parameters.Add("@apiType", userApp.ApiType.GetHashCode());
                parameters.Add("@apiName", userApp.ApiName);
                parameters.Add("@apiKey", userApp.ApiKey);
                parameters.Add("@apiSecret", userApp.ApiSecret);
                parameters.Add("@appHosts", userApp.ApiSecret);
                parameters.Add("@appIps", userApp.ApiSecret);

                long results;

                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {
                    
                    results = await conn.ExecuteAsync(commandText, parameters);

                }

                return results;
            }
        }

        public async static Task<long> UpdateUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
            {
                return 0;
            }
            if (userApp.Id > 0)
            {
                string commandText = "Update UserApps Set ";
                commandText += "UserId = @userId, IsActive = @isActive, ApiType = @apiType, ApiName = @apiName, ApiKey = @apiKey, ApiSecret = @apiSecret, AppHosts = @appHosts, AppIps = @appIps ";
                commandText += "where Id = @id";
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@id", userApp.Id);
                parameters.Add("@userId", userApp.UserId);
                parameters.Add("@isActive", userApp.IsActive ? 1 : 0);
                parameters.Add("@apiType", userApp.ApiType.GetHashCode());
                parameters.Add("@apiName", userApp.ApiName);
                parameters.Add("@apiKey", userApp.ApiKey);
                parameters.Add("@apiSecret", userApp.ApiSecret);
                parameters.Add("@appHosts", userApp.AppHosts);
                parameters.Add("@appIps", userApp.AppIps);

                long results;

                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {
                    
                    results = await conn.ExecuteAsync(commandText, parameters);

                }

                return results;
            }
            else
            {
                return await InsertUserApp(userApp);
            }
        }

        public static async Task<IList<Claim>> GetClaimsAsync(long userId)
        {
            ClaimsIdentity identity = await UserClaimsDal.GetByUserId(userId);

            return (identity.Claims.ToList());
        }

    }
}