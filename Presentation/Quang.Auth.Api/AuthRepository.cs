using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api
{
    public class AuthRepository : IDisposable
    {
        private AuthContext _ctx;
        private UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            this._ctx = new AuthContext();
            this._userManager = new UserManager<IdentityUser>((IUserStore<IdentityUser>)new UserStore<IdentityUser>((DbContext)this._ctx));
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            IdentityUser identityUser = new IdentityUser();
            identityUser.UserName = userModel.UserName;
            IdentityUser user = identityUser;
            IdentityResult result = await this._userManager.CreateAsync(user, userModel.Password);
            return result;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await this._userManager.FindAsync(userName, password);
            return user;
        }

        public Client FindClient(string clientId)
        {
            return this._ctx.Clients.Find((object)clientId);
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            RefreshToken existingToken = Queryable.SingleOrDefault<RefreshToken>(Queryable.Where<RefreshToken>((IQueryable<RefreshToken>)this._ctx.RefreshTokens, (Expression<Func<RefreshToken, bool>>)(r => r.Subject == token.Subject && r.ClientId == token.ClientId)));
            if (existingToken != null)
            {
                int num = await this.RemoveRefreshToken(existingToken) ? 1 : 0;
            }
            this._ctx.RefreshTokens.Add(token);
            return await this._ctx.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            RefreshToken refreshToken = await this._ctx.RefreshTokens.FindAsync((object)refreshTokenId);
            bool flag;
            if (refreshToken != null)
            {
                this._ctx.RefreshTokens.Remove(refreshToken);
                flag = await this._ctx.SaveChangesAsync() > 0;
            }
            else
                flag = false;
            return flag;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            this._ctx.RefreshTokens.Remove(refreshToken);
            return await this._ctx.SaveChangesAsync() > 0;
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            RefreshToken refreshToken = await this._ctx.RefreshTokens.FindAsync((object)refreshTokenId);
            return refreshToken;
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return Enumerable.ToList<RefreshToken>((IEnumerable<RefreshToken>)this._ctx.RefreshTokens);
        }

        public async Task<IdentityUser> FindAsync(UserLoginInfo loginInfo)
        {
            IdentityUser user = await this._userManager.FindAsync(loginInfo);
            return user;
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            IdentityResult result = await this._userManager.CreateAsync(user);
            return result;
        }

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            IdentityResult result = await this._userManager.AddLoginAsync(userId, login);
            return result;
        }

        public void Dispose()
        {
            this._ctx.Dispose();
            this._userManager.Dispose();
        }
    }
}