using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quang.Auth.Api.DataAccess
{
    public class PermissionTable : IPermissionTable
    {
        private readonly MySQLDatabase _database;

        public PermissionTable(MySQLDatabase database)
        {
            _database = database;
        }

        public int Delete(int permissionId)
        {
            return _database.Execute("Delete from Permissions where Id = @id", new Dictionary<string, object>
                                                                               {
        {
          "@id",
          permissionId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            return _database.Execute("Delete from Permissions where Id in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
        }

        public int Insert(Permission permission)
        {
            const string commandText = "Insert into Permissions (Id, Name, Description) values (@id, @name, @description)";
            var parameters = new Dictionary<string, object>();
            if (permission.Id > 0)
                parameters.Add("@id", permission.Id);
            else
                parameters.Add("@id", null);
            parameters.Add("@name", permission.Name);
            parameters.Add("@description", permission.Description);
            return _database.Execute(commandText, parameters);
        }

        public int Update(Permission permission)
        {
            return _database.Execute("Update Permissions set Name = @name, Description = @description  where Id = @id", new Dictionary<string, object>
                                                                                                                        {
        {
          "@id",
          permission.Id
        },
        {
          "@name",
          permission.Name
        },
        {
          "@description",
          permission.Description
        }
      });
        }

        public Permission GetOnePermission(int permissionId)
        {
            var permission = (Permission)null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Permissions where Id = @id", new Dictionary<string, object>
                                                                                                                {
        {
          "@id",
          permissionId
        }
      });
            if (list != null && list.Count == 1)
            {
                var dictionary = list[0];
                permission = new Permission
                             {
                                 Id = int.Parse(dictionary["Id"]),
                                 Name =
                                     string.IsNullOrEmpty(dictionary["Name"])
                                         ? null
                                         : dictionary["Name"],
                                 Description =
                                     string.IsNullOrEmpty(dictionary["Description"])
                                         ? null
                                         : dictionary["Description"]
                             };
            }
            return permission;
        }

        public int GetTotal(string keyword)
        {
            return int.Parse(_database.QueryValue("select count(*) from Permissions where Name LIKE @param", new Dictionary<string, object>
                                                                                                             {
        {
          "@param",
          "%" + Utils.EncodeForLike(keyword) + "%"
        }
      }).ToString());
        }

        public IEnumerable<Permission> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "select * from Permissions where Name LIKE @param order by Name limit @rowNumber, @pageSize";
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            var list = _database.Query(commandText, parameters).Select(dictionary => new Permission
                                                                                     {
                                                                                              Id = int.Parse(dictionary["Id"]), Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                          }).ToList();
            return list;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "select * from Permissions where 1=1 order by Name";
            var list = _database.Query(commandText, parameters).Select(dictionary => new Permission
                                                                                     {
                                                                                              Id = int.Parse(dictionary["Id"]), Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                          }).ToList();
            return list;
        }

        public IEnumerable<PermissionGrant> GetPermissionGrants(int permissionId)
        {
            const string commandText = "select * from PermissionGrants where PermissionId = @permissionId order by Id asc";
            var parameters = new Dictionary<string, object> {{"@permissionId", permissionId}};
            IList<PermissionGrant> list = new List<PermissionGrant>();
            foreach (var dictionary in _database.Query(commandText, parameters))
            {
                var permissionGrant = new PermissionGrant {Id = int.Parse(dictionary["Id"])};
                var num = 0;
                var flag = false;
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
            return list;
        }

        public int UpdatePermissionGrants(int permissionId, IEnumerable<PermissionGrant> permissionGrants)
        {
            int[] currentItems = GetPermissionGrants(permissionId).Select(m =>(int) m.Id).ToArray();
            PermissionGrant[] permissionGrantArray = permissionGrants.Where(m => !currentItems.Contains((int)m.Id)).ToArray();
            int[] numArray = currentItems.Where(m => !permissionGrants.Select(n => (int)n.Id).Contains(m)).ToArray();
            foreach (PermissionGrant permissionGrant in permissionGrants.Where(m => currentItems.Contains((int)m.Id)).ToArray())
                UpdatePermissionGrant(permissionGrant);
            foreach (PermissionGrant permissionGrant in permissionGrantArray)
                InsertPermissionGrant(permissionGrant);
            DeletePermissionGrant(numArray);
            return 1;
        }

        private int InsertPermissionGrant(PermissionGrant permissionGrant)
        {
            return _database.Execute("Insert into PermissionGrants (Id, PermissionId, Type, IsExactPattern, TermPattern, TermExactPattern) values (@id, @permissionId, @type, @isExactPattern, @termPattern, @termExactPattern)", new Dictionary<string, object>()
      {
        {
          "@id",
          null
        },
        {
          "@permissionId",
          permissionGrant.PermissionId
        },
        {
          "@type",
          permissionGrant.Type
        },
        {
          "@isExactPattern",
          permissionGrant.IsExactPattern ? 1 : 0
        },
        {
          "@termPattern",
          permissionGrant.TermPattern
        },
        {
          "@termExactPattern",
          permissionGrant.TermExactPattern
        }
      });
        }

        private int UpdatePermissionGrant(PermissionGrant permissionGrant)
        {
            return _database.Execute("Update PermissionGrants set PermissionId = @permissionId, Type = @type, IsExactPattern = @isExactPattern, TermPattern = @termPattern, TermExactPattern = @termExactPattern  where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          permissionGrant.Id
        },
        {
          "@permissionId",
          permissionGrant.PermissionId
        },
        {
          "@type",
          permissionGrant.Type
        },
        {
          "@isExactPattern",
          permissionGrant.IsExactPattern ? 1 : 0
        },
        {
          "@termPattern",
          permissionGrant.TermPattern
        },
        {
          "@termExactPattern",
          permissionGrant.TermExactPattern
        }
      });
        }

        public int DeletePermissionGrant(IEnumerable<int> Ids)
        {
            if (Ids != null && Ids.Any())
                return _database.Execute("Delete from PermissionGrants where Id in (" + string.Join(",", Ids.ToArray()) + ")", new Dictionary<string, object>());
            return 0;
        }

        public IEnumerable<Permission> GetUserPermissions(int userId)
        {
            const string commandText = "select p.* from UserPermissions up inner join Permissions p on p.Id = up.PermissionId where up.UserId = @userId";
            var parameters = new Dictionary<string, object> {{"@userId", userId}};
            var list = _database.Query(commandText, parameters).Select(dictionary => new Permission
                                                                                          {
                                                                                              Id = int.Parse(dictionary["Id"]), Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                          }).ToList();
            return list;
        }

        public IEnumerable<Permission> GetGroupPermissions(int groupId)
        {
            const string commandText = "select p.* from GroupPermissions gp inner join Permissions p on p.Id = gp.PermissionId where gp.GroupId = @groupId";
            var parameters = new Dictionary<string, object> {{"@groupId", groupId}};
            var list = _database.Query(commandText, parameters).Select(dictionary => new Permission
                                                                                     {
                                                                                              Id = int.Parse(dictionary["Id"]), Name = dictionary["Name"], Description = dictionary["Description"]
                                                                                          }).ToList();
            return list;
        }

        public int UpdateUserPermissions(int userId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null)
                permissionIds = new int[0];
            const int num = 1;
            int[] currentItems = GetUserPermissions(userId).Select(m => m.Id).ToArray();
            int[] numArray1 = permissionIds.Where(m => !currentItems.Contains(m)).ToArray();
            int[] numArray2 = currentItems.Where(m => !permissionIds.Contains(m)).ToArray();
            foreach (int permissionId in numArray1)
                InsertUserPermission(userId, permissionId);
            DeleteUserPermissions(userId, numArray2);
            return num;
        }

        private int InsertUserPermission(int userId, int permissionId)
        {
            return _database.Execute("Insert into UserPermissions (Id, UserId, PermissionId) values (@id, @userId, @permissionId)", new Dictionary<string, object>()
      {
        {
          "@id",
          null
        },
        {
          "@userId",
          userId
        },
        {
          "@permissionId",
          (object) permissionId
        }
      });
        }

        public int DeleteUserPermissions(int userId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null || !permissionIds.Any())
                return 0;
            return _database.Execute("Delete from UserPermissions where UserId = @userId and PermissionId in (" + string.Join(",", permissionIds.ToArray()) + ")", new Dictionary<string, object>()
      {
        {
          "@userId",
          userId
        }
      });
        }

        public int UpdateGroupPermissions(int groupId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null)
                permissionIds = new int[0];
            const int num = 1;
            int[] currentItems = GetGroupPermissions(groupId).Select(m => m.Id).ToArray();
            int[] numArray1 = permissionIds.Where(m => !currentItems.Contains(m)).ToArray();
            int[] numArray2 = currentItems.Where(m => !permissionIds.Contains(m)).ToArray();
            foreach (int permissionId in numArray1)
                InsertGroupPermission(groupId, permissionId);
            DeleteGroupPermissions(groupId, numArray2);
            return num;
        }

        private int InsertGroupPermission(int groupId, int permissionId)
        {
            return _database.Execute("Insert into GroupPermissions (Id, GroupId, PermissionId) values (@id, @groupId, @permissionId)", new Dictionary<string, object>()
      {
        {
          "@id",
          null
        },
        {
          "@groupId",
          groupId
        },
        {
          "@permissionId",
          permissionId
        }
      });
        }

        public int DeleteGroupPermissions(int groupId, IEnumerable<int> permissionIds)
        {
            if (permissionIds == null || !permissionIds.Any())
                return 0;
            return _database.Execute("Delete from GroupPermissions where GroupId = @groupId and PermissionId in (" + string.Join(",", permissionIds.ToArray()) + ")", new Dictionary<string, object>()
      {
        {
          "@groupId",
          groupId
        }
      });
        }

        public IEnumerable<PermissionGrant> GetAllPermissionGrantBelongToUser(int userId)
        {
            var parameters = new Dictionary<string, object>();
            var list = (IList<PermissionGrant>)new List<PermissionGrant>();
            const string commandText = "" + "select pg.* " + "from PermissionGrants pg " + "where pg.PermissionId in ( " + "    select t.PermissionId " + "    from (" + "        (" + "            select distinct up.PermissionId as PermissionId from UserPermissions as up where up.UserId = @userId " + "        )" + "        UNION DISTINCT " + "        (   select distinct gp.PermissionId as PermissionId " + "            from GroupUsers as gu " + "            inner join GroupPermissions as gp on gp.GroupId = gu.GroupId " + "            where gu.UserId = @userId " + "        )" + "    ) as t " + ")";
            parameters.Add("@userId", userId);
            foreach (var dictionary in _database.Query(commandText, parameters))
            {
                var permissionGrant = new PermissionGrant {Id = int.Parse(dictionary["Id"])};
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
            return list;
        }

        public IEnumerable<int> GetAllUserIdHasPermission(int permissionId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select distinct up.UserId as UserId from UserPermissions as up where up.PermissionId = @permissionId " + "UNION DISTINCT " + "select distinct gu.UserId as UserId " + "from GroupUsers as gu " + "inner join GroupPermissions as gp on gp.GroupId = gu.GroupId " + "where gp.PermissionId = @permissionId ";
            parameters.Add("@permissionId", permissionId);
            var list1 = _database.Query(commandText, parameters);
            return list1.Select(dictionary => int.Parse(dictionary["UserId"])).ToList();
        }

        public int DeleteUserPermissions(int userId)
        {
            return _database.Execute("Delete from UserPermissions where UserId = @userId", new Dictionary<string, object>
                                                                                           {
        {
          "@userId",
          userId
        }
      });
        }

        public int DeleteGroupPermissions(int groupId)
        {
            return _database.Execute("Delete from GroupPermissions where GroupId = @groupId", new Dictionary<string, object>
                                                                                              {
        {
          "@groupId",
          groupId
        }
      });
        }

        public int DeletePermissionGrants(int permissionId)
        {
            return _database.Execute("Delete from PermissionGrants where PermissionId = @permissionId", new Dictionary<string, object>
                                                                                                        {
        {
          "@permissionId",
          permissionId
        }
      });
        }
    }
}