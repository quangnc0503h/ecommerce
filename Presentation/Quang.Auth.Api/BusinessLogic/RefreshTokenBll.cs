using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class RefreshTokenBll : IRefreshTokenBll
    {
        private readonly IRefreshTokenTable _refreshTokenTable;

        public MySQLDatabase Database { get; private set; }

        public RefreshTokenBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _refreshTokenTable = new RefreshTokenTable(Database);
        }

        public Task<Client> GetOneClient(string clientId)
        {
            return Task.FromResult(_refreshTokenTable.GetOneClient(clientId));
        }

        public Task<IEnumerable<RefreshToken>> GetAllRefreshTokens()
        {
            return Task.FromResult(_refreshTokenTable.GetAllRefreshTokens());
        }

        public Task<RefreshToken> GetOneRefreshToken(string refreshTokenId)
        {
            return Task.FromResult(_refreshTokenTable.GetOneRefreshToken(refreshTokenId));
        }

        public Task<RefreshToken> FindRefreshToken(string clientId, string subject)
        {
            return Task.FromResult(_refreshTokenTable.FindRefreshToken(clientId, subject));
        }

        public Task<int> AddRefreshToken(RefreshToken token)
        {
            return Task.FromResult(_refreshTokenTable.AddRefreshToken(token));
        }

        public Task<int> RemoveRefreshToken(string refreshTokenId)
        {
            return Task.FromResult(_refreshTokenTable.RemoveRefreshToken(refreshTokenId));
        }

        public Task<int> InsertRefreshToken(RefreshToken token)
        {
            return Task.FromResult(_refreshTokenTable.InsertRefreshToken(token));
        }
    }
}