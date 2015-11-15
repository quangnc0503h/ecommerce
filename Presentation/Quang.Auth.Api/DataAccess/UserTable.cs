using System.Linq;
using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System.Collections.Generic;

namespace Quang.Auth.Api.DataAccess
{
    public class UserTable : IUserTable
    {
        private readonly MySQLDatabase _database;

        public UserTable(MySQLDatabase database)
        {
            _database = database;
        }

        public IEnumerable<User> GetAllUsers()
        {
            var list = (IList<User>)new List<User>();
            foreach (var dictionary in _database.Query("Select * from Users order by Name"))
                list.Add(new User
                         {
                    Id = int.Parse(dictionary["Id"]),
                    UserName = dictionary["UserName"],
                    Email = dictionary["Email"],
                    PhoneNumber = dictionary["PhoneNumber"]
                });
            return list;
        }

        public IEnumerable<User> GetUsersByGroup(int groupId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "select u.Id, u.UserName, u.Email, u.PhoneNumber from Users u where exists (select gu.* from GroupUsers gu where gu.GroupId = @groupId and gu.UserId = u.Id)";
            parameters.Add("@groupId", groupId);
            var list = _database.Query(commandText, parameters).Select(dictionary => new User
                                                                                          {
                                                                                              Id = int.Parse(dictionary["Id"]), UserName = dictionary["UserName"], Email = dictionary["Email"], PhoneNumber = dictionary["PhoneNumber"]
                                                                                          }).ToList();
            return list;
        }

        public IEnumerable<User> GetUsersByRole(int roleId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "select u.Id, u.UserName, u.Email, u.PhoneNumber from UserRoles ur inner join Users u on u.Id = ur.UserId where ur.RoleId = @roleId limit 15";
            parameters.Add("@roleId", roleId);
            return _database.Query(commandText, parameters).Select(dictionary => new User
                                                                                      {
                                                                                          Id = int.Parse(dictionary["Id"]), UserName = dictionary["UserName"], Email = dictionary["Email"], PhoneNumber = dictionary["PhoneNumber"]
                                                                                      }).ToList();
        }

        public User GetOneUser(int userId)
        {
            User user = null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Users where Id = @id", new Dictionary<string, object>
                                                                                                          {
        {
          "@id",
          userId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                user = new User
                       {
                           Id = int.Parse(dictionary["Id"]),
                           UserName = dictionary["UserName"],
                           Email = dictionary["Email"],
                           PhoneNumber = dictionary["PhoneNumber"]
                       };
            }
            return user;
        }

        public int GetTotal(int? groupId, string keyword)
        {
            string commandText = "select count(u.id) from Users u where (u.UserName like @param or u.Email like @param)";
            var parameters = new Dictionary<string, object> {{"@param", "%" + Utils.EncodeForLike(keyword) + "%"}};
            if (groupId.HasValue)
            {
                int? nullable = groupId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (1)) != 0)
                {
                    commandText += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                    parameters.Add("@param1", groupId.Value);
                }
            }
            return int.Parse(_database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<User> GetPaging(int pageSize, int pageNumber, int? groupId, string keyword)
        {
            var parameters = new Dictionary<string, object>();
            string str = "select u.* from Users u where (u.UserName like @param or u.Email like @param)";
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (groupId.HasValue)
            {
                int? nullable = groupId;
                if ((nullable.GetValueOrDefault() <= 0 ? 0 : (1)) != 0)
                {
                    str += " and exists (select gu.* from GroupUsers gu where gu.GroupId = @param1 and gu.UserId = u.Id)";
                    parameters.Add("@param1", groupId.Value);
                }
            }
            string commandText = str + " order by u.UserName limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            return _database.Query(commandText, parameters).Select(dictionary => new User
                                                                                      {
                                                                                          Id = int.Parse(dictionary["Id"]), UserName = dictionary["UserName"], Email = dictionary["Email"], PhoneNumber = dictionary["PhoneNumber"]
                                                                                      }).ToList();
        }

        public IEnumerable<Group> GetGroupsByUser(int userId)
        {
            var parameters = new Dictionary<string, object>();
            const string commandText = "" + "select g.* " + "from GroupUsers gu " + "inner join Users u on u.Id = gu.UserId " + "inner join Groups g on g.Id = gu.GroupId " + "where u.Id = @userId";
            parameters.Add("@userId", userId);
            IList<Group> list = new List<Group>();
            foreach (Dictionary<string, string> dictionary in _database.Query(commandText, parameters))
            {
                var group = new Group();
                group.Id = int.Parse(dictionary["Id"]);
                group.ParentId = new int?();
                if (!string.IsNullOrEmpty(dictionary["ParentId"]))
                    group.ParentId = int.Parse(dictionary["ParentId"]);
                group.Name = dictionary["Name"];
                group.Description = dictionary["Description"];
                list.Add(group);
            }
            return list;
        }

