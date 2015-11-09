
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        //private static readonly ICacheClient _redisCache = (ICacheClient)new RedisCacheClient(RedisConnection.SecurityConn, (ISerializer)null, 0);
        private static readonly Dictionary<string, string> _mobileProfileClaims = new Dictionary<string, string>(){
            { "profile:cmnd","cmnd"},
            { "profile:cty","congTy"},
            { "profile:diachi","diaChi"},
            { "profile:mst","maSoThue"},
            { "profile:avartar","avatar"}
        };
        private const string CientMobileMuaVe = "MMuaVe";
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
                throw new ArgumentNullException("publicClientId");
            _publicClientId = publicClientId;
        }

        private async Task LogHistory(LoginType loginType, LoginStatus status, string clientId, string username, string deviceKey, string apiKey, string clientIp = null, string clientUri = null)
        {
            var unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            var loginHistoryBll = unityContainer.Resolve<ILoginHistoryBll>();
            if (string.IsNullOrEmpty(clientIp))
                clientIp =SecurityUtils.GetClientIPAddress();
            if (string.IsNullOrEmpty(clientUri) && HttpContext.Current != null)
                clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            int num = await loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput()
            {
                Type = loginType.GetHashCode(),
                UserName = username,
                LoginTime = DateTime.Now,
                LoginStatus = status.GetHashCode(),
                AppId = string.IsNullOrEmpty(clientId) ? null : clientId,
                ClientUri = clientUri,
                ClientIP = clientIp,
                ClientUA = HttpContext.Current.Request.UserAgent,
                ClientApiKey = apiKey,
                ClientDevice = deviceKey
            });
        }

        private async Task<long> CountSuccessLoggedIn(string username)
        {
            var unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            var loginHistoryBll = unityContainer.Resolve<ILoginHistoryBll>();
            return await loginHistoryBll.CountSuccessLoggedIn(username);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin") ?? "*";
			
            if(context.OwinContext.Response != null)
			{
				var origin = context.OwinContext.Response.Headers.Get("Origin");
				
				if (!string.IsNullOrEmpty(origin)){
					context.OwinContext.Response.Headers.Set("Access-Control-Allow-Origin", origin);
				}else
				{
				    const string corsHeader = "Access-Control-Allow-Origin";
				    if (!context.OwinContext.Response.Headers.ContainsKey(corsHeader))
                    {
                        context.OwinContext.Response.Headers.Add(corsHeader, new[] { allowedOrigin });
                    }
				}
			}
                
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new string[1]{ allowedOrigin});
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            IFormCollection form = await context.Request.ReadFormAsync();
            string clientAuthorization = form.Get("cauthorization");
            string clientRequestUri = form.Get("curi");
            string clientRquestMethod = form.Get("cmethod");
            if (!string.IsNullOrEmpty(clientRequestUri))
            {
                try
                {
                    clientRequestUri = Encoding.UTF8.GetString(Convert.FromBase64String(clientRequestUri));
                }
                catch
                {
                }
            }
            ApplicationUser user = null;
            Device deviceInfo = null;
            string clientApiSecret = null;
            if (!string.IsNullOrEmpty(clientAuthorization) && !string.IsNullOrEmpty(clientRequestUri) && !string.IsNullOrEmpty(clientRquestMethod))
            {
                string clientHostName = form.Get("chostname");
                string clientHostAddress = form.Get("chostaddress");
              //  Log.Logger.Debug("DuongDQ1");
                if (context.UserName == "dsvn-c1db31a7-27df-4e64-9cac-c1b238ea2b80" && context.Password == "0IdM+3dxTteHMp1VT2Rl5Vb2Z3fK8Pkbm8O7VgV9SBE=")
                {
                   // Log.Logger.Debug("DuongDQ2");
                    AuthenticationHeaderValue authHeaderValue = AuthenticationHeaderValue.Parse(clientAuthorization);
                    if (authHeaderValue != null && authHeaderValue.Scheme == "Cliamx" && !string.IsNullOrEmpty(authHeaderValue.Parameter))
                    {
                        string apiKey = this.GetUserClientApiKey(authHeaderValue.Parameter);
                       // Log.Logger.Debug("DuongDQ3");
                        Tuple<ApplicationUser, string> userFromClient = await GetUserByClientAuthorization(context.OwinContext, authHeaderValue.Parameter, clientRequestUri, clientRquestMethod, clientHostName, clientHostAddress);
                        if (userFromClient != null)
                        {
                            user = userFromClient.Item1;
                            clientApiSecret = userFromClient.Item2;
                        //    Log.Logger.Debug("DuongDQ3: Get user from client ok");
                            await LogHistory(LoginType.LoginApiKey, LoginStatus.Success, context.ClientId, null, null, apiKey, clientHostAddress, clientRequestUri);
                        }
                        else
                        {
                            //Log.Logger.Debug("DuongDQ3: Get user from client err");
                            await LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiInfo, context.ClientId, null, null, apiKey, clientHostAddress, clientRequestUri);
                        }
                    }
                    else
                        await LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiHeaderFormat, context.ClientId, null, null, null, clientHostAddress, clientRequestUri);
                }
                else
                    await LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiClientToken, context.ClientId, null, null, null, clientHostAddress, clientRequestUri);
            }
            else
            {
                string deviceKey = form.Get("deviceKey");
                if (!string.IsNullOrEmpty(deviceKey))
                {
                    var unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
                    var deviceBll = unityContainer.Resolve<IDeviceBll>();
                    deviceInfo = await deviceBll.GetOneDeviceByKey(string.IsNullOrEmpty(context.ClientId) ? "MSoatVe" : context.ClientId, deviceKey);
                    if (deviceInfo != null && deviceInfo.IsActived)
                    {
                        user = await userManager.FindAsync(context.UserName, context.Password);
                        if (user != null)
                            await LogHistory(LoginType.LoginDevice, LoginStatus.Success, context.ClientId, context.UserName, deviceKey, null);
                        else
                            await LogHistory(LoginType.LoginDevice, LoginStatus.InvalidUserNameOrPassword, context.ClientId, context.UserName, deviceKey, null);
                    }
                    else
                    {
                        context.SetError("invalid_grant", "The device infomation is incorrect.");
                        await LogHistory(LoginType.LoginDevice, LoginStatus.InvalidDeviceKey, context.ClientId, context.UserName, deviceKey, null);
                        
                    }
                }
                else
                {
                    user = await userManager.FindAsync(context.UserName, context.Password);
                    if (user != null)
                        await LogHistory(LoginType.LoginForm, LoginStatus.Success, context.ClientId, context.UserName, deviceKey, null);
                    else
                        await LogHistory(LoginType.LoginForm, LoginStatus.InvalidUserNameOrPassword, context.ClientId, context.UserName, deviceKey, null);
                }
            }
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
            }
            else
            {
                if (!string.IsNullOrEmpty(clientApiSecret))
                {
                    context.Options.AccessTokenExpireTimeSpan = ClientApiProvider.ClientTokenCacheTimeInMinutes > 0UL ? TimeSpan.FromMinutes(ClientApiProvider.ClientTokenCacheTimeInMinutes + 1UL) : TimeSpan.FromMinutes(10.0);
                }
                else
                    context.Options.AccessTokenExpireTimeSpan = Startup.AccessTokenExpireTimeSpan;
                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "Bearer");
                IList<Claim> userClaims = oAuthIdentity.Claims.Select(m => m).ToList();
                context.OwinContext.Set("data:claims", userClaims);
                foreach (string str in _mobileProfileClaims.Keys)
                {
                    string claimType = str;
                    var claim = oAuthIdentity.Claims.FirstOrDefault(m => m.Type == claimType);
                    if (claim != null)
                        oAuthIdentity.TryRemoveClaim(claim);
                }
                KeyValuePair<string, IList<Claim>> clientInfo = AppAuthorizeAttribute.GenerateClientInfo(user.UserName, TimeSpan.FromDays(2.0));
                string userClientId = clientInfo.Key;
                IList<Claim> userClientClaims = clientInfo.Value;
                if (userClientClaims.Count > 0)
                    oAuthIdentity.AddClaims(userClientClaims);
                if (deviceInfo != null)
                    oAuthIdentity.AddClaims(AppAuthorizeAttribute.GetDeviceInfoClaims(deviceInfo.DeviceKey, deviceInfo.DeviceName, null));
                string clientId = context.ClientId ?? string.Empty;
                var countSuccessLoggedIn = new long?();
                if (clientId == "MMuaVe")
                    countSuccessLoggedIn = await CountSuccessLoggedIn(user.UserName);
                AuthenticationProperties properties = await CreateProperties(clientId, user, userClientId, clientApiSecret, countSuccessLoggedIn, userClaims);
                var ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
            }
            
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            string str = context.Ticket.Properties.Dictionary["as:client_id"];
            string clientId = context.ClientId;
            if (str != clientId)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return Task.FromResult((object)null);
            }
            var identity = new ClaimsIdentity(context.Ticket.Identity);
            AppAuthorizeAttribute.UpdateClientInfoCacheTime(identity, TimeSpan.FromDays(2.0));
            if (context.Ticket.Properties.Dictionary.ContainsKey("isFirstLogin"))
                context.Ticket.Properties.Dictionary["isFirstLogin"] = bool.FalseString;
            if (clientId == "MMuaVe")
            {
                foreach (string key in _mobileProfileClaims.Values)
                {
                    if (context.Ticket.Properties.Dictionary.ContainsKey(key))
                        context.Ticket.Properties.Dictionary.Remove(key);
                }
            }
            var ticket = new AuthenticationTicket(identity, context.Ticket.Properties);
            context.Validated(ticket);
            return Task.FromResult((object)null);
        }

        private string GetUserClientApiKey(string clientAuthorization)
        {
            string[] autherizationHeaderValues = ClientApiProvider.GetAutherizationHeaderValues(clientAuthorization);
            if (autherizationHeaderValues != null && autherizationHeaderValues.Length > 0)
                return autherizationHeaderValues[0];
            return null;
        }

        private async Task<Tuple<ApplicationUser, string>> GetUserByClientAuthorization(IOwinContext context, string clientAuthorization, string clientRequestUri, string clientRequestMethod, string clientHostName, string clientHostAddress)
        {
            Tuple<ApplicationUser, string> result = null;
            string[] autherizationHeaderArray = ClientApiProvider.GetAutherizationHeaderValues(clientAuthorization);
            if (autherizationHeaderArray != null)
            {
               // Log.Logger.Debug("DuongDQ4");
                string apiKey = autherizationHeaderArray[0];
                string incomingSignatureHash = autherizationHeaderArray[1];
                string nonce = autherizationHeaderArray[2];
                string requestTimeStamp = autherizationHeaderArray[3];
                var userBll = UnityConfig.GetConfiguredContainer().Resolve<IUserBll>();
                UserApp userApp = await userBll.GetUserApp(apiKey);
                if (userApp != null && !string.IsNullOrEmpty(userApp.ApiSecret))
                {
                   // Log.Logger.Debug("DuongDQ5: Get user app ok");
                    if (CheckClientHost(userApp.AppHosts, userApp.AppIps, clientHostName, clientHostAddress))
                    {
                       // Log.Logger.Debug("DuongDQ6: check host ok");
                        bool isValid;
                        try
                        {
                            isValid = ClientApiProvider.isValidRequest(clientRequestUri, clientRequestMethod, userApp.ApiKey, userApp.ApiSecret, incomingSignatureHash, nonce, requestTimeStamp);
                        }
                        catch
                        {
                            isValid = false;
                        }
                        if (isValid)
                        {
                           // Log.Logger.Debug("DuongDQ7: Valid request");
                            var userManager = context.GetUserManager<ApplicationUserManager>();
                            ApplicationUser user = await userManager.FindByIdAsync(userApp.UserId);
                            if (user != null)
                            {
                                //Log.Logger.Debug(string.Format("{0}: Find user ok", (object)clientHostAddress));
                                result = new Tuple<ApplicationUser, string>(user, userApp.ApiSecret);
                            }
                            else
                                Log.Logger.Debug(string.Format("{0}: Not found user", clientHostAddress));
                        }
                        else
                            Log.Logger.Debug(string.Format("{0}: Invalid request", clientHostAddress));
                    }
                }
                else
                    Log.Logger.Debug(string.Format("{0}: Invalid api key", clientHostAddress));
            }
            return result;
        }

        private bool CheckClientHost(string allowHosts, string allowIps, string clientHostName, string clientHostAddress)
        {
            var flag = true;
            if (!string.IsNullOrEmpty(allowHosts))
            {
                flag = !string.IsNullOrEmpty(clientHostName) && allowHosts.Split(new[]{';',','}).Select(m => m.Trim().ToLower()).Contains(clientHostName.ToLower());
            }
            if (flag && !string.IsNullOrEmpty(allowIps))
            {
                flag = !string.IsNullOrEmpty(clientHostAddress) && allowIps.Split(new[]{';',','}).Select(m => m.Trim().ToLower()).Contains(clientHostAddress.ToLower());
            }
            return flag;
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> keyValuePair in context.Properties.Dictionary)
            {
                if (keyValuePair.Key == "isFirstLogin")
                {
                    bool result;
                    if (!bool.TryParse(keyValuePair.Value, out result))
                        result = false;
                    context.AdditionalResponseParameters.Add(keyValuePair.Key, result ? 1 : 0);
                }
                else
                    context.AdditionalResponseParameters.Add(keyValuePair.Key, keyValuePair.Value);
            }
            if (context.Properties.Dictionary["as:client_id"] == "MMuaVe")
            {
                var list = context.OwinContext.Get<IList<Claim>>("data:claims");
                if (list != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in _mobileProfileClaims)
                    {
                        KeyValuePair<string, string> profileClaim = keyValuePair;
                        Claim claim = list.FirstOrDefault(m => m.Type == profileClaim.Key);
                        if (claim != null)
                            context.AdditionalResponseParameters.Add(profileClaim.Value, string.IsNullOrEmpty(claim.Value) ? string.Empty : claim.Value);
                        else
                            context.AdditionalResponseParameters.Add(profileClaim.Value, string.Empty);
                    }
                }
            }
            return Task.FromResult<object>(null);
        }

        private LoginType DetectLoginType(IFormCollection form)
        {
            if (!string.IsNullOrEmpty(form.Get("grant_type")) && form.Get("grant_type").Equals("refresh_token"))
                return LoginType.RefreshToken;
            if (!string.IsNullOrEmpty(form.Get("cauthorization")))
                return LoginType.LoginApiKey;
            return !string.IsNullOrEmpty(form.Get("deviceKey")) ? LoginType.LoginDevice : LoginType.LoginForm;
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                context.TryGetFormCredentials(out clientId, out clientSecret);
            if (context.ClientId == null)
            {
                context.Validated();
            }
            else
            {
                var unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
                var refreshTokenBll = unityContainer.Resolve<IRefreshTokenBll>();
                Client client = await refreshTokenBll.GetOneClient(context.ClientId);
                if (client == null)
                {
                    context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                    IFormCollection form = await context.Request.ReadFormAsync();
                    await LogHistory(DetectLoginType(form), LoginStatus.InvalidCientId, context.ClientId, null, null, null);
                }
                else
                {
                    if (client.ApplicationType == ApplicationTypes.NativeConfidential)
                    {
                        if (string.IsNullOrWhiteSpace(clientSecret))
                        {
                            context.SetError("invalid_clientId", "Client secret should be sent.");
                            IFormCollection form = await context.Request.ReadFormAsync();
                            await LogHistory(DetectLoginType(form), LoginStatus.InvalidCientSecret, context.ClientId, null, null, null);
                            
                        }
                        else if (client.Secret != GetSecretHash(clientSecret))
                        {
                            context.SetError("invalid_clientId", "Client secret is invalid.");
                            IFormCollection form = await context.Request.ReadFormAsync();
                            await LogHistory(DetectLoginType(form), LoginStatus.InvalidCientSecret, context.ClientId, null, null, null);
                            
                        }
                    }
                    if (!client.Active)
                    {
                        context.SetError("invalid_clientId", "Client is inactive.");
                        IFormCollection form = await context.Request.ReadFormAsync();
                        await LogHistory(DetectLoginType(form), LoginStatus.InvalidCientActive, context.ClientId, null, null, null);
                    }
                    else
                    {
                        context.OwinContext.Set("as:clientAllowedOrigin", client.AllowedOrigin);
                        context.OwinContext.Set("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString(CultureInfo.InvariantCulture));
                        context.Validated();
                    }
                }
            }
            
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                if (new Uri(context.Request.Uri, "/").AbsoluteUri == context.RedirectUri)
                    context.Validated();
                context.Validated();
            }
            return Task.FromResult((object)null);
        }

        public static async Task<AuthenticationProperties> CreateProperties(string clientId, ApplicationUser user, string userClientId, string clientApiSecret, long? countSuccessLoggedIn, IList<Claim> userClaims)
        {
            var userManager = HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            Claim claim = userClaims.FirstOrDefault(m => m.Type == "displayName");
            IDictionary<string, string> dictionary1 = new Dictionary<string, string>();
            dictionary1.Add("as:client_id", clientId);
            dictionary1.Add("userName", user.UserName);
            dictionary1.Add("displayName", claim != null ? claim.Value : user.UserName);
            dictionary1.Add("userClientId", userClientId);
            dictionary1.Add("email", string.IsNullOrEmpty(user.Email) ? string.Empty : user.Email);
            if (!string.IsNullOrEmpty(clientApiSecret))
                dictionary1.Add("client_api_secret", clientApiSecret);
            if (countSuccessLoggedIn.HasValue)
            {
                IDictionary<string, string> dictionary2 = dictionary1;
                const string key = "isFirstLogin";
                long? nullable = countSuccessLoggedIn;
                string str = (nullable.GetValueOrDefault() != 1L ? 0 : (1)) != 0 ? bool.TrueString : bool.FalseString;
                dictionary2.Add(key, str);
            }
            IList<string> result = userManager.GetRolesAsync(user.Id).Result;
            dictionary1.Add("roles", string.Join(",", result));
            return new AuthenticationProperties(dictionary1);
        }

        public override Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            IHeaderDictionary headers = context.Request.Headers;
            string input = headers.Get("Authorization");
            if (!string.IsNullOrEmpty(input))
            {
                AuthenticationHeaderValue authenticationHeaderValue = AuthenticationHeaderValue.Parse(input);
                if (authenticationHeaderValue != null && authenticationHeaderValue.Scheme == "Cliamx")
                {
                    string result = ClientApiProvider.GetAccessToken(authenticationHeaderValue, context.Request).Result;
                    if (!string.IsNullOrEmpty(result))
                    {
                        string str = new AuthenticationHeaderValue("Bearer", result).ToString();
                        context.Request.Headers.Set("Authorization", str);
                    }
                }
                else if (authenticationHeaderValue != null && authenticationHeaderValue.Scheme == "Bearer")
                {
                    string username = headers.Get("userName");
                    string realAccessToken;
                    if (AppAuthorizeAttribute.ValidateOAuthAuthorizationHeader(username, authenticationHeaderValue.Parameter, out realAccessToken))
                    {
                        string str = new AuthenticationHeaderValue(authenticationHeaderValue.Scheme, realAccessToken).ToString();
                        context.Request.Headers.Set("Authorization", str);
                    }
                    else
                        context.Request.Headers.Remove("Authorization");
                }
            }
            return base.MatchEndpoint(context);
        }

        public static string GetSecretHash(string input)
        {
            return Convert.ToBase64String(new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}