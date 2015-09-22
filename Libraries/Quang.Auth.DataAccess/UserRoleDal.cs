using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quang.Auth.DataAccess
{
    /// <summary>
    /// Class that represents the UserRoles table in the MySQL Database
    /// </summary>
    public static class UserRolesDal
    {
        

        /// <summary>
        /// Returns a list of user's roles
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<List<string>> GetRolesByUserId(long userId)
        {
            List<string> roles = new List<string>();
            string commandText = "Select Roles.Name from UserRoles, Roles where UserRoles.UserId = @userId and UserRoles.RoleId = Roles.Id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);
            
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync<string>(commandText, parameters);

                foreach (var row in rows)
                {
                    roles.Add(row);
                }
            }
            
            

            return roles;
        }

        /// <summary>
        /// Deletes all roles from a user in the UserRoles table
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<long> Delete(long userId)
        {
            string commandText = "Delete from UserRoles where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("roleId", userId);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        /// <summary>
        /// Deletes all roles from a user in the UserRoles table
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="roleId">The role's id</param>
        /// <returns></returns>
        public async static Task<long> Delete(long userId, long roleId)
        {
            string commandText = "Delete from UserRoles where UserId = @userId and RoleId = @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);
            parameters.Add("roleId", roleId);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        /// <summary>
        /// Inserts a new role for a user in the UserRoles table
        /// </summary>
        /// <param name="user">The User</param>
        /// <param name="roleId">The Role's id</param>
        /// <returns></returns>
        public async static Task<long> Insert(long userId, long roleId)
        {
            string commandText = "Insert into UserRoles (UserId, RoleId) values (@userId, @roleId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);
            parameters.Add("roleId", roleId);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }
    }
}
