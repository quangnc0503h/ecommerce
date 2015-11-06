
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        //private static readonly ICacheClient _redisCache = (ICacheClient)new RedisCacheClient(RedisConnection.SecurityConn, (ISerializer)null, 0);
        private static Dictionary<string, string> _mobileProfileClaims = new Dictionary<string, string>()
    {
      {
        "profile:cmnd",
        "cmnd"
      },
      {
        "profile:cty",
        "congTy"
      },
      {
        "profile:diachi",
        "diaChi"
      },
      {
        "profile:mst",
        "maSoThue"
      },
      {
        "profile:avartar",
        "avatar"
      }
    };
        private const string CientMobileMuaVe = "MMuaVe";
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
                throw new ArgumentNullException("publicClientId");
            this._publicClientId = publicClientId;
        }

        private async Task LogHistory(LoginType loginType, LoginStatus status, string clientId, string username, string deviceKey, string apiKey, string clientIp = null, string clientUri = null)
        {
            UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            ILoginHistoryBll loginHistoryBll = UnityContainerExtensions.Resolve<ILoginHistoryBll>((IUnityContainer)unityContainer);
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
                AppId = string.IsNullOrEmpty(clientId) ? (string)null : clientId,
                ClientUri = clientUri,
                ClientIP = clientIp,
                ClientUA = HttpContext.Current.Request.UserAgent,
                ClientApiKey = apiKey,
                ClientDevice = deviceKey
            });
        }

        private async Task<long> CountSuccessLoggedIn(string username)
        {
            UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            ILoginHistoryBll loginHistoryBll = UnityContainerExtensions.Resolve<ILoginHistoryBll>((IUnityContainer)unityContainer);
            return await loginHistoryBll.CountSuccessLoggedIn(username);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin") ?? "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new string[1]
            {
        allowedOrigin
            });
            ApplicationUserManager userManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(context.OwinContext);
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
            ApplicationUser user = (ApplicationUser)null;
            Device deviceInfo = (Device)null;
            string clientApiSecret = (string)null;
            if (!string.IsNullOrEmpty(clientAuthorization) && !string.IsNullOrEmpty(clientRequestUri) && !string.IsNullOrEmpty(clientRquestMethod))
            {
                string clientHostName = form.Get("chostname");
                string clientHostAddress = form.Get("chostaddress");
                Log.Logger.Debug("DuongDQ1");
                if (context.UserName == "dsvn-c1db31a7-27df-4e64-9cac-c1b238ea2b80" && context.Password == "0IdM+3dxTteHMp1VT2Rl5Vb2Z3fK8Pkbm8O7VgV9SBE=")
                {
                    Log.Logger.Debug("DuongDQ2");
                    AuthenticationHeaderValue authHeaderValue = AuthenticationHeaderValue.Parse(clientAuthorization);
                    if (authHeaderValue != null && authHeaderValue.Scheme == "Cliamx" && !string.IsNullOrEmpty(authHeaderValue.Parameter))
                    {
                        string apiKey = this.GetUserClientApiKey(authHeaderValue.Parameter);
                        Log.Logger.Debug("DuongDQ3");
                        Tuple<ApplicationUser, string> userFromClient = await this.GetUserByClientAuthorization(context.OwinContext, authHeaderValue.Parameter, clientRequestUri, clientRquestMethod, clientHostName, clientHostAddress);
                        if (userFromClient != null)
                        {
                            user = userFromClient.Item1;
                            clientApiSecret = userFromClient.Item2;
                            Log.Logger.Debug("DuongDQ3: Get user from client ok");
                            await this.LogHistory(LoginType.LoginApiKey, LoginStatus.Success, context.ClientId, (string)null, (string)null, apiKey, clientHostAddress, clientRequestUri);
                        }
                        else
                        {
                            Log.Logger.Debug("DuongDQ3: Get user from client err");
                            await this.LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiInfo, context.ClientId, (string)null, (string)null, apiKey, clientHostAddress, clientRequestUri);
                        }
                    }
                    else
                        await this.LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiHeaderFormat, context.ClientId, (string)null, (string)null, (string)null, clientHostAddress, clientRequestUri);
                }
                else
                    await this.LogHistory(LoginType.LoginApiKey, LoginStatus.InvalidApiClientToken, context.ClientId, (string)null, (string)null, (string)null, clientHostAddress, clientRequestUri);
            }
            else
            {
                string deviceKey = form.Get("deviceKey");
                if (!string.IsNullOrEmpty(deviceKey))
                {
                    UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
                    IDeviceBll deviceBll = UnityContainerExtensions.Resolve<IDeviceBll>((IUnityContainer)unityContainer);
                    deviceInfo = await deviceBll.GetOneDeviceByKey(string.IsNullOrEmpty(context.ClientId) ? "MSoatVe" : context.ClientId, deviceKey);
                    if (deviceInfo != null && deviceInfo.IsActived)
                    {
                        user = await userManager.FindAsync(context.UserName, context.Password);
                        if (user != null)
                            await this.LogHistory(LoginType.LoginDevice, LoginStatus.Success, context.ClientId, context.UserName, deviceKey, (string)null, (string)null, (string)null);
                        else
                            await this.LogHistory(LoginType.LoginDevice, LoginStatus.InvalidUserNameOrPassword, context.ClientId, context.UserName, deviceKey, (string)null, (string)null, (string)null);
                    }
                    else
                    {
                        context.SetError("invalid_grant", "The device infomation is incorrect.");
                        await this.LogHistory(LoginType.LoginDevice, LoginStatus.InvalidDeviceKey, context.ClientId, context.UserName, deviceKey, (string)null, (string)null, (string)null);
                        goto label_48;
                    }
                }
                else
                {
                    user = await userManager.FindAsync(context.UserName, context.Password);
                    if (user != null)
                        await this.LogHistory(LoginType.LoginForm, LoginStatus.Success, context.ClientId, context.UserName, deviceKey, (string)null, (string)null, (string)null);
                    else
                        await this.LogHistory(LoginType.LoginForm, LoginStatus.InvalidUserNameOrPassword, context.ClientId, context.UserName, deviceKey, (string)null, (string)null, (string)null);
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
                    if (ClientApiProvider.ClientTokenCacheTimeInMinutes > 0UL)
                        context.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes((double)(ClientApiProvider.ClientTokenCacheTimeInMinutes + 1UL));
                    else
                        context.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(10.0);
                }
                else
                    context.Options.AccessTokenExpireTimeSpan = Startup.AccessTokenExpireTimeSpan;
                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync((UserManager<ApplicationUser, int>)userManager, "Bearer");
                IList<Claim> userClaims = (IList<Claim>)Enumerable.ToList<Claim>(Enumerable.Select<Claim, Claim>(oAuthIdentity.Claims, (Func<Claim, Claim>)(m => m)));
                context.OwinContext.Set<IList<Claim>>("data:claims", userClaims);
                foreach (string str in ApplicationOAuthProvider._mobileProfileClaims.Keys)
                {
                    string claimType = str;
                    Claim claim = Enumerable.FirstOrDefault<Claim>(oAuthIdentity.Claims, (Func<Claim, bool>)(m => m.Type == claimType));
                    if (claim != null)
                        oAuthIdentity.TryRemoveClaim(claim);
                }
                KeyValuePair<string, IList<Claim>> clientInfo = AppAuthorizeAttribute.GenerateClientInfo(user.UserName, TimeSpan.FromDays(2.0));
                string userClientId = clientInfo.Key;
                IList<Claim> userClientClaims = clientInfo.Value;
                if (userClientClaims.Count > 0)
                    oAuthIdentity.AddClaims((IEnumerable<Claim>)userClientClaims);
                if (deviceInfo != null)
                    oAuthIdentity.AddClaims(AppAuthorizeAttribute.GetDeviceInfoClaims(deviceInfo.DeviceKey, deviceInfo.DeviceName, (IEnumerable<string>)null));
                string clientId = context.ClientId == null ? string.Empty : context.ClientId;
                long? countSuccessLoggedIn = new long?();
                if (clientId == "MMuaVe")
                    countSuccessLoggedIn = new long?(await this.CountSuccessLoggedIn(user.UserName));
                AuthenticationProperties properties = await ApplicationOAuthProvider.CreateProperties(clientId, user, userClientId, clientApiSecret, countSuccessLoggedIn, userClaims);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
            }
            label_48:;
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            string str = context.Ticket.Properties.Dictionary["as:client_id"];
            string clientId = context.ClientId;
            if (str != clientId)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                return (Task)Task.FromResult<object>((object)null);
            }
            ClaimsIdentity identity = new ClaimsIdentity((IIdentity)context.Ticket.Identity);
            AppAuthorizeAttribute.UpdateClientInfoCacheTime(identity, TimeSpan.FromDays(2.0));
            if (context.Ticket.Properties.Dictionary.ContainsKey("isFirstLogin"))
                context.Ticket.Properties.Dictionary["isFirstLogin"] = bool.FalseString;
            if (clientId == "MMuaVe")
            {
                foreach (string key in ApplicationOAuthProvider._mobileProfileClaims.Values)
                {
                    if (context.Ticket.Properties.Dictionary.ContainsKey(key))
                        context.Ticket.Properties.Dictionary.Remove(key);
                }
            }
            AuthenticationTicket ticket = new AuthenticationTicket(identity, context.Ticket.Properties);
            context.Validated(ticket);
            return (Task)Task.FromResult<object>((object)null);
        }

        private string GetUserClientApiKey(string clientAuthorization)
        {
            string[] autherizationHeaderValues = ClientApiProvider.GetAutherizationHeaderValues(clientAuthorization);
            if (autherizationHeaderValues != null && autherizationHeaderValues.Length > 0)
                return autherizationHeaderValues[0];
            return (string)null;
        }

        private async Task<Tuple<ApplicationUser, string>> GetUserByClientAuthorization(IOwinContext context, string clientAuthorization, string clientRequestUri, string clientRequestMethod, string clientHostName, string clientHostAddress)
        {
            Tuple<ApplicationUser, string> result = (Tuple<ApplicationUser, string>)null;
            string[] autherizationHeaderArray = ClientApiProvider.GetAutherizationHeaderValues(clientAuthorization);
            if (autherizationHeaderArray != null)
            {
                Log.Logger.Debug("DuongDQ4");
                string apiKey = autherizationHeaderArray[0];
                string incomingSignatureHash = autherizationHeaderArray[1];
                string nonce = autherizationHeaderArray[2];
                string requestTimeStamp = autherizationHeaderArray[3];
                IUserBll userBll = UnityContainerExtensions.Resolve<IUserBll>(UnityConfig.GetConfiguredContainer());
                UserApp userApp = await userBll.GetUserApp(apiKey);
                if (userApp != null && !string.IsNullOrEmpty(userApp.ApiSecret))
                {
                    Log.Logger.Debug("DuongDQ5: Get user app ok");
                    if (this.CheckClientHost(userApp.AppHosts, userApp.AppIps, clientHostName, clientHostAddress))
                    {
                        Log.Logger.Debug("DuongDQ6: check host ok");
                        bool isValid = false;
                        try
                        {
                            isValid = await ClientApiProvider.isValidRequest(clientRequestUri, clientRequestMethod, userApp.ApiKey, userApp.ApiSecret, incomingSignatureHash, nonce, requestTimeStamp);
                        }
                        catch
                        {
                            isValid = false;
                        }
                        if (isValid)
                        {
                            Log.Logger.Debug("DuongDQ7: Valid request");
                            ApplicationUserManager userManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(context);
                            ApplicationUser user = await userManager.FindByIdAsync(userApp.UserId);
                            if (user != null)
                            {
                                Log.Logger.Debug(string.Format("{0}: Find user ok", (object)clientHostAddress));
                                result = new Tuple<ApplicationUser, string>(user, userApp.ApiSecret);
                            }
                            else
                                Log.Logger.Debug(string.Format("{0}: Not found user", (object)clientHostAddress));
                        }
                        else
                            Log.Logger.Debug(string.Format("{0}: Invalid request", (object)clientHostAddress));
                    }
                }
                else
                    Log.Logger.Debug(string.Format("{0}: Invalid api key", (object)clientHostAddress));
            }
            return result;
        }

        private bool CheckClientHost(string allowHosts, string allowIps, string clientHostName, string clientHostAddress)
        {
            bool flag = true;
            if (flag && !string.IsNullOrEmpty(allowHosts))
            {
                if (!string.IsNullOrEmpty(clientHostName))
                    flag = Enumerable.Contains<string>(Enumerable.Select<string, string>((IEnumerable<string>)allowHosts.Split(new char[2]
                    {
            ';',
            ','
                    }), (Func<string, string>)(m => m.Trim().ToLower())), clientHostName.ToLower());
                else
                    flag = false;
            }
            if (flag && !string.IsNullOrEmpty(allowIps))
            {
                if (!string.IsNullOrEmpty(clientHostAddress))
                    flag = Enumerable.Contains<string>(Enumerable.Select<string, string>((IEnumerable<string>)allowIps.Split(new char[2]
                    {
            ';',
            ','
                    }), (Func<string, string>)(m => m.Trim().ToLower())), clientHostAddress.ToLower());
                else
                    flag = false;
            }
            return flag;
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)context.Properties.Dictionary)
            {
                if (keyValuePair.Key == "isFirstLogin")
                {
                    bool result;
                    if (!bool.TryParse(keyValuePair.Value, out result))
                        result = false;
                    context.AdditionalResponseParameters.Add(keyValuePair.Key, (object)(result ? 1 : 0));
                }
                else
                    context.AdditionalResponseParameters.Add(keyValuePair.Key, (object)keyValuePair.Value);
            }
            if (context.Properties.Dictionary["as:client_id"] == "MMuaVe")
            {
                IList<Claim> list = context.OwinContext.Get<IList<Claim>>("data:claims");
                if (list != null)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in ApplicationOAuthProvider._mobileProfileClaims)
                    {
                        KeyValuePair<string, string> profileClaim = keyValuePair;
                        Claim claim = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)list, (Func<Claim, bool>)(m => m.Type == profileClaim.Key));
                        if (claim != null)
                            context.AdditionalResponseParameters.Add(profileClaim.Value, string.IsNullOrEmpty(claim.Value) ? (object)string.Empty : (object)claim.Value);
                        else
                            context.AdditionalResponseParameters.Add(profileClaim.Value, (object)string.Empty);
                    }
                }
            }
            return (Task)Task.FromResult<object>((object)null);
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
            Client client = (Client)null;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                context.TryGetFormCredentials(out clientId, out clientSecret);
            if (context.ClientId == null)
            {
                context.Validated();
            }
            else
            {
                UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
                IRefreshTokenBll refreshTokenBll = UnityContainerExtensions.Resolve<IRefreshTokenBll>((IUnityContainer)unityContainer);
                client = await refreshTokenBll.GetOneClient(context.ClientId);
                if (client == null)
                {
                    context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", (object)context.ClientId));
                    IFormCollection form = await context.Request.ReadFormAsync();
                    await this.LogHistory(this.DetectLoginType(form), LoginStatus.InvalidCientId, context.ClientId, (string)null, (string)null, (string)null, (string)null, (string)null);
                }
                else
                {
                    if (client.ApplicationType == ApplicationTypes.NativeConfidential)
                    {
                        if (string.IsNullOrWhiteSpace(clientSecret))
                        {
                            context.SetError("invalid_clientId", "Client secret should be sent.");
                            IFormCollection form = await context.Request.ReadFormAsync();
                            await this.LogHistory(this.DetectLoginType(form), LoginStatus.InvalidCientSecret, context.ClientId, (string)null, (string)null, (string)null, (string)null, (string)null);
                            
                        }
                        else if (client.Secret != ApplicationOAuthProvider.GetSecretHash(clientSecret))
                        {
                            context.SetError("invalid_clientId", "Client secret is invalid.");
                            IFormCollection form = await context.Request.ReadFormAsync();
                            await this.LogHistory(this.DetectLoginType(form), LoginStatus.InvalidCientSecret, context.ClientId, (string)null, (string)null, (string)null, (string)null, (string)null);
                            
                        }
                    }
                    if (!client.Active)
                    {
                        context.SetError("invalid_clientId", "Client is inactive.");
                        IFormCollection form = await context.Request.ReadFormAsync();
                        await this.LogHistory(this.DetectLoginType(form), LoginStatus.InvalidCientActive, context.ClientId, (string)null, (string)null, (string)null, (string)null, (string)null);
                    }
                    else
                    {
                        context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
                        context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());
                        context.Validated();
                    }
                }
            }
            
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == this._publicClientId)
            {
                if (new Uri(context.Request.Uri, "/").AbsoluteUri == context.RedirectUri)
                    context.Validated();
                context.Validated();
            }
            return (Task)Task.FromResult<object>((object)null);
        }

        public static async Task<AuthenticationProperties> CreateProperties(string clientId, ApplicationUser user, string userClientId, string clientApiSecret, long? countSuccessLoggedIn, IList<Claim> userClaims)
        {
            ApplicationUserManager userManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            Claim claim = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)userClaims, (Func<Claim, bool>)(m => m.Type == "displayName"));
            IDictionary<string, string> dictionary1 = (IDictionary<string, string>)new Dictionary<string, string>();
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
                string key = "isFirstLogin";
                long? nullable = countSuccessLoggedIn;
                string str = (nullable.GetValueOrDefault() != 1L ? 0 : (nullable.HasValue ? 1 : 0)) != 0 ? bool.TrueString : bool.FalseString;
                dictionary2.Add(key, str);
            }
            IList<string> result = userManager.GetRolesAsync(user.Id).Result;
            dictionary1.Add("roles", string.Join(",", (IEnumerable<string>)result));
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
                    string realAccessToken = (string)null;
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