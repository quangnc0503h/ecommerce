using Quang.Auth.BusinessLogic;
using Quang.Auth.Api.Models;
using Quang.Common.Auth;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }
        
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
         //   var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            IFormCollection form = await context.Request.ReadFormAsync();

            var clientAuthorization = form.Get("cauthorization");
            var clientRequestUri = form.Get("curi");
            var clientRquestMethod = form.Get("cmethod");
            var clientHostName = form.Get("chostname");
            var clientHostAddress = form.Get("chostname");

            ApplicationUser user = null;
            string clientApiSecret = null;
            
            if (!string.IsNullOrEmpty(clientAuthorization) && !string.IsNullOrEmpty(clientRequestUri) && !string.IsNullOrEmpty(clientRquestMethod))
            {
                if (context.UserName == ClientApiProvider.ClientTokenUserName && context.Password == ClientApiProvider.ClientTokenPassword)
                {
                    var authHeaderValue = AuthenticationHeaderValue.Parse(clientAuthorization);
                    if (authHeaderValue != null && authHeaderValue.Scheme == ClientApiProvider.ClientHeaderAuthScheme && !string.IsNullOrEmpty(authHeaderValue.Parameter))
                    {
                        var userFromClient = await GetUserByClientAuthorization(context.OwinContext, authHeaderValue.Parameter, clientRequestUri, clientRquestMethod, clientHostName, clientHostAddress);
                        if (userFromClient != null)
                        {
                            user = userFromClient.Item1;
                            clientApiSecret = userFromClient.Item2;
                        }
                    }
                }
            }
            else
            {

                user = await UserBll.get(context.UserName, context.Password);
            }

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            if (!string.IsNullOrEmpty(clientApiSecret))
            {
                if (ClientApiProvider.ClientTokenCacheTimeInMinutes > 0)
                {
                    context.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(ClientApiProvider.ClientTokenCacheTimeInMinutes + 1);
                }
                else
                {
                    context.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(ClientApiProvider.ClientTokenCacheTimeInMinutesDefault);
                }
            }
            else
            {
                context.Options.AccessTokenExpireTimeSpan = Startup.AccessTokenExpireTimeSpan;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = await CreateProperties(user, clientApiSecret);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }
        
        private async Task<Tuple<ApplicationUser, string>> GetUserByClientAuthorization(IOwinContext context, string clientAuthorization, string clientRequestUri, string clientRequestMethod, string clientHostName, string clientHostAddress)
        {
            Tuple<ApplicationUser, string> result = null;
            var autherizationHeaderArray = ClientApiProvider.GetAutherizationHeaderValues(clientAuthorization);
            if (autherizationHeaderArray != null)
            {
                var apiKey = autherizationHeaderArray[0];
                var incomingBase64Signature = autherizationHeaderArray[1];
                var nonce = autherizationHeaderArray[2];
                var requestTimeStamp = autherizationHeaderArray[3];
               // var userBll = UnityConfig.GetConfiguredContainer().Resolve<IUserBll>();
                var userApp = await UserBll.GetUserApp(apiKey);
                if (userApp != null && !string.IsNullOrEmpty(userApp.ApiSecret))
                {
                    if (CheckClientHost(userApp.AppHosts, userApp.AppIps, clientHostName, clientHostAddress))
                    {
                        string clientRequestUriDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(clientRequestUri));
                        var isValid = await ClientApiProvider.isValidRequest(clientRequestUriDecoded, clientRequestMethod, userApp.ApiKey, userApp.ApiSecret, incomingBase64Signature, nonce, requestTimeStamp);

                        if (isValid)
                        {
                            var userManager = context.GetUserManager<ApplicationUserManager>();
                            var user = await userManager.FindByIdAsync(userApp.UserId);
                            if (user != null)
                            {
                                result = new Tuple<ApplicationUser, string>(user, userApp.ApiSecret);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private bool CheckClientHost(string allowHosts, string allowIps, string clientHostName, string clientHostAddress)
        {
            bool isAllow = true;
            if (isAllow && !string.IsNullOrEmpty(allowHosts))
            {
                if (!string.IsNullOrEmpty(clientHostName))
                {
                    var hosts = allowHosts.Split(';', ',').Select(m => m.Trim().ToLower());
                    isAllow = hosts.Contains(clientHostName.ToLower());
                }
                else
                {
                    isAllow = false;
                }
            }
            if (isAllow && !string.IsNullOrEmpty(allowIps))
            {
                if (!string.IsNullOrEmpty(clientHostAddress))
                {
                    var ips = allowIps.Split(';', ',').Select(m => m.Trim().ToLower());
                    isAllow = ips.Contains(clientHostAddress.ToLower());
                }
                else
                {
                    isAllow = false;
                }
            }
            return isAllow;
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
                
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public static async Task<AuthenticationProperties> CreateProperties(ApplicationUser user, string clientApiSecret)
        {
            var userManager = HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            
            var claims = await userManager.GetClaimsAsync(user.Id);
            var displayName = claims.FirstOrDefault(m => m.Type == "displayName");

            IDictionary<string, string> data = new Dictionary<string, string>();
            data.Add("userName", user.UserName);
            data.Add("displayName", displayName != null ? displayName.Value : user.UserName);
            if (!string.IsNullOrEmpty(clientApiSecret))
            {
                data.Add("client_api_secret", clientApiSecret);
            }
            
            var roles = userManager.GetRolesAsync(user.Id).Result;
            data.Add("roles", string.Join(",", roles));

            var property = new AuthenticationProperties(data);
            
            return property;
        }

        public override Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            var headers = context.Request.Headers;
            var authorization = headers.Get(ClientApiProvider.ClientHeaderAuthName);
            if (!string.IsNullOrEmpty(authorization))
            {
                var authentication = AuthenticationHeaderValue.Parse(authorization);
                if (authentication != null && authentication.Scheme == ClientApiProvider.ClientHeaderAuthScheme)
                {
                    string accessToken = ClientApiProvider.GetAccessToken(authentication, context.Request).Result;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var authorizationHeader = new AuthenticationHeaderValue(ClientApiProvider.OAuthHeaderScheme, accessToken).ToString();
                        context.Request.Headers.Set(ClientApiProvider.OAuthHeaderName, authorizationHeader);
                    }
                }
            }

            return base.MatchEndpoint(context);
        }
    }
}