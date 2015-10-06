using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quang.Auth.DataAccess
{
    /// <summary>
    /// Class that represents the UserLogins table in the MySQL Database
    /// </summary>
    public static class UserLoginDal
    {
       

        /// <summary>
        /// Deletes a login from a user in the UserLogins table
        /// </summary>
        /// <param name="user">User to have login deleted</param>
        /// <param name="login">Login to be deleted from user</param>
        /// <returns></returns>
        public static async Task<long> Delete(long userId, string loginProvider, string providerKey)
        {
            string commandText = "Delete from UserLogins where UserId = @userId and LoginProvider = @loginProvider and ProviderKey = @providerKey";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);
            parameters.Add("loginProvider", loginProvider);
            parameters.Add("providerKey", providerKey);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        /// <summary>
        /// Deletes all Logins from a user in the UserLogins table
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public static async Task<long> Delete(string userId)
        {
            string commandText = "Delete from UserLogins where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);


            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        /// <summary>
        /// Inserts a new login in the UserLogins table
        /// </summary>
        /// <param name="user">User to have new login added</param>
        /// <param name="login">Login to be added</param>
        /// <returns></returns>
        public static async Task<long> Insert(long userId, string loginProvider, string providerKey)
        {
            string commandText = "Insert into UserLogins (LoginProvider, ProviderKey, UserId) values (@loginProvider, @providerKey, @userId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("loginProvider", loginProvider);
            parameters.Add("providerKey", providerKey);
            parameters.Add("userId", userId);


            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        /// <summary>
        /// Return a userId given a user's login
        /// </summary>
        /// <param name="userLogin">The user's login info</param>
        /// <returns></returns>
        public static async Task<long> FindUserIdByLogin(string loginProvider, string providerKey)
        {
            string commandText = "Select UserId from UserLogins where LoginProvider = @loginProvider and ProviderKey = @providerKey";
            //commandText = "select UserId from UserLogins where LoginProvider = 'Facebook' and ProviderKey = '10200549931506121'";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("loginProvider", loginProvider);
            parameters.Add("providerKey", providerKey);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        
    }
}
