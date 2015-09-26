using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace Quang.Auth.DataAccess
{
    public static class UserClaimsDal
    {
        /// <summary>
        /// Returns a ClaimsIdentity instance given a userId
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<ClaimsIdentity> GetByUserId(long userId)
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            string commandText = "Select ClaimType, ClaimValue from UserClaims where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@UserId", userId } };
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var rows = await conn.QueryAsync<Claim>(commandText, parameters);
                foreach (var row in rows)
                {
                   // Claim claim = new Claim(row["ClaimType"], row["ClaimValue"]);
                    claims.AddClaim(row);
                }
            }
               

            return claims;
        }
        /// <summary>
        /// Deletes all claims from a user given a userId
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns></returns>
        public async static Task<long> Delete(int userId)
        {
            string commandText = "Delete from UserClaims where UserId = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);

            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        /// <summary>
        /// Inserts a new claim in UserClaims table
        /// </summary>
        /// <param name="userClaim">User's claim to be added</param>
        /// <param name="userId">User's id</param>
        /// <returns></returns>
        public async static Task<long> Insert(Claim userClaim, long userId)
        {
            string commandText = "Insert into UserClaims (ClaimValue, ClaimType, UserId) values (@value, @type, @userId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("value", userClaim.Value);
            parameters.Add("type", userClaim.Type);
            parameters.Add("userId", userId);


            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }

        /// <summary>
        /// Deletes a claim from a user 
        /// </summary>
        /// <param name="user">The user to have a claim deleted</param>
        /// <param name="claim">A claim to be deleted from user</param>
        /// <returns></returns>
        public async static Task<long> Delete(long userId, Claim claim)
        {
            string commandText = "Delete from UserClaims where UserId = @userId and ClaimValue = @value and ClaimType = @type";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);
            parameters.Add("value", claim.Value);
            parameters.Add("type", claim.Type);


            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                results = await conn.ExecuteAsync(commandText, parameters);

            }

            return results;
        }
    }
}
