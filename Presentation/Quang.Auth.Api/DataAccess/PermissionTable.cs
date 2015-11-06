using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class PermissionTable : IPermissionTable
    {
        private MySQLDatabase _database;

        public PermissionTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public int Delete(int permissionId)
        {
            return this._database.Execute("Delete from Permissions where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) permissionId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            return this._database.Execute("Delete from Permissions where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
        }

        public int Insert(Permission permission)
        {
            string commandText = "Insert into Permissions (Id, Name, Description) values (@id, @name, @description)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (permission.Id > 0)
                parameters.Add("@id", (object)permission.Id);
            else
                parameters.Add("@id", (object)null);
            parameters.Add("@name", (object)permission.Name);
            parameters.Add("@description", (object)permission.Description);
            return this._database.Execute(commandText, parameters);
        }

        public int Update(Permission permission)
        {
            return this._database.Execute("Update Permissions set Name = @name, Description = @description  where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) permission.Id
        },
        {
          "@name",
          (object) permission.Name
        },
        {
          "@description",
          (object) permission.Description
        }
      });
        }

        public Permission GetOnePermission(int permissionId)
        {
            Permission permission = (Permission)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Permissions where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) permissionId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                permission = new Permission();
                permission.Id = int.Parse(dictionary["Id"]);
                permission.Name = string.IsNullOrEmpty(dictionary["Name"]) ? (string)null : dictionary["Name"];
                permission.Description = string.IsNullOrEmpty(dictionary["Description"]) ? (string)null : dictionary["Description"];
            }
            return permission;
        }

        public int GetTotal(string keyword)
        {
            return int.Parse(this._database.QueryValue("select count(*) from Permissions where Name LIKE @param", new Dictionary<string, object>()
      {
        {
          "@param",
          (object) ("%" + Utils.EncodeForLike(keyword) + "%")
        }
      }).ToString());
        }

        public IEnumerable<Permission> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "select * from Permissions where Name LIKE @param order by Name limit @rowNumber, @pageSize";
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<Permission> list = new List<Permission>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Permission()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Permission>)list;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "select * from Permissions where 1=1 order by Name";
            List<Permission> list = new List<Permission>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Permission()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Permission>)list;
        }

        public IEnumerable<PermissionGrant> GetPermissionGrants(int permissionId)
        {
            string commandText = "select * from PermissionGrants where PermissionId = @permissionId order by Id asc";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@permissionId", (object)permissionId);
            IList<PermissionGrant> list = (IList<PermissionGrant>)new List<PermissionGrant>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                PermissionGrant permissionGrant = new PermissionGrant();
                permissionGrant.Id = int.Parse(dictionary["Id"]);
                int num = 0;
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary["Type"]))
                    num = int.Parse(dictionary["Type"]);
                if (!string.IsNullOrEmpty(dictionary["IsExactPattern"]))
                    flag = bool.Parse(dictionary["IsExactPattern"]);
                permissionGrant.Type = num;
                permissionGrant.IsExactPattern = flag;
                permissionGrant.TermPattern = dictionary["TermPattern"];
                permissionGrant.TermExactPattern = dictionary["TermExactPattern"];
                list.Add(permissionGrant);
            }
            return (IEnumerable<PermissionGrant>)list;
        }

        public int UpdatePermissionGrants(int permissionId, IEnumerable<PermissionGrant> permissionGrants)
        {
            int[] currentItems = Enumerable.ToArray<int>(Enumerable.Select<PermissionGrant, int>(this.GetPermissionGrants(permissionId), (Func<PermissionGrant, int>)(m =>(int) m.Id)));
            PermissionGrant[] permissionGrantArray = Enumerable.ToArray<PermissionGrant>(Enumerable.Where<PermissionGrant>(permissionGrants, (Func<PermissionGrant, bool>)(m => !Enumerable.Contains<int>((IEnumerable<int>)currentItems, (int)m.Id))));
            int[] numArray = Enumerable.ToArray<int>(Enumerable.Where<int>((IEnumerable<int>)currentItems, (Func<int, bool>)(m => !Enumerable.Contains<int>(Enumerable.Select<PermissionGrant, int>(permissionGrants, (Func<PermissionGrant, int>)(n => (int)n.Id)), m))));
            foreach (PermissionGrant permissionGrant in Enumerable.ToArray<PermissionGrant>(Enumerable.Where<PermissionGrant>(permissionGrants, (Func<PermissionGrant, bool>)(m => Enumerable.Contains<int>((IEnumerable<int>)currentItems, (int)m.Id)))))
                this.UpdatePermissionGrant(permissionGrant);
            foreach (PermissionGrant permissionGrant in permissionGrantArray)
                this.InsertPermissionGrant(permissionGrant);
            this.DeletePermissionGrant((IEnumerable<int>)numArray);
            return 1;
        }

        private int InsertPermissionGrant(PermissionGrant permissionGrant)
        {
            return this._database.Execute("Insert into PermissionGrants (Id, PermissionId, Type, IsExactPattern, TermPattern, TermExactPattern) values (@id, @permissionId, @type, @isExactPattern, @termPattern, @termExactPattern)", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) null
        },
        {
          "@permissionId",
          (object) permissionGrant.PermissionId
        },
        {
          "@type",
          (object) permissionGrant.Type
        },
        {
          "@isExactPattern",
          (object) (permissionGrant.IsExactPattern ? 1 : 0)
        },
        {
          "@termPattern",
          (object) permissionGrant.TermPattern
        },
        {
          "@termExactPattern",
          (object) permissionGrant.TermExactPattern
        }
      });
        }

        private int UpdatePermissionGrant(PermissionGrant permissionGrant)
        {
            return this._database.Execute("Update PermissionGrants set PermissionId = @permissionId, Type = @type, IsExactPattern = @isExactPattern, TermPattern = @termPattern, TermExactPattern = @termExactPattern  where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) permissionGrant.Id
        },
        {
          "@permissionId",
          (object) permissionGrant.PermissionId
        },
        {
          "@type",
          (object) permissionGrant.Type
        },
        {
          "@isExactPattern",
          (object) (permissionGrant.IsExactPattern ? 1 : 0)
        },
        {
          "@termPattern",
          (object) permissionGrant.TermPattern
        },
        {
          "@termExactPattern",
          (object) permissionGrant.TermExactPattern
        }
      });
        }

        public int DeletePermissionGrant(IEnumerable<int> Ids)
        {
            if (Ids != null && Enumerable.Count<int>(Ids) > 0)
                return this._database.Execute("Delete from PermissionGrants where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")", new Dictionary<string, object>());
            return 0;
        }

        public IEnumerable<Permission> GetUserPermissions(int userId)
        {
            string commandText = "select p.* from UserPermissions up inner join Permissions p on p.Id = up.PermissionId where up.UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", (object)userId);
            List<Permission> list = new List<Permission>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Permission()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Permission>)list;
        }

        public IEnumerable<Permission> GetGroupPermissions(int groupId)
        {
            string commandText = "select p.* from GroupPermissions gp inner join Permissions p on p.Id = gp.PermissionId where gp.GroupId = @groupId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@groupId", (object)groupId);
            List<Permission> list = new List<Permission>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new Permission()
                {
                    Id = int.Parse(dictionary["Id"]),
                    Name = dictionary["Name"],
                    Description = dictionary["Description"]
                });
            return (IEnumerable<Permission>)list;
        }

        public int UpdateUserPermissions(int userId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null)
                permissionIds = (IEnumerable<int>)new int[0];
            int num = 1;
            int[] currentItems = Enumerable.ToArray<int>(Enumerable.Select<Permission, int>(this.GetUserPermissions(userId), (Func<Permission, int>)(m => m.Id)));
            int[] numArray1 = Enumerable.ToArray<int>(Enumerable.Where<int>(permissionIds, (Func<int, bool>)(m => !Enumerable.Contains<int>((IEnumerable<int>)currentItems, m))));
            int[] numArray2 = Enumerable.ToArray<int>(Enumerable.Where<int>((IEnumerable<int>)currentItems, (Func<int, bool>)(m => !Enumerable.Contains<int>(permissionIds, m))));
            foreach (int permissionId in numArray1)
                this.InsertUserPermission(userId, permissionId);
            this.DeleteUserPermissions(userId, (IEnumerable<int>)numArray2);
            return num;
        }

        private int InsertUserPermission(int userId, int permissionId)
        {
            return this._database.Execute("Insert into UserPermissions (Id, UserId, PermissionId) values (@id, @userId, @permissionId)", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) null
        },
        {
          "@userId",
          (object) userId
        },
        {
          "@permissionId",
          (object) permissionId
        }
      });
        }

        public int DeleteUserPermissions(int userId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null || Enumerable.Count<int>(permissionIds) <= 0)
                return 0;
            return this._database.Execute("Delete from UserPermissions where UserId = @userId and PermissionId in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(permissionIds)) + ")", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userId
        }
      });
        }

        public int UpdateGroupPermissions(int groupId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null)
                permissionIds = (IEnumerable<int>)new int[0];
            int num = 1;
            int[] currentItems = Enumerable.ToArray<int>(Enumerable.Select<Permission, int>(this.GetGroupPermissions(groupId), (Func<Permission, int>)(m => (int)m.Id)));
            int[] numArray1 = Enumerable.ToArray<int>(Enumerable.Where<int>(permissionIds, (Func<int, bool>)(m => !Enumerable.Contains<int>((IEnumerable<int>)currentItems, m))));
            int[] numArray2 = Enumerable.ToArray<int>(Enumerable.Where<int>((IEnumerable<int>)currentItems, (Func<int, bool>)(m => !Enumerable.Contains<int>(permissionIds, m))));
            foreach (int permissionId in numArray1)
                this.InsertGroupPermission(groupId, permissionId);
            this.DeleteGroupPermissions(groupId, (IEnumerable<int>)numArray2);
            return num;
        }

        private int InsertGroupPermission(int groupId, int permissionId)
        {
            return this._database.Execute("Insert into GroupPermissions (Id, GroupId, PermissionId) values (@id, @groupId, @permissionId)", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) null
        },
        {
          "@groupId",
          (object) groupId
        },
        {
          "@permissionId",
          (object) permissionId
        }
      });
        }

        public int DeleteGroupPermissions(int groupId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null || Enumerable.Count<int>(permissionIds) <= 0)
                return 0;
            return this._database.Execute("Delete from GroupPermissions where GroupId = @groupId and PermissionId in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(permissionIds)) + ")", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        }
      });
        }

        public IEnumerable<PermissionGrant> GetAllPermissionGrantBelongToUser(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            IList<PermissionGrant> list = (IList<PermissionGrant>)new List<PermissionGrant>();
            string commandText = "" + "select pg.* " + "from PermissionGrants pg " + "where pg.PermissionId in ( " + "    select t.PermissionId " + "    from (" + "        (" + "            select distinct up.PermissionId as PermissionId from UserPermissions as up where up.UserId = @userId " + "        )" + "        UNION DISTINCT " + "        (   select distinct gp.PermissionId as PermissionId " + "            from GroupUsers as gu " + "            inner join GroupPermissions as gp on gp.GroupId = gu.GroupId " + "            where gu.UserId = @userId " + "        )" + "    ) as t " + ")";
            parameters.Add("@userId", (object)userId);
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                PermissionGrant permissionGrant = new PermissionGrant();
                permissionGrant.Id = int.Parse(dictionary["Id"]);
                int num = 0;
                bool flag = false;
                if (!string.IsNullOrEmpty(dictionary["Type"]))
                    num = int.Parse(dictionary["Type"]);
                if (!string.IsNullOrEmpty(dictionary["IsExactPattern"]))
                    flag = bool.Parse(dictionary["IsExactPattern"]);
                permissionGrant.Type = num;
                permissionGrant.IsExactPattern = flag;
                permissionGrant.TermPattern = dictionary["TermPattern"];
                permissionGrant.TermExactPattern = dictionary["TermExactPattern"];
                list.Add(permissionGrant);
            }
            return (IEnumerable<PermissionGrant>)list;
        }

        public IEnumerable<int> GetAllUserIdHasPermission(int permissionId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select distinct up.UserId as UserId from UserPermissions as up where up.PermissionId = @permissionId " + "UNION DISTINCT " + "select distinct gu.UserId as UserId " + "from GroupUsers as gu " + "inner join GroupPermissions as gp on gp.GroupId = gu.GroupId " + "where gp.PermissionId = @permissionId ";
            parameters.Add("@permissionId", (object)permissionId);
            List<Dictionary<string, string>> list1 = this._database.Query(commandText, parameters);
            IList<int> list2 = (IList<int>)new List<int>();
            foreach (Dictionary<string, string> dictionary in list1)
                list2.Add(int.Parse(dictionary["UserId"]));
            return (IEnumerable<int>)list2;
        }

        public int DeleteUserPermissions(int userId)
        {
            return this._database.Execute("Delete from UserPermissions where UserId = @userId", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userId
        }
      });
        }

        public int DeleteGroupPermissions(int groupId)
        {
            return this._database.Execute("Delete from GroupPermissions where GroupId = @groupId", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        }
      });
        }

        public int DeletePermissionGrants(int permissionId)
        {
            return this._database.Execute("Delete from PermissionGrants where PermissionId = @permissionId", new Dictionary<string, object>()
      {
        {
          "@permissionId",
          (object) permissionId
        }
      });
        }
    }
}