
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Quang.Auth.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class PermissionDal 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<long> Delete(long permissionId)
        {
            string commandText = "Delete from Permissions where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", permissionId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            string commandText = "Delete from Permissions where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async static Task<long> Insert(Permission permission)
        {
            string commandText = "Insert into Permissions (Id, Name, Description) values (@id, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (permission.Id > 0)
            {
                parameters.Add("@id", permission.Id);
            }
            else
            {
                parameters.Add("@id", null);
            }
            parameters.Add("@name", permission.Name);
            parameters.Add("@description", permission.Description);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async static Task<long> Update(Permission permission)
        {
            string commandText = "Update Permissions set Name = @name, Description = @description  where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", permission.Id);
            parameters.Add("@name", permission.Name);
            parameters.Add("@description", permission.Description);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<Permission> GetOnePermission(long permissionId)
        {
            Permission permission = null;
            string commandText = "Select * from Permissions where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", permissionId } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Permission>(commandText, parameters);
                permission = data.FirstOrDefault();
            }


            return permission;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(string keyword)
        {
            var sql = "select count(*) from Permissions where Name LIKE @param";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(sql, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Permission>> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "select * from Permissions where Name LIKE @param order by Name limit @rowNumber, @pageSize";

            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);

            List<Permission> permissions = new List<Permission>();


            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Permission>(sql, parameters);
                permissions = data.ToList();
            }


            return permissions;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<IEnumerable<Permission>> GetAllPermissions()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "select * from Permissions where 1=1 order by Name";

            List<Permission> permissions = new List<Permission>();


            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Permission>(sql, parameters);
                permissions = data.ToList();
            }


            return permissions;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<PermissionGrant>> GetPermissionGrants(long permissionId)
        {
            var sql = "select * from PermissionGrants where PermissionId = @permissionId order by Id asc";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@permissionId", permissionId);

            IList<PermissionGrant> items = new List<PermissionGrant>();


            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<PermissionGrant>(sql, parameters);
                items = data.ToList();
            }

            return items;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <param name="permissionGrants"></param>
        /// <returns></returns>
        public async static Task<long> UpdatePermissionGrants(long permissionId, IEnumerable<PermissionGrant> permissionGrants)
        {
            var currentItems = (await GetPermissionGrants(permissionId)).Select(m => m.Id).ToArray();
            var addItems = permissionGrants.Where(m => !currentItems.Contains(m.Id)).ToArray();
            var delItems = currentItems.Where(m => !permissionGrants.Select(n => n.Id).Contains(m)).ToArray();
            var updateItems = permissionGrants.Where(m => currentItems.Contains(m.Id)).ToArray();

            foreach (var item in updateItems)
            {
               await UpdatePermissionGrant(item);
            }
            foreach (var item in addItems)
            {
               await InsertPermissionGrant(item);
            }
            await  DeletePermissionGrant(delItems);
            return 1;
        }

        private async static Task<long> InsertPermissionGrant(PermissionGrant permissionGrant)
        {
            string commandText = "Insert into PermissionGrants (Id, PermissionId, Type, IsExactPattern, TermPattern, TermExactPattern) values (@id, @permissionId, @type, @isExactPattern, @termPattern, @termExactPattern)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", null);
            parameters.Add("@permissionId", permissionGrant.PermissionId);
            parameters.Add("@type", permissionGrant.Type);
            parameters.Add("@isExactPattern", permissionGrant.IsExactPattern ? 1 : 0);
            parameters.Add("@termPattern", permissionGrant.TermPattern);
            parameters.Add("@termExactPattern", permissionGrant.TermExactPattern);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        private async static Task<long> UpdatePermissionGrant(PermissionGrant permissionGrant)
        {
            string commandText = "Update PermissionGrants set PermissionId = @permissionId, Type = @type, IsExactPattern = @isExactPattern, TermPattern = @termPattern, TermExactPattern = @termExactPattern  where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", permissionGrant.Id);
            parameters.Add("@permissionId", permissionGrant.PermissionId);
            parameters.Add("@type", permissionGrant.Type);
            parameters.Add("@isExactPattern", permissionGrant.IsExactPattern ? 1 : 0);
            parameters.Add("@termPattern", permissionGrant.TermPattern);
            parameters.Add("@termExactPattern", permissionGrant.TermExactPattern);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<long> DeletePermissionGrant(IEnumerable<long> Ids)
        {
            if (Ids != null && Ids.Count() > 0)
            {
                string commandText = "Delete from PermissionGrants where Id in (" + string.Join(",", Ids.ToArray()) + ")";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                long results;

                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {

                    var id = await conn.QueryAsync<ulong>(commandText, parameters);
                    results = (long)id.Single();
                }

                return results;
            }
            return 0;
        }

        public async static Task<IEnumerable<Permission>> GetUserPermissions(long userId)
        {
            var sql = "select p.* from UserPermissions up inner join Permissions p on p.Id = up.PermissionId where up.UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);

            List<Permission> permissions = new List<Permission>();


            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Permission>(sql, parameters);
                permissions = data.ToList();
            }


            return permissions;
        }

        public async static Task<IEnumerable<Permission>> GetGroupPermissions(long groupId)
        {
            var sql = "select p.* from GroupPermissions gp inner join Permissions p on p.Id = gp.PermissionId where gp.GroupId = @groupId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@groupId", groupId);

            List<Permission> permissions = new List<Permission>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Permission>(sql, parameters);
                permissions = data.ToList();
            }

            return permissions;
        }

        public async static Task<long> UpdateUserPermissions(long userId, IEnumerable<long> permissionIds)
        {
            if (permissionIds == null)
            {
                permissionIds = new long[] { };
            }
            var result = 1;
            var currentItems = (await GetUserPermissions(userId)).Select(m => m.Id).ToArray();
            var addItems = permissionIds.Where(m => !currentItems.Contains(m)).ToArray();
            var delItems = currentItems.Where(m => !permissionIds.Contains(m)).ToArray();

            foreach (var permissionId in addItems)
            {
                await InsertUserPermission(userId, permissionId);
            }
            await DeleteUserPermissions(userId, delItems);

            return result;
        }

        private async static Task<long> InsertUserPermission(long userId, long permissionId)
        {
            string commandText = "Insert into UserPermissions (Id, UserId, PermissionId) values (@id, @userId, @permissionId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", null);
            parameters.Add("@userId", userId);
            parameters.Add("@permissionId", permissionId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;

        }

        public async static Task<long> DeleteUserPermissions(long userId, IEnumerable<long> permissionIds)
        {
            if (permissionIds != null && permissionIds.Count() > 0)
            {
                string commandText = "Delete from UserPermissions where UserId = @userId and PermissionId in (" + string.Join(",", permissionIds.ToArray()) + ")";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@userId", userId);
                long results;

                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {

                    var id = await conn.QueryAsync<ulong>(commandText, parameters);
                    results = (long)id.Single();
                }

                return results;
            }
            return 0;
        }

        public static async  Task<long> UpdateGroupPermissions(long groupId, IEnumerable<long> permissionIds)
        {
            if (permissionIds == null)
            {
                permissionIds = new long[] { };
            }
            var result = 1;
            var currentItems = (await GetGroupPermissions(groupId)).Select(m => m.Id).ToArray();
            var addItems = permissionIds.Where(m => !currentItems.Contains(m)).ToArray();
            var delItems = currentItems.Where(m => !permissionIds.Contains(m)).ToArray();

            foreach (var permissionId in addItems)
            {
               await InsertGroupPermission(groupId, permissionId);
            }
            await DeleteGroupPermissions(groupId, delItems);

            return result;
        }

        private async static Task<long> InsertGroupPermission(long groupId, long permissionId)
        {
            string commandText = "Insert into GroupPermissions (Id, GroupId, PermissionId) values (@id, @groupId, @permissionId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", null);
            parameters.Add("@groupId", groupId);
            parameters.Add("@permissionId", permissionId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;

        }

        public async static Task<long> DeleteGroupPermissions(long groupId, IEnumerable<long> permissionIds)
        {
            if (permissionIds != null && permissionIds.Count() > 0)
            {
                string commandText = "Delete from GroupPermissions where GroupId = @groupId and PermissionId in (" + string.Join(",", permissionIds.ToArray()) + ")";
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@groupId", groupId);
                long results;

                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {

                    var id = await conn.QueryAsync<ulong>(commandText, parameters);
                    results = (long)id.Single();
                }

                return results;
            }
            return 0;
        }

        public async static Task<IEnumerable<PermissionGrant>> GetAllPermissionGrantBelongToUser(long userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IList<PermissionGrant> permissionGrants = new List<PermissionGrant>();

            string sql = "";
            sql += "select pg.* ";
            sql += "from PermissionGrants pg ";
            sql += "where pg.PermissionId in ( ";
            sql += "    select t.PermissionId ";
            sql += "    from (";
            sql += "        (";
            sql += "            select distinct up.PermissionId as PermissionId from UserPermissions as up where up.UserId = @userId ";
            sql += "        )";
            sql += "        UNION DISTINCT ";
            sql += "        (   select distinct gp.PermissionId as PermissionId ";
            sql += "            from GroupUsers as gu ";
            sql += "            inner join GroupPermissions as gp on gp.GroupId = gu.GroupId ";
            sql += "            where gu.UserId = @userId ";
            sql += "        )";
            sql += "    ) as t ";
            sql += ")";

            parameters.Add("@userId", userId);

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    var permissionGrant = new PermissionGrant();
                    permissionGrant.Id = Int32.Parse(row["Id"]);
                    int type = 0;
                    bool isExactPattern = false;
                    if (!string.IsNullOrEmpty(row["Type"]))
                    {
                        type = int.Parse(row["Type"]);
                    }
                    if (!string.IsNullOrEmpty(row["IsExactPattern"]))
                    {
                        isExactPattern = bool.Parse(row["IsExactPattern"]);
                    }
                    permissionGrant.Type = type;
                    permissionGrant.IsExactPattern = isExactPattern;
                    permissionGrant.TermPattern = row["TermPattern"];
                    permissionGrant.TermExactPattern = row["TermExactPattern"];

                    permissionGrants.Add(permissionGrant);
                }
            }
         

            return permissionGrants;
        }

        public async static Task<IEnumerable<long>> GetAllUserIdHasPermission(long permissionId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = "";
            sql += "select distinct up.UserId as UserId from UserPermissions as up where up.PermissionId = @permissionId ";
            sql += "UNION DISTINCT ";
            sql += "select distinct gu.UserId as UserId ";
            sql += "from GroupUsers as gu ";
            sql += "inner join GroupPermissions as gp on gp.GroupId = gu.GroupId ";
            sql += "where gp.PermissionId = @permissionId ";
            parameters.Add("@permissionId", permissionId);
            IList<long> userIds = new List<long>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync(sql, parameters);
                foreach (var row in rows)
                {
                    userIds.Add(long.Parse(row["UserId"]));
                }
            }
            
            
            return userIds;
        } 

        public async static Task<long> DeleteUserPermissions(long userId)
        {
            string commandText = "Delete from UserPermissions where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        public async static Task<long> DeleteGroupPermissions(long groupId)
        {
            string commandText = "Delete from GroupPermissions where GroupId = @groupId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@groupId", groupId);
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
        public async static Task<long> DeletePermissionGrants(long permissionId)
        {
            string commandText = "Delete from PermissionGrants where PermissionId = @permissionId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@permissionId", permissionId);
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }
    }
}