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
        private MySQLDatabase _database;

        public LoginHistoryTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public LoginHistory GetOneLoginHistory(int loginHistoryId)
        {
            LoginHistory loginHistory = (LoginHistory)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from LoginHistory where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) loginHistoryId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                loginHistory = new LoginHistory();
                loginHistory.Id = (long)int.Parse(dictionary["Id"]);
                loginHistory.Type = int.Parse(dictionary["Type"]);
                loginHistory.UserName = dictionary["UserName"];
                loginHistory.LoginTime = DateTime.Parse(dictionary["LoginTime"]);
                loginHistory.LoginStatus = int.Parse(dictionary["LoginStatus"]);
                loginHistory.RefreshToken = dictionary["RefreshToken"];
                loginHistory.AppId = dictionary["AppId"];
                loginHistory.ClientUri = dictionary["ClientUri"];
                loginHistory.ClientIP = dictionary["ClientIP"];
                loginHistory.ClientUA = dictionary["ClientUA"];
                loginHistory.ClientDevice = dictionary["ClientDevice"];
                loginHistory.ClientApiKey = dictionary["ClientApiKey"];
                loginHistory.Created = DateTime.Parse(dictionary["Created"]);
            }
            return loginHistory;
        }

        public int Delete(int id)
        {
            return this._database.Execute("Delete from LoginHistory where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) id
        }
      });
        }

        public int Delete(IEnumerable<int> ids)
        {
            return this._database.Execute("Delete from LoginHistory where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(ids)) + ")", new Dictionary<string, object>());
        }

        private string GetSqlFilterConditions(FilterLoginHistoryInput input, out Dictionary<string, object> parameters)
        {
            List<string> list1 = new List<string>();
            parameters = new Dictionary<string, object>();
            if (input.Type != null && input.Type.Length > 0)
                list1.Add("Type in (" + string.Join<int>(",", (IEnumerable<int>)input.Type) + ")");
            if (!string.IsNullOrEmpty(input.UserName))
            {
                list1.Add("UserName LIKE @UserName");
                parameters.Add("@UserName", (object)("%" + Utils.EncodeForLike(input.UserName) + "%"));
            }
            if (input.LoginTimeFrom.HasValue)
            {
                list1.Add("LoginTime >= @LoginTimeFrom");
                parameters.Add("@LoginTimeFrom", (object)input.LoginTimeFrom.Value.ToLocalTime());
            }
            if (input.LoginTimeTo.HasValue)
            {
                list1.Add("LoginTime <= @LoginTimeTo");
                parameters.Add("@LoginTimeTo", (object)input.LoginTimeTo.Value.ToLocalTime());
            }
            if (input.LoginStatus != null && input.LoginStatus.Length > 0)
                list1.Add("LoginStatus in (" + string.Join<int>(",", (IEnumerable<int>)input.LoginStatus) + ")");
            if (!string.IsNullOrEmpty(input.RefreshToken))
            {
                list1.Add("RefreshToken LIKE @RefreshToken");
                parameters.Add("@RefreshToken", (object)("%" + Utils.EncodeForLike(input.RefreshToken) + "%"));
            }
            if (input.AppId != null && input.AppId.Length > 0)
            {
                List<string> list2 = new List<string>();
                if (Enumerable.Count<string>(Enumerable.Where<string>((IEnumerable<string>)input.AppId, (Func<string, bool>)(m => string.IsNullOrEmpty(m)))) > 0)
                    list2.Add("AppId IS NULL");
                IEnumerable<string> source = Enumerable.Select<string, string>(Enumerable.Where<string>((IEnumerable<string>)input.AppId, (Func<string, bool>)(m => !string.IsNullOrEmpty(m))), (Func<string, string>)(m => Regex.Replace(m, "[^(0-9a-zA-Z_\\-\\:)]", string.Empty)));
                if (Enumerable.Count<string>(source) > 0)
                    list2.Add("AppId in (" + string.Join(",", Enumerable.Select<string, string>(source, (Func<string, string>)(m => string.Format("'{0}'", (object)m)))) + ")");
                list1.Add("(" + string.Join(" OR ", (IEnumerable<string>)list2) + ")");
            }
            if (!string.IsNullOrEmpty(input.ClientUri))
            {
                list1.Add("ClientUri LIKE @ClientUri");
                parameters.Add("@ClientUri", (object)("%" + Utils.EncodeForLike(input.ClientUri) + "%"));
            }
            if (!string.IsNullOrEmpty(input.ClientIP))
            {
                list1.Add("ClientIP LIKE @ClientIP");
                parameters.Add("@ClientIP", (object)("%" + Utils.EncodeForLike(input.ClientIP) + "%"));
            }
            if (!string.IsNullOrEmpty(input.ClientUA))
            {
                list1.Add("ClientUA LIKE @ClientUA");
                parameters.Add("@ClientUA", (object)("%" + Utils.EncodeForLike(input.ClientUA) + "%"));
            }
            if (!string.IsNullOrEmpty(input.ClientDevice))
            {
                list1.Add("ClientDevice LIKE @ClientDevice");
                parameters.Add("@ClientDevice", (object)("%" + Utils.EncodeForLike(input.ClientDevice) + "%"));
            }
            if (!string.IsNullOrEmpty(input.ClientApiKey))
            {
                list1.Add("ClientApiKey LIKE @ClientApiKey");
                parameters.Add("@ClientApiKey", (object)("%" + Utils.EncodeForLike(input.ClientApiKey) + "%"));
            }
            return string.Join(" AND ", list1.ToArray());
        }

        public int GetTotal(FilterLoginHistoryInput input)
        {
            Dictionary<string, object> parameters;
            string filterConditions = this.GetSqlFilterConditions(input, out parameters);
            string commandText = "select count(*) from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                commandText = commandText + " WHERE " + filterConditions;
            return int.Parse(this._database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<LoginHistory> GetPaging(FilterLoginHistoryInput input)
        {
            Dictionary<string, object> parameters;
            string filterConditions = this.GetSqlFilterConditions(input, out parameters);
            string str = "select * from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                str = str + " WHERE " + filterConditions;
            string commandText = str + " order by Created DESC limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(input.PageSize * input.PageNumber));
            parameters.Add("@pageSize", (object)input.PageSize);
            List<LoginHistory> list = new List<LoginHistory>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
                list.Add(new LoginHistory()
                {
                    Id = (long)int.Parse(dictionary["Id"]),
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
                });
            return (IEnumerable<LoginHistory>)list;
        }

        public int InsertHistory(LoginHistory input)
        {
            try
            {
                return this._database.Execute("INSERT INTO LoginHistory\n                (   Id,\n                    Type,\n                    UserName,\n                    LoginTime,\n                    LoginStatus,\n                    RefreshToken,\n                    AppId,\n                    ClientUri,\n                    ClientIP,\n                    ClientUA,\n                    ClientDevice,\n                    ClientApiKey,\n                    Created)\n                VALUES(\n                    @Id,\n                    @Type,\n                    @UserName,\n                    @LoginTime,\n                    @LoginStatus,\n                    @RefreshToken,\n                    @AppId,\n                    @ClientUri,\n                    @ClientIP,\n                    @ClientUA,\n                    @ClientDevice,\n                    @ClientApiKey,\n                    @Created)", new Dictionary<string, object>()
        {
          {
            "@Id",            (object) null
          },
          {
            "@Type",            (object) input.Type
          },
          {
            "@UserName",            (object) input.UserName
          },
          {
            "@LoginTime",
            (object) input.LoginTime
          },
          {
            "@LoginStatus",
            (object) input.LoginStatus
          },
          {
            "@RefreshToken",
            (object) input.RefreshToken
          },
          {
            "@AppId",
            (object) input.AppId
          },
          {
            "@ClientUri",
            (object) input.ClientUri
          },
          {
            "@ClientIP",
            (object) input.ClientIP
          },
          {
            "@ClientUA",
            (object) input.ClientUA
          },
          {
            "@ClientDevice",
            (object) input.ClientDevice
          },
          {
            "@ClientApiKey",
            (object) input.ClientApiKey
          },
          {
            "@Created",
            (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
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
            return long.Parse(this._database.QueryValue("select count(*) from LoginHistory where UserName=@UserName and LoginStatus=@LoginStatus", new Dictionary<string, object>()
      {
        {
          "@UserName",
          (object) username
        },
        {
          "@LoginStatus",
          (object) LoginStatus.Success.GetHashCode()
        }
      }).ToString());
        }
    }
}