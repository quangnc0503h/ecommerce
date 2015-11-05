using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quang.Auth.Entities;
using System.Text.RegularExpressions;

namespace Quang.Auth.DataAccess
{
    public static class LoginHistoryDal
    {
        public static async Task< LoginHistory> GetOneLoginHistory(int loginHistoryId)
        {
           // LoginHistory loginHistory = (LoginHistory)null;
            const string commandText = @"Select * from LoginHistory where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", loginHistoryId);
            List<LoginHistory> loginHistories;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
               
                var data = await conn.QueryAsync<LoginHistory>(commandText, parameters);
                loginHistories = data.ToList();

            }

            return loginHistories.FirstOrDefault();
           
        }

        public static async Task< long> Delete(int id)
        {
            const string commandText = "Delete from LoginHistory where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
          
        }

        public static async Task<long> Delete(IEnumerable<int> Ids)
        {
            string commandText = "Delete from LoginHistory where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            var parameters = new DynamicParameters();
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;         
        }

        private static string GetSqlFilterConditions(int[] paramType, string userName, DateTime? loginTimeFrom, DateTime? loginTimeTo, int[] loginStatus, string refreshToken, string[] appId, 
            string clientUri, string clientIP, string clientUA, string clientDevice, string clientApiKey, out DynamicParameters parameters)
        {
            List<string> list1 = new List<string>();
            parameters = new DynamicParameters();
            if (paramType != null && paramType.Length > 0)
                list1.Add("Type in (" + string.Join<int>(",", (IEnumerable<int>)paramType) + ")");
            if (!string.IsNullOrEmpty(userName))
            {
                list1.Add("UserName LIKE @UserName");
                parameters.Add("@UserName", (object)("%" + Utils.EncodeForLike(userName) + "%"));
            }
            if (loginTimeFrom.HasValue)
            {
                list1.Add("LoginTime >= @LoginTimeFrom");
                parameters.Add("@LoginTimeFrom", (object)loginTimeFrom.Value.ToLocalTime());
            }
            if (loginTimeTo.HasValue)
            {
                list1.Add("LoginTime <= @LoginTimeTo");
                parameters.Add("@LoginTimeTo", (object)loginTimeTo.Value.ToLocalTime());
            }
            if (loginStatus != null && loginStatus.Length > 0)
                list1.Add("LoginStatus in (" + string.Join<int>(",", (IEnumerable<int>)loginStatus) + ")");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                list1.Add("RefreshToken LIKE @RefreshToken");
                parameters.Add("@RefreshToken", (object)("%" + Utils.EncodeForLike(refreshToken) + "%"));
            }
            if (appId != null && appId.Length > 0)
            {
                List<string> list2 = new List<string>();
                if (Enumerable.Count<string>(Enumerable.Where<string>((IEnumerable<string>)appId, (Func<string, bool>)(m => string.IsNullOrEmpty(m)))) > 0)
                    list2.Add("AppId IS NULL");
                IEnumerable<string> source = Enumerable.Select<string, string>(Enumerable.Where<string>((IEnumerable<string>)appId, (Func<string, bool>)(m => !string.IsNullOrEmpty(m))), (Func<string, string>)(m => Regex.Replace(m, "[^(0-9a-zA-Z_\\-\\:)]", string.Empty)));
                if (Enumerable.Count<string>(source) > 0)
                    list2.Add("AppId in (" + string.Join(",", Enumerable.Select<string, string>(source, (Func<string, string>)(m => string.Format("'{0}'", (object)m)))) + ")");
                list1.Add("(" + string.Join(" OR ", (IEnumerable<string>)list2) + ")");
            }
            if (!string.IsNullOrEmpty(clientUri))
            {
                list1.Add("ClientUri LIKE @ClientUri");
                parameters.Add("@ClientUri", (object)("%" + Utils.EncodeForLike(clientUri) + "%"));
            }
            if (!string.IsNullOrEmpty(clientIP))
            {
                list1.Add("ClientIP LIKE @ClientIP");
                parameters.Add("@ClientIP", (object)("%" + Utils.EncodeForLike(clientIP) + "%"));
            }
            if (!string.IsNullOrEmpty(clientUA))
            {
                list1.Add("ClientUA LIKE @ClientUA");
                parameters.Add("@ClientUA", (object)("%" + Utils.EncodeForLike(clientUA) + "%"));
            }
            if (!string.IsNullOrEmpty(clientDevice))
            {
                list1.Add("ClientDevice LIKE @ClientDevice");
                parameters.Add("@ClientDevice", (object)("%" + Utils.EncodeForLike(clientDevice) + "%"));
            }
            if (!string.IsNullOrEmpty(clientApiKey))
            {
                list1.Add("ClientApiKey LIKE @ClientApiKey");
                parameters.Add("@ClientApiKey", (object)("%" + Utils.EncodeForLike(clientApiKey) + "%"));
            }
            return string.Join(" AND ", list1.ToArray());
        }

        public static async Task< long> GetTotal(int[] paramType, string userName, DateTime? loginTimeFrom, DateTime? loginTimeTo, int[] loginStatus, string refreshToken, string[] appId,
            string clientUri, string clientIP, string clientUA, string clientDevice, string clientApiKey)
        {
            DynamicParameters parameters;
            string filterConditions = GetSqlFilterConditions(paramType,  userName,  loginTimeFrom,  loginTimeTo, loginStatus,  refreshToken,  appId,
             clientUri, clientIP,  clientUA, clientDevice,  clientApiKey, out parameters);
            string commandText = "select count(*) from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                commandText = commandText + " WHERE " + filterConditions;
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = id.Single();
            }

            return results;
            
        }

        public static async Task< IEnumerable<LoginHistory>> GetPaging(int[] paramType, string userName, DateTime? loginTimeFrom, DateTime? loginTimeTo, int[] loginStatus, string refreshToken, string[] appId,
            string clientUri, string clientIP, string clientUA, string clientDevice, string clientApiKey, int pageSize, int pageNumber)
        {
            DynamicParameters parameters;
            string filterConditions = GetSqlFilterConditions(paramType, userName, loginTimeFrom, loginTimeTo, loginStatus, refreshToken, appId,
             clientUri, clientIP, clientUA, clientDevice, clientApiKey, out parameters);
            string str = "select * from LoginHistory";
            if (!string.IsNullOrEmpty(filterConditions))
                str = str + " WHERE " + filterConditions;
            string commandText = str + " order by Created DESC limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<LoginHistory> list = new List<LoginHistory>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<LoginHistory>(commandText, parameters);
                list = data.ToList();
            }
          
            return (IEnumerable<LoginHistory>)list;
        }

        public static async Task< long> InsertHistory(LoginHistory input)
        {
            string commandText = @" INSERT INTO LoginHistory(Id, Type, UserName, LoginTime, LoginStatus, RefreshToken, AppId, ClientUri, ClientIP, ClientUA,  ClientDevice, ClientApiKey, Created)
                                    VALUES( @Id, @Type, @UserName, @LoginTime, @LoginStatus, @RefreshToken, @AppId, @ClientUri, @ClientIP, @ClientUA, @ClientDevice, @ClientApiKey, @Created)";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", (object)null);
            parameters.Add("@Type", (object)input.Type);
            parameters.Add("@UserName", (object)input.UserName);
            parameters.Add("@LoginTime", (object)input.LoginTime);
            parameters.Add("@LoginStatus", (object)input.LoginStatus);
            parameters.Add("@RefreshToken", (object)input.RefreshToken);
            parameters.Add("@AppId", (object)input.AppId);
            parameters.Add("@ClientUri", (object)input.ClientUri);
            parameters.Add("@ClientIP", (object)input.ClientIP);
            parameters.Add("@ClientUA", (object)input.ClientUA);
            parameters.Add("@ClientDevice", (object)input.ClientDevice);
            parameters.Add("@ClientApiKey", (object)input.ClientApiKey);
            parameters.Add("@Created", (object)DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
           
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = id.Single();
            }

            return results;
        }

        public static async Task< long> CountSuccessLoggedIn(string username)
        {
            string commandText = "select count(*) from LoginHistory where UserName=@UserName and LoginStatus=@LoginStatus";
            var parameters = new DynamicParameters();
            parameters.Add("UserName", username);
           parameters.Add("LoginStatus", LoginStatus.Success.GetHashCode());                       

            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

    }
}
