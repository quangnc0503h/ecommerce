using Dapper;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quang.Auth.DataAccess
{
    /// <summary>
    /// Class that represents the Role table in the MySQL Database
    /// </summary>
    public static class RoleDal 
    {
        
        /// <summary>
        /// Deltes a role from the Roles table
        /// </summary>
        /// <param name="roleId">The role Id</param>
        /// <returns></returns>
        public async static Task<long> Delete(int roleId)
        {
            string commandText = "Delete from Roles where Id = @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@roleId", roleId);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        /// <summary>
        /// Inserts a new Role in the Roles table
        /// </summary>
        /// <param name="roleName">The role's name</param>
        /// <returns></returns>
        public async static Task<long> Insert(Role role)
        {
            string commandText = "Insert into Roles (Id, Name) values (@id, @name)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", role.Name);
            parameters.Add("@id", role.Id);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        /// <summary>
        /// Returns a role name given the roleId
        /// </summary>
        /// <param name="roleId">The role Id</param>
        /// <returns>Role name</returns>
        public async static Task<string> GetRoleName(int roleId)
        {
            string commandText = "Select * from Roles where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);
            Role role;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Role>(commandText, new { });
                role = data.First();
            }
            return role.Name;
        }

        /// <summary>
        /// Returns the role Id given a role name
        /// </summary>
        /// <param name="roleName">Role's name</param>
        /// <returns>Role's Id</returns>
        public async static Task<long> GetRoleId(string roleName)
        {
            
            string commandText = "Select Id from Roles where Name = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", roleName } };

            Role role;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Role>(commandText, new { });
                role = data.First();
            }

            return role.Id;
        }

        /// <summary>
        /// Gets the IdentityRole given the role Id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async static Task<Role> GetRoleById(long roleId)
        {
            string commandText = "Select * from Roles where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);
            Role role;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Role>(commandText, new { });
                role = data.First();
            }

            return role;

        }

        /// <summary>
        /// Gets the IdentityRole given the role name
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async static Task<Role> GetRoleByName(string roleName)
        {
            string commandText = "Select Id from Roles where Name = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", roleName } };

            Role role;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Role>(commandText, new { });
                role = data.First();
            }


            return role;
        }

        public async static Task<long> Update(Role role)
        {
            string commandText = "Update Roles set Name = @name where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", role.Id);

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        public async static Task<IEnumerable<Role>> GetAllRoles()
        {
            List<Role> roles = new List<Role>();
            string commandText = "Select * from Roles ";



            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Role>(commandText, new { });
                roles = data.ToList();
            }

            return roles;
        }
    }
}
