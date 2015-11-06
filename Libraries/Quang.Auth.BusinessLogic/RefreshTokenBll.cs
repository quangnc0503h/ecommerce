using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.BusinessLogic
{
    public static class RefreshTokenBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<Client> GetOneClient(string clientId)
        {
            return await (RefreshTokenDal.GetOneClient(clientId));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<RefreshToken>> GetAllRefreshTokens()
        {
            return await (RefreshTokenDal.GetAllRefreshTokens());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshTokenId"></param>
        /// <returns></returns>
        public static async Task<RefreshToken> GetOneRefreshToken(string refreshTokenId)
        {
            return await (RefreshTokenDal.GetOneRefreshToken(refreshTokenId));
        }

        public static async Task<RefreshToken> FindRefreshToken(string clientId, string subject)
        {
            return  await (RefreshTokenDal.FindRefreshToken(clientId, subject));
        }

        public static async Task<long> AddRefreshToken(RefreshToken token)
        {
            return await (RefreshTokenDal.AddRefreshToken(token));
        }

        public static async Task<long> RemoveRefreshToken(string refreshTokenId)
        {
            return await (RefreshTokenDal.RemoveRefreshToken(refreshTokenId));
        }

        public static async Task<long> InsertRefreshToken(RefreshToken token)
        {
            return await (RefreshTokenDal.InsertRefreshToken(token));
        }
    }
}
