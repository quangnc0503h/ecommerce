using AspNet.Identity.MySQL;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class LoginHistoryTable : ILoginHistoryTable
    {
        private readonly MySQLDatabase _database;

        public LoginHistoryTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public LoginHistory GetOneLoginHistory(int loginHistoryId)
        {
            var loginHistory = (LoginHistory)null;
            List<Dictionary<string, string>> list = _database.Query("Select * from LoginHistory where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          loginHistoryId
        }
      });
            if (list != null && list.Count == 1)
            {
                var dictionary = list[0];
                loginHistory = new LoginHistory
                               {
                                   Id = int.Parse(dictionary["Id"]),
                                   Type = int.Parse(dictionary["Type"]),
                                   UserName = dictionary["UserName"],
                                   LoginTime = DateTime.Parse(dictionary["LoginTime"]),
                                   LoginStatus = int.Parse(dictionary["LoginStatus"]),
                                   RefreshToken = dictionary["RefreshToken"],
                                   AppId = dictionary["AppId"],
                                   ClientUri = dictionary["ClientUri"],
                                   ClientIP = dictionary["ClientIP"],
                                   ClientUA = dictionary["ClientUA"],
                                   ClientDevice = dictionary["ClientDevice"],
                                   ClientApiKey = dictionary["ClientApiKey"],
                                   Created = DateTime.Parse(dictionary["Created"])
                               };
            }
            return loginHistory;
        }

        public int Delete(int id)
        {
            return _database.Execute("Delete from LoginHistory where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          id
        }
      });
        }

        public int Delete(IEnumerable<int> ids)
        {
            return _database.Execute("Delete from LoginHistory where Id in (" + string.Join(",", ids.ToArray()) + ")", new Dictionary<string, object>());
        }

        private string GetSqlFilterConditions(FilterLoginHistoryInput input, out Dictionary<string, object> parameters)
        {
            var list1 = new List<string>();
            parameters = new Dictionary<string, object>();
            if (input.Type != null && input.Type.Length > 0)
                list1.Add("Type in (" + string.Join(",", input.Type) + ")");
            if (!string.IsNullOrEmpty(input.UserName))
            {
                list1.Add("UserName LIKE @UserName");
                parameters.Add("@UserName", "%" + Utils.EncodeForLike(input.UserName) + "%");
            }
            if (input.LoginTimeFrom.HasValue)
            {
                list1.Add("LoginTime >= @LoginTimeFrom");
                parameters.Add("@LoginTimeFrom", input.LoginTimeFrom.Value.ToLocalTime());
            }
            if (input.LoginTimeTo.HasValue)
            {
                list1.Add("LoginTime <= @LoginTimeTo");
                parameters.Add("@LoginTimeTo", input.LoginTimeTo.Value.ToLocalTime());
            }
            if (input.LoginStatus != null && input.LoginStatus.Length > 0)
                list1.Add("LoginStatus in (" + string.Join(",", input.LoginStatus) + ")");
            if (!string.IsNullOrEmpty(input.RefreshToken))
            {
                list1.Add("RefreshToken LIKE @RefreshToken");
                parameters.Add("@RefreshToken", "%" + Utils.EncodeForLike(input.RefreshToken) + "%");
            }
            if (input.AppId != null && input.AppId.Length > 0)
            {
                var list2 = new List<string>();
                if (input.AppId.Where(string.IsNullOrEmpty).Any())
                    list2.Add("AppId IS NULL");
                IEnumerable<string> source = input.AppId.Where(m => !string.IsNullOrEmpty(m)).Select(m => Regex.Replace(m, "[^(0-9a-zA-Z_\\-\\:)]", string.Empty));
                if (source.Any())
                    list2.Add("AppId in (" + string.Join(",", source.Select(m => string.Format("'{0}'", m))) + ")");
                list1.Add("(" + string.Join(" OR ", list2) + ")");
            }
            if (!string.IsNullOrEmpty(input.ClientUri))
            {
                list1.Add("ClientUri LIKE @ClientUri");
                parameters.Add("@ClientUri", "%" + Utils.EncodeForLike(input.ClientUri) + "%");
            }
            if (!string.IsNullOrEmpty(input.ClientIP))
            {
                list1.Add("ClientIP LIKE @ClientIP");
                parameters.Add("@ClientIP", "%" + Utils.EncodeForLike(input.ClientIP) + "%");
            }
            if (!string.IsNullOrEmpty(input.ClientUA))
            {
                list1.Add("ClientUA LIKE @ClientUA");
                parameters.Add("@ClientUA", "%" + Utils.EncodeForLike(input.ClientUA) + "%");
            }
            if (!string.IsNullOrEmpty(input.ClientDevice))
            {
                list1.Add("ClientDevice LIKE @ClientDevice");
                parameters.Add("@ClientDevice", "%" + Utils.EncodeForLike(input.ClientDevice) + "%");
            }
            if (!string.IsNullOrEmpty(input.ClientApiKey))
            {
                list1.Add("ClientApiKey LIKE @ClientApiKey");
                parameters.Add("@ClientApiKey", "%" + Utils.EncodeForLike(input.ClientApiKey) + "%");
            }
            return string.Join(" AND ", list1.ToArray());
        }

        public int GetTotal(FilterLoginHistoryInput input)
        {
            Dictionary<string, object> parameters;
            string filterConditions = GetSqlFilterConditions(input, out parameters);
            string commandText = "select count(*) from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                commandText = commandText + " WHERE " + filterConditions;
            return int.Parse(_database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<LoginHistory> GetPaging(FilterLoginHistoryInput input)
        {
            Dictionary<string, object> parameters;
            string filterConditions = GetSqlFilterConditions(input, out parameters);
            string str = "select * from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                str = str + " WHERE " + filterConditions;
            string commandText = str + " order by Created DESC limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", input.PageSize * input.PageNumber);
            parameters.Add("@pageSize", input.PageSize);
            return _database.Query(commandText, parameters).Select(dictionary => new LoginHistory
                                                                                 {
                                                                                     Id = int.Parse(dictionary["Id"]), Type = int.Parse(dictionary["Type"]), UserName = dictionary["UserName"], LoginTime = DateTime.Parse(dictionary["LoginTime"]), LoginStatus = int.Parse(dictionary["LoginStatus"]), RefreshToken = dictionary["RefreshToken"], AppId = dictionary["AppId"], ClientUri = dictionary["ClientUri"], ClientIP = dictionary["ClientIP"], ClientUA = dictionary["ClientUA"], ClientDevice = dictionary["ClientDevice"], ClientApiKey = dictionary["ClientApiKey"], Created = DateTime.Parse(dictionary["Created"])
                                                                                 }).ToList();
        }

        public int InsertHistory(LoginHistory input)
        {
            try
            {
                return this._database.Execute("INSERT INTO LoginHistory\n                (   Id,\n                    Type,\n                    UserName,\n                    LoginTime,\n                    LoginStatus,\n                    RefreshToken,\n                    AppId,\n                    ClientUri,\n                    ClientIP,\n                    ClientUA,\n                    ClientDevice,\n                    ClientApiKey,\n                    Created)\n                VALUES(\n                    @Id,\n                    @Type,\n                    @UserName,\n                    @LoginTime,\n                    @LoginStatus,\n                    @RefreshToken,\n                    @AppId,\n                    @ClientUri,\n                    @ClientIP,\n                    @ClientUA,\n                    @ClientDevice,\n                    @ClientApiKey,\n                    @Created)", new Dictionary<string, object>()
        {
          {
            "@Id",            null
          },
          {
            "@Type",            input.Type
          },
          {
            "@UserName",            input.UserName
          },
          {
            "@LoginTime",
            input.LoginTime
          },
          {
            "@LoginStatus",
            input.LoginStatus
          },
          {
            "@RefreshToken",
            input.RefreshToken
          },
          {
            "@AppId",
            input.AppId
          },
          {
            "@ClientUri",
            input.ClientUri
          },
          {
            "@ClientIP",
            input.ClientIP
          },
          {
            "@ClientUA",
            input.ClientUA
          },
          {
            "@ClientDevice",
            input.ClientDevice
          },
          {
            "@ClientApiKey",
            input.ClientApiKey
          },
          {
            "@Created",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });
            }
            catch (Exception ex)
            {
            }
            return -1;
        }

        public long CountSuccessLoggedIn(string username)
        {
            return long.Parse(_database.QueryValue("select count(*) from LoginHistory where UserName=@UserName and LoginStatus=@LoginStatus", new Dictionary<string, object>()
      {
        {
          "@UserName",
          username
        },
        {
          "@LoginStatus",
          LoginStatus.Success.GetHashCode()
        }
      }).ToString());
        }
    }
}