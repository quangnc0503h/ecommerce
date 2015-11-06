using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;

using Quang.Auth.Entities;
using Quang.Common.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        private string CurrentToken { get; set; }

        public void Create(AuthenticationTokenCreateContext context)
        {
            this.CreateAsync(context).Wait();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            string clientid = context.Ticket.Properties.Dictionary["as:client_id"];
            if (!string.IsNullOrEmpty(clientid))
            {
                string refreshTokenId = Guid.NewGuid().ToString("n");
                UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
                IRefreshTokenBll refreshTokenBll = UnityContainerExtensions.Resolve<IRefreshTokenBll>((IUnityContainer)unityContainer);
                string refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");
                RefreshToken token = new RefreshToken()
                {
                    Id = ApplicationOAuthProvider.GetSecretHash(refreshTokenId),
                    ClientId = clientid,
                    Subject = context.Ticket.Identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
                };
                context.Ticket.Properties.IssuedUtc = new DateTimeOffset?((DateTimeOffset)token.IssuedUtc);
                context.Ticket.Properties.ExpiresUtc = new DateTimeOffset?((DateTimeOffset)token.ExpiresUtc);
                token.ProtectedTicket = context.SerializeTicket();
                int result = await refreshTokenBll.InsertRefreshToken(token);
                if (result > 0)
                {
                    context.SetToken(refreshTokenId);
                    if (!string.IsNullOrEmpty(this.CurrentToken))
                        await this.LogHistory(LoginType.RefreshToken, LoginStatus.Success, clientid, token.Subject, this.CurrentToken, (string)null, (string)null);
                }
                else if (!string.IsNullOrEmpty(this.CurrentToken))
                    await this.LogHistory(LoginType.RefreshToken, LoginStatus.ErrorAddRefreshToken, clientid, token.Subject, this.CurrentToken, (string)null, (string)null);
            }
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            this.ReceiveAsync(context).Wait();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            this.CurrentToken = context.Token;
            string allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new string[1]
            {
        allowedOrigin
            });
            string hashedTokenId = ApplicationOAuthProvider.GetSecretHash(context.Token);
            UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            IRefreshTokenBll refreshTokenBll = UnityContainerExtensions.Resolve<IRefreshTokenBll>((IUnityContainer)unityContainer);
            RefreshToken refreshToken = await refreshTokenBll.GetOneRefreshToken(hashedTokenId);
            if (refreshToken != null)
            {
                context.DeserializeTicket(refreshToken.ProtectedTicket);
                int num = await refreshTokenBll.RemoveRefreshToken(hashedTokenId);
            }
            else
                await this.LogHistory(LoginType.RefreshToken, LoginStatus.InvalidRefreshToken, (string)null, (string)null, context.Token, (string)null, (string)null);
        }

        private async Task LogHistory(LoginType loginType, LoginStatus status, string clientId, string username, string refreshToken, string clientIp = null, string clientUri = null)
        {
            UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            ILoginHistoryBll loginHistoryBll = UnityContainerExtensions.Resolve<ILoginHistoryBll>((IUnityContainer)unityContainer);
            if (string.IsNullOrEmpty(clientIp))
                clientIp = SecurityUtils.GetClientIPAddress();
            if (string.IsNullOrEmpty(clientUri) && HttpContext.Current != null)
                clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            int num = await loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput()
            {
                Type = loginType.GetHashCode(),
                UserName = username,
                LoginTime = DateTime.Now,
                LoginStatus = status.GetHashCode(),
                RefreshToken = refreshToken,
                AppId = string.IsNullOrEmpty(clientId) ? (string)null : clientId,
                ClientUri = clientUri,
                ClientIP = clientIp,
                ClientUA = HttpContext.Current.Request.UserAgent,
                ClientApiKey = (string)null,
                ClientDevice = (string)null
            });
        }
    }
}