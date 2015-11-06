using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.BusinessLogic
{
    public static class LoginHistoryBll
    {
        public static async Task<LoginHistory> GetOneLoginHistory(int loginHistoryId)
        {
            return await (LoginHistoryDal.GetOneLoginHistory(loginHistoryId));
        }

       
        public static async Task<long> GetTotal(int[] paramType, string userName, DateTime? loginTimeFrom, DateTime? loginTimeTo, int[] loginStatus, string refreshToken, string[] appId,
            string clientUri, string clientIP, string clientUA, string clientDevice, string clientApiKey)
        {
            return await LoginHistoryDal.GetTotal(paramType, userName, loginTimeFrom, loginTimeTo, loginStatus, refreshToken, appId,
             clientUri, clientIP, clientUA, clientDevice, clientApiKey);
        }
        public static async Task<IEnumerable<LoginHistory>> GetPaging(int[] paramType, string userName, DateTime? loginTimeFrom, DateTime? loginTimeTo, int[] loginStatus, string refreshToken, string[] appId,
            string clientUri, string clientIP, string clientUA, string clientDevice, string clientApiKey, int pageSize, int pageNumber)
        {
            return await LoginHistoryDal.GetPaging(paramType, userName, loginTimeFrom, loginTimeTo, loginStatus, refreshToken, appId,
             clientUri, clientIP, clientUA, clientDevice, clientApiKey, pageSize, pageNumber);
        }
        public static async Task<long> DeleteLoginHistory(int loginHistoryId)
        {
            return await (LoginHistoryDal.Delete(loginHistoryId));
        }

        public static async Task<long> DeleteLoginHistory(IEnumerable<int> Ids)
        {
            return await (LoginHistoryDal.Delete(Ids));
        }

        public static async Task<long> InsertLoginHistory(LoginHistory input)
        {
            
            return await (LoginHistoryDal.InsertHistory(input));
        }

        public static async Task<long> CountSuccessLoggedIn(string username)
        {
            return await (LoginHistoryDal.CountSuccessLoggedIn(username));
        }
    }
}
