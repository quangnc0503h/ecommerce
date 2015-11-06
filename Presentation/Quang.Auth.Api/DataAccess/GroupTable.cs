using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class GroupTable : IGroupTable
    {
        private MySQLDatabase _database;

        public GroupTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public int Delete(int groupId)
        {
            return this._database.Execute("Delete from Groups where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) groupId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            return this._database.Execute("Delete from Groups where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
        }

        public int Insert(Group group)
        {
            string commandText = "Insert into Groups (Id, ParentId, Name, Description) values (@id, @parentId, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (group.Id > 0)
                parameters.Add("@id", (object)group.Id);
            else
                parameters.Add("@id", (object)null);
            int? parentId = (int)group.ParentId;
            if ((parentId.GetValueOrDefault() <= 0 ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
                parameters.Add("@parentId", (object)group.ParentId);
            else
                parameters.Add("@parentId", (object)null);
            parameters.Add("@name", (object)group.Name);
            parameters.Add("@description", (object)group.Description);
            return this._database.Execute(commandText, parameters);
        }

        public int Update(Group group)
        {
            string commandText = "Update Groups set ParentId = @parentId, Name = @name, Description = @description  where Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int? parentId = (int)group.ParentId;
            if ((parentId.GetValueOrDefault() <= 0 ? 0 : (parentId.HasValue ? 1 : 0)) != 0)
                parameters.Add("@parentId", (object)group.ParentId.Value);
            else
                parameters.Add("@parentId", (object)null);
            parameters.Add("@id", (object)group.Id);
            parameters.Add("@name", (object)group.Name);
            parameters.Add("@description", (object)group.Description);
            return this._database.Execute(commandText, parameters);
        }

        public IDictionary<int, Group> GetAllGroups()
        {
            IDictionary<int, Group> dictionary1 = (IDictionary<int, Group>)new Dictionary<int, Group>();
            foreach (Dictionary<string, string> dictionary2 in this._database.Query("" + "select g.Id, g.Name, g.Description, g.ParentId, g1.Name as ParentName " + ", sum(if(gu.UserId is not null, 1, 0)) as TotalMembers " + "from Groups g " + "left join GroupUsers gu on gu.GroupId = g.Id " + "left join Groups as g1 on g1.Id = g.ParentId " + "group  by g.Id, g.Name, g.Description, g.ParentId, ParentName " + "order by g.ParentId, g.Name"))
            {
                Group group = new Group();
                group.Id = int.Parse(dictionary2["Id"]);
                group.Name = dictionary2["Name"];
                group.Description = dictionary2["Description"];
                if (!string.IsNullOrEmpty(dictionary2["ParentId"]))
                {
                    group.ParentId = new int?(int.Parse(dictionary2["ParentId"]));
                    group.ParentName = dictionary2["ParentName"];
                }
                if (!string.IsNullOrEmpty(dictionary2["TotalMembers"]))
                    group.TotalMembers = int.Parse(dictionary2["TotalMembers"]);
                dictionary1.Add((int)group.Id, group);
            }
            return dictionary1;
        }

        public Group GetOneGroup(int groupId)
        {
            Group group = (Group)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Groups where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) groupId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                group = new Group();
                group.Id = int.Parse(dictionary["Id"]);
                group.ParentId = new int?();
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = new int?(int.Parse(dictionary["ParentId"]));
                group.Name = string.IsNullOrEmpty(dictionary["Name"]) ? (string)null : dictionary["Name"];
                group.Description = string.IsNullOrEmpty(dictionary["Description"]) ? (string)null : dictionary["Description"];
            }
            return group;
        }

        public int GetTotal(int? parentId, string keyword)
        {
            string commandText = "select count(*) from Groups where Name LIKE @param";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (parentId.HasValue)
            {
                int? nullable = parentId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                {
                    commandText += " and ParentId = @param1";
                    parameters.Add("@param1", (object)parentId.Value);
                }
            }
            return int.Parse(this._database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<Group> GetPaging(int pageSize, int pageNumber, int? parentId, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string str = "select g1.*,g2.Name as ParentName from Groups g1 left join Groups g2 on g2.Id = g1.ParentId where g1.Name LIKE @param";
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (parentId.HasValue)
            {
                int? nullable = parentId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                {
                    str += " and g1.ParentId = @param1";
                    parameters.Add("@param1", (object)parentId.Value);
                }
            }
            string commandText = str + " order by g1.Name limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<Group> list = new List<Group>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                Group group = new Group();
                group.Id = int.Parse(dictionary["Id"]);
                group.Name = dictionary["Name"];
                group.ParentName = dictionary["ParentName"];
                group.Description = dictionary["Description"];
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = new int?(int.Parse(dictionary["ParentId"]));
                list.Add(group);
            }
            return (IEnumerable<Group>)list;
        }
    }
}