        public int addUserToGroup(int groupId, int userId)
        {
            return _database.Execute("Insert into GroupUsers (GroupId, UserId) values (@groupId, @userId)", new Dictionary<string, object>
                                                                                                            {
        {
          "@groupId",
          groupId
        },
        {
          "@userId",
          userId
        }
      });
        }

        public int removeUserFromGroup(int groupId, int userId)
        {
            return _database.Execute("Delete from GroupUsers where GroupId = @groupId and UserId = @userId", new Dictionary<string, object>
                                                                                                             {
        {
          "@groupId",
          groupId
        },
        {
          "@userId",
          userId
        }
      });
        }

        public UserApp GetUserApp(int userId, AppApiType appType)
        {
            UserApp userApp = null;
            List<Dictionary<string, string>> list = _database.Query("" + "select ua.* from UserApps ua " + "inner join Users u on u.id = ua.UserId " + "where u.Id = @userId " + "and ua.ApiType = @apiType", new Dictionary<string, object>
                                                                                                                                                                                                              {
        {
          "@userId",
          userId
        },
        {
          "@apiType",
          appType.GetHashCode()
        }
      });
            if (list != null && list.Count == 1)
                userApp = _parseUserAppObj(list[0]);
            return userApp;
        }

        public UserApp GetUserApp(string userApiKey, bool? isActive)
        {
            UserApp userApp = null;
            string commandText = "" + "select ua.* from UserApps ua " + "inner join Users u on u.id = ua.UserId " + "where ua.ApiKey = @apiKey";
            if (isActive.HasValue)
                commandText = commandText + (object)" and ua.IsActive = " + (string)(object)(isActive.Value ? 1 : 0);
            var parameters = new Dictionary<string, object>
                             {
        {
          "@apiKey",
          userApiKey
        }
      };
            List<Dictionary<string, string>> list = _database.Query(commandText, parameters);
            if (list != null && list.Count == 1)
                userApp = _parseUserAppObj(list[0]);
            return userApp;
        }

        public int InsertUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
                return 0;
            if (userApp.Id > 0)
                return UpdateUserApp(userApp);
            return _database.Execute("Insert Into UserApps  " + "(UserId,  IsActive,  ApiType,  ApiName,  ApiKey,  ApiSecret,  AppHosts,  AppIps) Values " + "(@userId, @isActive, @apiType, @apiName, @apiKey, @apiSecret, @appHosts, @appIps)", new Dictionary<string, object>
                                                                                                                                                                                                                                                  {
        {
          "@userId",
          userApp.UserId
        },
        {
          "@isActive",
          userApp.IsActive ? 1 : 0
        },
        {
          "@apiType",
          userApp.ApiType.GetHashCode()
        },
        {
          "@apiName",
          userApp.ApiName
        },
        {
          "@apiKey",
          userApp.ApiKey
        },
        {
          "@apiSecret",
          userApp.ApiSecret
        },
        {
          "@appHosts",
          userApp.AppHosts
        },
        {
          "@appIps",
          userApp.AppIps
        }
      });
        }

        public int UpdateUserApp(UserApp userApp)
        {
            if (userApp.UserId == 0)
                return 0;
            if (userApp.Id <= 0)
                return InsertUserApp(userApp);
            return _database.Execute("Update UserApps Set " + "UserId = @userId, IsActive = @isActive, ApiType = @apiType, ApiName = @apiName, ApiKey = @apiKey, ApiSecret = @apiSecret, AppHosts = @appHosts, AppIps = @appIps " + "where Id = @id", new Dictionary<string, object>
                                                                                                                                                                                                                                                      {
        {
          "@id",
          userApp.Id
        },
        {
          "@userId",
          userApp.UserId
        },
        {
          "@isActive",
          userApp.IsActive ? 1 : 0
        },
        {
          "@apiType",
          userApp.ApiType.GetHashCode()
        },
        {
          "@apiName",
          userApp.ApiName
        },
        {
          "@apiKey",
          userApp.ApiKey
        },
        {
          "@apiSecret",
          userApp.ApiSecret
        },
        {
          "@appHosts",
          userApp.AppHosts
        },
        {
          "@appIps",
          userApp.AppIps
        }
      });
        }

        private UserApp _parseUserAppObj(Dictionary<string, string> row)
        {
            var userApp = new UserApp
                              {
                                  Id = int.Parse(row["Id"]),
                                  UserId = int.Parse(row["UserId"]),
                                  IsActive = false
                              };
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