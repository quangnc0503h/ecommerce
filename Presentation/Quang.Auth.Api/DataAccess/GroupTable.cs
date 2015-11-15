using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Quang.Auth.Api.DataAccess
{
    public class GroupTable : IGroupTable
    {
        private readonly MySQLDatabase _database;

        public GroupTable(MySQLDatabase database)
        {
            _database = database;
        }

        public int Delete(int groupId)
        {
            return _database.Execute("Delete from Groups where Id = @id", new Dictionary<string, object>()
                                                                          {
                                                                              {
                                                                                  "@id",
                                                                                  groupId
                                                                              }
                                                                          });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            return _database.Execute("Delete from Groups where Id in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
        }

        public int Insert(Group group)
        {
            const string commandText = "Insert into Groups (Id, ParentId, Name, Description) values (@id, @parentId, @name, @description)";
            var parameters = new Dictionary<string, object>();
            if (group.Id > 0)
                parameters.Add("@id", @group.Id);
            else
                parameters.Add("@id", null);
            int? parentId = @group.ParentId;
            parameters.Add("@parentId",
                (parentId.GetValueOrDefault() <= 0 ? 0 : (parentId.HasValue ? 1 : 0)) != 0 ? @group.ParentId : null);
            parameters.Add("@name", @group.Name);
            parameters.Add("@description", @group.Description);
            return _database.Execute(commandText, parameters);
        }

        public int Update(Group group)
        {
            const string commandText = "Update Groups set ParentId = @parentId, Name = @name, Description = @description  where Id = @id";
            var parameters = new Dictionary<string, object>();
            var parentId = @group.ParentId;
            if ((parentId.GetValueOrDefault() <= 0 ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
                parameters.Add("@parentId", @group.ParentId.Value);
            else
                parameters.Add("@parentId", null);
            parameters.Add("@id", @group.Id);
            parameters.Add("@name", @group.Name);
            parameters.Add("@description", @group.Description);
            return _database.Execute(commandText, parameters);
        }

        public IDictionary<int, Group> GetAllGroups()
        {
            var dictionary1 = new Dictionary<int, Group>();
            foreach (var dictionary2 in _database.Query("" + "select g.Id, g.Name, g.Description, g.ParentId, g1.Name as ParentName " + ", sum(if(gu.UserId is not null, 1, 0)) as TotalMembers " + "from Groups g " + "left join GroupUsers gu on gu.GroupId = g.Id " + "left join Groups as g1 on g1.Id = g.ParentId " + "group  by g.Id, g.Name, g.Description, g.ParentId, ParentName " + "order by g.ParentId, g.Name"))
            {
                var group = new Group();
                group.Id = int.Parse(dictionary2["Id"]);
                group.Name = dictionary2["Name"];
                group.Description = dictionary2["Description"];
                if (!string.IsNullOrEmpty(dictionary2["ParentId"]))
                {
                    group.ParentId = int.Parse(dictionary2["ParentId"]);
                    group.ParentName = dictionary2["ParentName"];
                }
                if (!string.IsNullOrEmpty(dictionary2["TotalMembers"]))
                    group.TotalMembers = int.Parse(dictionary2["TotalMembers"]);
                dictionary1.Add(@group.Id, group);
            }
            return dictionary1;
        }

        public Group GetOneGroup(int groupId)
        {
            Group group = null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Groups where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          groupId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                group = new Group {Id = int.Parse(dictionary["Id"]), ParentId = new int?()};
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = int.Parse(dictionary["ParentId"]);
                group.Name = string.IsNullOrEmpty(dictionary["Name"]) ? null : dictionary["Name"];
                group.Description = string.IsNullOrEmpty(dictionary["Description"]) ? null : dictionary["Description"];
            }
            return group;
        }

        public int GetTotal(int? parentId, string keyword)
        {
            string commandText = "select count(*) from Groups where Name LIKE @param";
            var parameters = new Dictionary<string, object>
                             {
                                 {
                                     "@param",
                                     "%" + Utils.EncodeForLike(keyword) + "%"
                                 }
                             };
            if (parentId.HasValue)
            {
                int? nullable = parentId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (1)) != 0)
                {
                    commandText += " and ParentId = @param1";
                    parameters.Add("@param1", parentId.Value);
                }
            }
            return int.Parse(_database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<Group> GetPaging(int pageSize, int pageNumber, int? parentId, string keyword)
        {
            var parameters = new Dictionary<string, object>();
            string str = "select g1.*,g2.Name as ParentName from Groups g1 left join Groups g2 on g2.Id = g1.ParentId where g1.Name LIKE @param";
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (parentId.HasValue)
            {
                int? nullable = parentId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (1)) != 0)
                {
                    str += " and g1.ParentId = @param1";
                    parameters.Add("@param1", parentId.Value);
                }
            }
            string commandText = str + " order by g1.Name limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            var list = new List<Group>();
            foreach (Dictionary<string, string> dictionary in _database.Query(commandText, parameters))
            {
                var group = new Group();
                group.Id = int.Parse(dictionary["Id"]);
                group.Name = dictionary["Name"];
                group.ParentName = dictionary["ParentName"];
                group.Description = dictionary["Description"];
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = int.Parse(dictionary["ParentId"]);
                list.Add(group);
            }
            return list;
        }
    }
}