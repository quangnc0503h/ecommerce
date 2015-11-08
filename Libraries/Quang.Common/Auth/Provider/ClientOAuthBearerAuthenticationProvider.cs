using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Common.Auth.Provider
{
    public class ClientOAuthBearerAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            try
            {
                IHeaderDictionary headers = context.Request.Headers;
                string input = headers.Get("Authorization");
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.IndexOf(' ') == -1)
                        input = string.Format("{0} {1}", "Cliamx", input);
                    AuthenticationHeaderValue authenticationHeaderValue = AuthenticationHeaderValue.Parse(input);
                    if (authenticationHeaderValue != null && authenticationHeaderValue.Scheme == "Cliamx")
                    {
                        string result = ClientApiProvider.GetAccessToken(authenticationHeaderValue, context.OwinContext.Request).Result;
                        if (!string.IsNullOrEmpty(result))
                        {
                            string str = new AuthenticationHeaderValue("Bearer", result).ToString();
                            context.Request.Headers.Set("Authorization", str);
                            context.Token = result;
                        }
                    }
                    else if (authenticationHeaderValue != null)
                    {
                        if (authenticationHeaderValue.Scheme == "Bearer")
                        {
                            string username = headers.Get("userName");
                            string realAccessToken;
                            if (AppAuthorizeAttribute.ValidateOAuthAuthorizationHeader(username, authenticationHeaderValue.Parameter, out realAccessToken))
                            {
                                string str = new AuthenticationHeaderValue(authenticationHeaderValue.Scheme, realAccessToken).ToString();
                                context.Request.Headers.Set("Authorization", str);
                                context.Token = realAccessToken;
                            }
                            else
                            {
                                context.Request.Headers.Remove("Authorization");
                                context.Token = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("RequestToken" + ex.ToString());
            }
            return base.RequestToken(context);
        }

        private void applyAuthorizationClientApi(OAuthRequestTokenContext context)
        {
            string input = context.Request.Headers.Get("Authorization");
            if (string.IsNullOrEmpty(input))
                return;
            AuthenticationHeaderValue authenticationHeaderValue = AuthenticationHeaderValue.Parse(input);
            if (authenticationHeaderValue == null || authenticationHeaderValue.Scheme != "Cliamx")
                return;
            string result = ClientApiProvider.GetAccessToken(authenticationHeaderValue, context.OwinContext.Request).Result;
            if (string.IsNullOrEmpty(result))
                return;
            string str = new AuthenticationHeaderValue("Bearer", result).ToString();
            context.Request.Headers.Set("Authorization", str);
            context.Token = result;
        }

        private void applyAuthorizationClientOAuth(OAuthRequestTokenContext context)
        {
            IHeaderDictionary headers = context.Request.Headers;
            string input = headers.Get("Authorization");
            if (string.IsNullOrEmpty(input))
                return;
            AuthenticationHeaderValue authenticationHeaderValue = AuthenticationHeaderValue.Parse(input);
            if (authenticationHeaderValue == null || authenticationHeaderValue.Scheme != "Bearer")
                return;
            string username = headers.Get("userName");
            string realAccessToken = (string)null;
            if (AppAuthorizeAttribute.ValidateOAuthAuthorizationHeader(username, authenticationHeaderValue.Parameter, out realAccessToken))
            {
                string str = new AuthenticationHeaderValue(authenticationHeaderValue.Scheme, realAccessToken).ToString();
                context.Request.Headers.Set("Authorization", str);
                context.Token = realAccessToken;
            }
            else
            {
                context.Request.Headers.Remove("Authorization");
                context.Token = (string)null;
            }
        }
    }
}
