using Dapper;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.DataAccess
{
    

    public static class GroupDal 
    {
        

        public async static Task<long> Delete(long groupId)
        {
            string commandText = "Delete from Groups where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", groupId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            string commandText = "Delete from Groups where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<long> Insert(Group group)
        {
            string commandText = "Insert into Groups (Id, ParentId, Name, Description) values (@id, @parentId, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (group.Id > 0)
            {
                parameters.Add("@id", group.Id);
            } else
            {
                parameters.Add("@id", null);
            }
            if (group.ParentId > 0)
            {
                parameters.Add("@parentId", group.ParentId);
            }
            else
            {
                parameters.Add("@parentId", null);
            }
            parameters.Add("@name", group.Name);
            parameters.Add("@description", group.Description);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<long> Update(Group group)
        {
            string commandText = "Update Groups set ParentId = @parentId, Name = @name, Description = @description  where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (group.ParentId > 0)
            {
                parameters.Add("@parentId", group.ParentId.Value);
            } else
            {
                parameters.Add("@parentId", null);
            }
            parameters.Add("@id", group.Id);
            parameters.Add("@name", group.Name);
            parameters.Add("@description", group.Description);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(commandText, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<IDictionary<long, Group>> GetAllGroups()
        {
            IDictionary<long, Group> groups = new Dictionary<long, Group>();
            string commandText = "";
            commandText += "select g.Id, g.Name, g.Description, g.ParentId, g1.Name as ParentName ";
            commandText += ", sum(if(gu.UserId is not null, 1, 0)) as TotalMembers ";
            commandText += "from Groups g ";
            commandText += "left join GroupUsers gu on gu.GroupId = g.Id ";
            commandText += "left join Groups as g1 on g1.Id = g.ParentId ";
            commandText += "group  by g.Id, g.Name, g.Description, g.ParentId, ParentName ";
            commandText += "order by g.ParentId, g.Name";

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var rows = await conn.QueryAsync<Group>(commandText, new{ });
                foreach (var row in rows)
                {
                    var group = new Group();
                    group.Id = row.Id;
                    group.Name = row.Name;
                    group.Description = row.Description;
                    if (row.ParentId.HasValue)
                    {
                        group.ParentId = row.ParentId;
                        group.ParentName = row.ParentName;
                    }
                    if ((row.TotalMembers > 0))
                    {
                        group.TotalMembers = row.TotalMembers;
                    }

                    groups.Add(group.Id, group);
                }
            }
          

            return groups;
        }

        public async static Task<Group> GetOneGroup(long groupId)
        {
            Group group = null;
            string commandText = "Select * from Groups where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", groupId } };

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Group>(commandText, parameters);
                group = data.FirstOrDefault();
            }

            return group;
        }

        public async static Task<long> GetTotal(long? parentId, string keyword)
        {
            var sql = "select count(*) from Groups where Name LIKE @param";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            
            if (parentId.HasValue && parentId > 0)
            {
                sql += " and ParentId = @param1";
                parameters.Add("@param1", parentId.Value);
            }

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<ulong>(sql, parameters);
                results = (long)id.Single();
            }

            return results;
        }

        public async static Task<IEnumerable<Group>> GetPaging(int pageSize, int pageNumber, long? parentId, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "select g1.*,g2.Name as ParentName from Groups g1 left join Groups g2 on g2.Id = g1.ParentId where g1.Name LIKE @param";

            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");

            if (parentId.HasValue && parentId > 0)
            {
                sql += " and g1.ParentId = @param1";
                parameters.Add("@param1", parentId.Value);
            }

            sql += " order by g1.Name limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);

            List<Group> groups = new List<Group>();

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Group>(sql, parameters);
                groups = data.ToList();
            }

            return groups;
        }
    }
}