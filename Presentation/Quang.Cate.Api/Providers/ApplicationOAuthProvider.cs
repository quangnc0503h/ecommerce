using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Quang.Common.Auth;
using System.Net.Http.Headers;

namespace Quang.Cate.Api.Providers
{
    public class ApplicationOAuthProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var headers = context.Request.Headers;
            var authorization = headers.Get(ClientApiProvider.ClientHeaderAuthName);
            if (!string.IsNullOrEmpty(authorization))
            {
                var authentication = AuthenticationHeaderValue.Parse(authorization);
                if (authentication != null && authentication.Scheme == ClientApiProvider.ClientHeaderAuthScheme)
                {
                    string accessToken = ClientApiProvider.GetAccessToken(authentication, context.OwinContext.Request).Result;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var authorizationHeader = new AuthenticationHeaderValue(ClientApiProvider.OAuthHeaderScheme, accessToken).ToString();
                        context.Request.Headers.Set(ClientApiProvider.OAuthHeaderName, authorizationHeader);
                        context.Token = accessToken;
                    }
                }
            }

            return base.RequestToken(context);
        }
    }
}