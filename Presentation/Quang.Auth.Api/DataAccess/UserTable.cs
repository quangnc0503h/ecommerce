using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class UserTable : IUserTable
    {
        private MySQLDatabase _database;

        public UserTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public IEnumerable<User> GetAllUsers()
        {
            IList<User> list = (IList<User>)new List<User>();
            foreach (Dictionary<string, string> dictionary in this._database.Query("Select * from Users order by Name"))
                list.Add(new User()
                {
                    Id = int.Parse(dictionary["Id"]),
                    UserName = dictionary["UserName"],
                    Email = dictionary["Email"],
                    PhoneNumber = dictionary["PhoneNumber"]
                });
            return (IEnumerable<User>)list;
        }

        public IEnumerable<User> GetUsersByGroup(int groupId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "select u.Id, u.UserName, u.Email, u.PhoneNumber from Users u where exists (select gu.* from GroupUsers gu where gu.GroupId = @groupId and gu.UserId = u.Id)";
            parameters.Add("@groupId", (object)groupId);
            List<User> list = new List<User>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new User()
                {
                    Id = int.Parse(dictionary["Id"]),
                    UserName = dictionary["UserName"],
                    Email = dictionary["Email"],
                    PhoneNumber = dictionary["PhoneNumber"]
                });
            return (IEnumerable<User>)list;
        }

        public IEnumerable<User> GetUsersByRole(int roleId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "select u.Id, u.UserName, u.Email, u.PhoneNumber from UserRoles ur inner join Users u on u.Id = ur.UserId where ur.RoleId = @roleId limit 15";
            parameters.Add("@roleId", (object)roleId);
            List<User> list = new List<User>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new User()
                {
                    Id = int.Parse(dictionary["Id"]),
                    UserName = dictionary["UserName"],
                    Email = dictionary["Email"],
                    PhoneNumber = dictionary["PhoneNumber"]
                });
            return (IEnumerable<User>)list;
        }

        public User GetOneUser(int userId)
        {
            User user = (User)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Users where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) userId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                user = new User();
                user.Id = int.Parse(dictionary["Id"]);
                user.UserName = dictionary["UserName"];
                user.Email = dictionary["Email"];
                user.PhoneNumber = dictionary["PhoneNumber"];
            }
            return user;
        }

        public int GetTotal(int? groupId, string keyword)
        {
            string commandText = "select count(u.id) from Users u where (u.UserName like @param or u.Email like @param)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (groupId.HasValue)
            {
                int? nullable = groupId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                {
                    commandText += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                    parameters.Add("@param1", (object)groupId.Value);
                }
            }
            return int.Parse(this._database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<User> GetPaging(int pageSize, int pageNumber, int? groupId, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string str = "select u.* from Users u where (u.UserName like @param or u.Email like @param)";
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (groupId.HasValue)
            {
                int? nullable = groupId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                {
                    str += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                    parameters.Add("@param1", (object)groupId.Value);
                }
            }
            string commandText = str + " order by u.UserName limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<User> list = new List<User>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new User()
                {
                    Id = int.Parse(dictionary["Id"]),
                    UserName = dictionary["UserName"],
                    Email = dictionary["Email"],
                    PhoneNumber = dictionary["PhoneNumber"]
                });
            return (IEnumerable<User>)list;
        }

        public IEnumerable<Group> GetGroupsByUser(int userId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string commandText = "" + "select g.* " + "from GroupUsers gu " + "inner join Users u on u.Id = gu.UserId " + "inner join Groups g on g.Id = gu.GroupId " + "where u.Id = @userId";
            parameters.Add("@userId", (object)userId);
            IList<Group> list = (IList<Group>)new List<Group>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                Group group = new Group();
                group.Id = int.Parse(dictionary["Id"]);
                group.ParentId = new int?();
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = new int?(int.Parse(dictionary["ParentId"]));
                group.Name = dictionary["Name"];
                group.Description = dictionary["Description"];
                list.Add(group);
            }
            return (IEnumerable<Group>)list;
        }

        public int addUserToGroup(int groupId, int userId)
        {
            return this._database.Execute("Insert into GroupUsers (GroupId, UserId) values (@groupId, @userId)", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        },
        {
          "@userId",
          (object) userId
        }
      });
        }

        public int removeUserFromGroup(int groupId, int userId)
        {
            return this._database.Execute("Delete from GroupUsers where GroupId = @groupId and UserId = @userId", new Dictionary<string, object>()
      {
        {
          "@groupId",
          (object) groupId
        },
        {
          "@userId",
          (object) userId
        }
      });
        }

        public UserApp GetUserApp(int userId, AppApiType appType)
        {
            UserApp userApp = (UserApp)null;
            List<Dictionary<string, string>> list = this._database.Query("" + "select ua.* from UserApps ua " + "inner join Users u on u.id = ua.UserId " + "where u.Id = @userId " + "and ua.ApiType = @apiType", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userId
        },
        {
          "@apiType",
          (object) appType.GetHashCode()
        }
      });
            if (list != null && list.Count == 1)
                userApp = this._parseUserAppObj(list[0]);
            return userApp;
        }

        public UserApp GetUserApp(string userApiKey, bool? isActive)
        {
            UserApp userApp = (UserApp)null;
            string commandText = "" + "select ua.* from UserApps ua " + "inner join Users u on u.id = ua.UserId " + "where ua.ApiKey = @apiKey";
            if (isActive.HasValue)
                commandText = commandText + (object)" and ua.IsActive = " + (string)(object)(isActive.Value ? 1 : 0);
            Dictionary<string, object> parameters = new Dictionary<string, object>()
      {
        {
          "@apiKey",
          (object) userApiKey
        }
      };
            List<Dictionary<string, string>> list = this._database.Query(commandText, parameters);
            if (list != null && list.Count == 1)
                userApp = this._parseUserAppObj(list[0]);
            return userApp;
        }

        public int InsertUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
                return 0;
            if (userApp.Id > 0)
                return this.UpdateUserApp(userApp);
            return this._database.Execute("Insert Into UserApps  " + "(UserId,  IsActive,  ApiType,  ApiName,  ApiKey,  ApiSecret,  AppHosts,  AppIps) Values " + "(@userId, @isActive, @apiType, @apiName, @apiKey, @apiSecret, @appHosts, @appIps)", new Dictionary<string, object>()
      {
        {
          "@userId",
          (object) userApp.UserId
        },
        {
          "@isActive",
          (object) (userApp.IsActive ? 1 : 0)
        },
        {
          "@apiType",
          (object) userApp.ApiType.GetHashCode()
        },
        {
          "@apiName",
          (object) userApp.ApiName
        },
        {
          "@apiKey",
          (object) userApp.ApiKey
        },
        {
          "@apiSecret",
          (object) userApp.ApiSecret
        },
        {
          "@appHosts",
          (object) userApp.AppHosts
        },
        {
          "@appIps",
          (object) userApp.AppIps
        }
      });
        }

        public int UpdateUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
                return 0;
            if (userApp.Id <= 0)
                return this.InsertUserApp(userApp);
            return this._database.Execute("Update UserApps Set " + "UserId = @userId, IsActive = @isActive, ApiType = @apiType, ApiName = @apiName, ApiKey = @apiKey, ApiSecret = @apiSecret, AppHosts = @appHosts, AppIps = @appIps " + "where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) userApp.Id
        },
        {
          "@userId",
          (object) userApp.UserId
        },
        {
          "@isActive",
          (object) (userApp.IsActive ? 1 : 0)
        },
        {
          "@apiType",
          (object) userApp.ApiType.GetHashCode()
        },
        {
          "@apiName",
          (object) userApp.ApiName
        },
        {
          "@apiKey",
          (object) userApp.ApiKey
        },
        {
          "@apiSecret",
          (object) userApp.ApiSecret
        },
        {
          "@appHosts",
          (object) userApp.AppHosts
        },
        {
          "@appIps",
          (object) userApp.AppIps
        }
      });
        }

        private UserApp _parseUserAppObj(Dictionary<string, string> row)
        {
            UserApp userApp = new UserApp();
            userApp.Id = int.Parse(row["Id"]);
            userApp.UserId = int.Parse(row["UserId"]);
            userApp.IsActive = false;
            if (!string.IsNullOrEmpty(row["IsActive"]))
                userApp.IsActive = int.Parse(row["IsActive"]) > 0;
            userApp.ApiType = AppApiType.None;
            if (!string.IsNullOrEmpty(row["ApiType"]) && int.Parse(row["ApiType"]) == 1)
                userApp.ApiType = AppApiType.ClientApi;
            userApp.ApiName = row["ApiName"];
            userApp.ApiKey = row["ApiKey"];
            userApp.ApiSecret = row["ApiSecret"];
            userApp.AppHosts = row["AppHosts"];
            userApp.AppIps = row["AppIps"];
            return userApp;
        }
    }
}