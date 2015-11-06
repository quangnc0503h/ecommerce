using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class RefreshTokenBll : IRefreshTokenBll
    {
        private IRefreshTokenTable _refreshTokenTable;

        public MySQLDatabase Database { get; private set; }

        public RefreshTokenBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._refreshTokenTable = (IRefreshTokenTable)new RefreshTokenTable(this.Database);
        }

        public Task<Client> GetOneClient(string clientId)
        {
            return Task.FromResult<Client>(this._refreshTokenTable.GetOneClient(clientId));
        }

        public Task<IEnumerable<RefreshToken>> GetAllRefreshTokens()
        {
            return Task.FromResult<IEnumerable<RefreshToken>>(this._refreshTokenTable.GetAllRefreshTokens());
        }

        public Task<RefreshToken> GetOneRefreshToken(string refreshTokenId)
        {
            return Task.FromResult<RefreshToken>(this._refreshTokenTable.GetOneRefreshToken(refreshTokenId));
        }

        public Task<RefreshToken> FindRefreshToken(string clientId, string subject)
        {
            return Task.FromResult<RefreshToken>(this._refreshTokenTable.FindRefreshToken(clientId, subject));
        }

        public Task<int> AddRefreshToken(RefreshToken token)
        {
            return Task.FromResult<int>(this._refreshTokenTable.AddRefreshToken(token));
        }

        public Task<int> RemoveRefreshToken(string refreshTokenId)
        {
            return Task.FromResult<int>(this._refreshTokenTable.RemoveRefreshToken(refreshTokenId));
        }

        public Task<int> InsertRefreshToken(RefreshToken token)
        {
            return Task.FromResult<int>(this._refreshTokenTable.InsertRefreshToken(token));
        }
    }
}