using System.Globalization;
using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Api.Results;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{

    [RoutePrefix("api/Account")]
    [AppAuthorize]
    public class AccountController : BaseApiController
    {
        /*
                private const string LocalLoginProvider = "Local";
        */
        /// private ApplicationUserManager _userManager;
        private readonly IUserBll _userBll;
        private readonly ILoginHistoryBll _loginHistoryBll;



        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        private IAuthenticationManager Authentication
        {
            get
            {
                return Request.GetOwinContext().Authentication;
            }
        }

        public AccountController()
        {
        }

        public AccountController(IUserBll userBll, ILoginHistoryBll loginHistoryBll)
        {
            _userBll = userBll;
            _loginHistoryBll = loginHistoryBll;
        }

        [HostAuthentication("ExternalBearer")]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            ExternalLoginData externalLoginData = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            return new UserInfoViewModel
                   {
                       Email = User.Identity.GetUserName(),
                       HasRegistered = externalLoginData == null,
                       LoginProvider = externalLoginData != null ? externalLoginData.LoginProvider : null
                   };
        }

        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            var identity = User.Identity as ClaimsIdentity;
            Authentication.SignOut("Cookies");
            AppAuthorizeAttribute.ClearAuthClientInfo(identity);
            return Ok();
        }

        [Route("Logout4Mobile")]
        public LogoutMobileOutput Logout4Mobile()
        {
            var logoutMobileOutput = new LogoutMobileOutput
                                     {
                Status = 0
            };
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                Authentication.SignOut("Cookies");
                if (identity != null)
                    AppAuthorizeAttribute.ClearAuthClientInfo(identity);
            }
            catch (Exception ex)
            {
                logoutMobileOutput.Status = 1;
                logoutMobileOutput.ErrorMsg = ex.Message;
            }
            return logoutMobileOutput;
        }

        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            var user = (IdentityUser)await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            ManageInfoViewModel manageInfoViewModel;
            if (user == null)
            {
                manageInfoViewModel = null;
            }
            else
            {
                var logins = new List<UserLoginInfoViewModel>();
                if (user.PasswordHash != null)
                    logins.Add(new UserLoginInfoViewModel
                               {
                                   LoginProvider = "Local",
                                   ProviderKey = user.UserName
                               });
                manageInfoViewModel = new ManageInfoViewModel
                                      {
                                          LocalLoginProvider = "Local",
                                          Email = user.UserName,
                                          Logins = logins,
                                          ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
                                      };
            }
            return manageInfoViewModel;
        }

        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                await LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    await LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                    httpActionResult = GetErrorResult(result);
                }
                else
                {
                    await LogHistory(LoginType.ChangePassword, LoginStatus.Success, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                    httpActionResult = Ok();
                }
            }
            return httpActionResult;
        }

        [Route("ChangePassword4Mobile")]
        public async Task<ChangePasswordMobileOutput> ChangePassword4Mobile(ChangePasswordBindingModel model)
        {
            var output = new ChangePasswordMobileOutput
                         {
                Status = 0
            };
            ChangePasswordMobileOutput passwordMobileOutput;
            if (!ModelState.IsValid)
            {
                output.Status = 1;
                output.ErrorMsg = "Invalid input";
                await LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                passwordMobileOutput = output;
            }
            else
            {
                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    output.Status = 1;
                    output.ErrorMsg = result.Errors == null || !result.Errors.Any() ? "Unknown errors" : result.Errors.First();
                    await LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                }
                else
                    await LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, User.GetConnectAppId(), User.Identity.GetUserName(), User.GetConnectDeviceKey(), null);
                passwordMobileOutput = output;
            }
            return passwordMobileOutput;
        }

        [Route("GetMobileProfile")]
        public async Task<GetMobileProfileOutput> GetMobileProfile()
        {
            var output = await _userBll.GetMobileProfile(User.Identity.GetUserId<int>());
            return output;
        }

        [Route("UpdateMobileProfile")]
        [HttpPost]
        public async Task<UpdateMobileProfileOutput> UpdateMobileProfile(UpdateMobileProfileInput input)
        {
            UpdateMobileProfileOutput output = await _userBll.UpdateMobileProfile(User.Identity.GetUserId<int>(), input);
            return output;
        }

        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user == null)
                    httpActionResult = BadRequest("Not found user");
                else if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    httpActionResult = BadRequest("Not allow create password");
                }
                else
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId<int>(), model.NewPassword);
                    httpActionResult = result.Succeeded ? Ok() : GetErrorResult(result);
                }
            }
            return httpActionResult;
        }

        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                Authentication.SignOut("ExternalCookie");
                AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
                if (ticket == null || ticket.Identity == null || ticket.Properties != null && ticket.Properties.ExpiresUtc.HasValue && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow)
                {
                    httpActionResult = BadRequest("External login failure.");
                }
                else
                {
                    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);
                    if (externalData == null)
                    {
                        httpActionResult = BadRequest("The external login is already associated with an account.");
                    }
                    else
                    {
                        IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));
                        httpActionResult = result.Succeeded ? Ok() : GetErrorResult(result);
                    }
                }
            }
            return httpActionResult;
        }

        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                IdentityResult result;
                if (model.LoginProvider == "Local")
                    result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId<int>());
                else
                    result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(model.LoginProvider, model.ProviderKey));
                httpActionResult = result.Succeeded ? Ok() : GetErrorResult(result);
            }
            return httpActionResult;
        }

        [Route("ExternalLogin", Name = "ExternalLogin")]
        [OverrideAuthentication]
        [HostAuthentication("ExternalCookie")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;
            IHttpActionResult httpActionResult;
            if (error != null)
                httpActionResult = BadRequest(Uri.EscapeDataString(error));
            else if (!User.Identity.IsAuthenticated)
            {
                httpActionResult = new ChallengeResult(provider, this);
            }
            else
            {
                string redirectUriValidationResult = ValidateClientAndRedirectUri(ref redirectUri);
                if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
                {
                    httpActionResult = BadRequest(redirectUriValidationResult);
                }
                else
                {
                    var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
                    if (externalLogin == null)
                        httpActionResult = InternalServerError();
                    else if (externalLogin.LoginProvider != provider)
                    {
                        Authentication.SignOut("ExternalCookie");
                        httpActionResult = new ChallengeResult(provider, this);
                    }
                    else
                    {
                        string email = string.Empty;
                        var externalIdentity = await HttpContext.Current.GetOwinContext().Authentication.GetExternalIdentityAsync("ExternalCookie");
                        if (externalIdentity != null)
                        {
                            var claim = externalIdentity.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                            if (claim != null)
                                email = claim.Value;
                        }
                        ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
                        bool hasRegistered = user != null;
                        redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&external_email={5}", (object)redirectUri, (object)externalLogin.ExternalAccessToken, (object)externalLogin.LoginProvider, (object)hasRegistered.ToString(), (object)externalLogin.UserName, (object)email);
                        httpActionResult = Redirect(redirectUri);
                    }
                }
            }
            return httpActionResult;
        }

        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> authenticationTypes = Authentication.GetExternalAuthenticationTypes();
            string str = !generateState ? null : RandomOAuthStateGenerator.Generate(256);
            return authenticationTypes.Select(authenticationDescription => new ExternalLoginViewModel
                                                                           {
                                                                               Name = authenticationDescription.Caption,
                                                                               Url = Url.Route("ExternalLogin", new
                                                                                                                {
                                                                                                                    provider = authenticationDescription.AuthenticationType,
                                                                                                                    response_type= "token",
                                                                                                                    client_id= Startup.PublicClientId,
                                                                                                                    redirect_uri=new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                                                                                                                    state= str}),
                                                                                                                    State = str}).ToList();
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                var applicationUser = new ApplicationUser {UserName = model.Email, Email = model.Email};
                ApplicationUser user = applicationUser;
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                httpActionResult = result.Succeeded ? Ok() : GetErrorResult(result);
            }
            return httpActionResult;
        }

        [AllowAnonymous]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!ModelState.IsValid)
            {
                httpActionResult = BadRequest(ModelState);
            }
            else
            {
                var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
                if (verifiedAccessToken == null)
                {
                    httpActionResult = BadRequest("Invalid Provider or External Access Token");
                }
                else
                {
                    ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
                    var user = await UserManager.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.ProviderKey));
                    var hasRegistered = user != null;
                    if (hasRegistered)
                    {
                        httpActionResult = BadRequest("External user is already registered");
                    }
                    else
                    {
                        var applicationUser = new ApplicationUser
                                              {
                                                  UserName =model.Provider.ToLower() +verifiedAccessToken.ProviderKey.ToLower(),
                                                  Email = model.Email
                                              };
                        user = applicationUser;
                        var result = await UserManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            httpActionResult = GetErrorResult(result);
                        }
                        else
                        {
                            var info = new ExternalLoginInfo
                                       {
                                DefaultUserName = user.UserName,
                                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.ProviderKey)
                            };
                            result = await UserManager.AddLoginAsync(user.Id, info.Login);
                            if (!result.Succeeded)
                            {
                                httpActionResult = GetErrorResult(result);
                            }
                            else
                            {
                                var accessTokenResponse = await GenerateLocalAccessTokenResponse(user);
                                httpActionResult = Ok(accessTokenResponse);
                            }
                        }
                    }
                }
            }
            return httpActionResult;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {
            IHttpActionResult httpActionResult;
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                httpActionResult = BadRequest("Provider or external access token is not sent");
            }
            else
            {
                var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken);
                if (verifiedAccessToken == null)
                {
                    httpActionResult = BadRequest("Invalid Provider or External Access Token");
                }
                else
                {
                    ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.ProviderKey));
                    bool hasRegistered = user != null;
                    if (!hasRegistered)
                    {
                        httpActionResult = BadRequest("External user is not registered");
                    }
                    else
                    {
                        var accessTokenResponse = await GenerateLocalAccessTokenResponse(user);
                        httpActionResult = Ok(accessTokenResponse);
                    }
                }
            }
            return httpActionResult;
        }
        private async Task LogHistory(LoginType loginType, LoginStatus status, string clientId, string username, string device, string apiKey)
        {
            string clientIp = SecurityUtils.GetClientIPAddress();
            string clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            await _loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput
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
                                                          ClientDevice = device
                                                      });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }

            base.Dispose(disposing);
        }

        private string ValidateClientAndRedirectUri(ref string redirectUriOutput)
        {
            string queryString1 = GetQueryString(Request, "redirect_uri");
            if (string.IsNullOrWhiteSpace(queryString1))
                return "redirect_uri is required";
            Uri result;
            if (!Uri.TryCreate(queryString1, UriKind.Absolute, out result))
                return "redirect_uri is invalid";
            string queryString2 = GetQueryString(Request, "client_id");
            if (string.IsNullOrWhiteSpace(queryString2))
                return "client_Id is required";
            if (queryString2 != "ngAuthApp")
                return string.Format("Client_id '{0}' is not registered in the system.", queryString2);
            redirectUriOutput = result.AbsoluteUri;
            return string.Empty;
        }
        //private string GetQueryString(HttpRequestMessage request, string key)
        //{
        //    IEnumerable<KeyValuePair<string, string>> queryNameValuePairs = System.Net.Http.HttpRequestMessageExtensions.GetQueryNameValuePairs(request);
        //    if (queryNameValuePairs == null)
        //        return (string)null;
        //    KeyValuePair<string, string> keyValuePair = Enumerable.FirstOrDefault<KeyValuePair<string, string>>(queryNameValuePairs, (Func<KeyValuePair<string, string>, bool>)(keyValue => string.Compare(keyValue.Key, key, true) == 0));
        //    if (string.IsNullOrEmpty(keyValuePair.Value))
        //        return (string)null;
        //    return keyValuePair.Value;
        //}

        private string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => String.Compare(keyValue.Key, key, StringComparison.OrdinalIgnoreCase) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }
        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            string verifyTokenEndPoint;

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
                var appToken = ConfigurationManager.AppSettings["OAuth.Facebook.AppToken"];
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.ProviderKey = jObj["data"]["user_id"];
                    parsedToken.AppId = jObj["data"]["app_id"];

                    if (!string.Equals(Startup.facebookAuthOptions.AppId, parsedToken.AppId, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.ProviderKey = jObj["user_id"];
                    parsedToken.AppId = jObj["audience"];

                    if (!string.Equals(Startup.googleAuthOptions.ClientId, parsedToken.AppId, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                }

            }

            return parsedToken;
        }

        private JObject GenerateLocalAccessTokenResponseBackup(ApplicationUser user)
        {
            TimeSpan timeSpan = TimeSpan.FromDays(1.0);
            var identity = new ClaimsIdentity("Bearer");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.UserName));
            var properties = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(timeSpan)
            };
            var data = new AuthenticationTicket(identity, properties);
            string str = Startup.OAuthOptions.AccessTokenFormat.Protect(data);
            return new JObject(new object[]
                               {
                                   new JProperty("userName", user.UserName),
                                   new JProperty("roles", string.Empty),
                                   new JProperty("access_token", str),
                                   new JProperty("token_type", "bearer"),
                                   new JProperty("expires_in", timeSpan.TotalSeconds.ToString()),
                                   new JProperty(".issued", data.Properties.IssuedUtc.ToString()),
                                   new JProperty(".expires", data.Properties.ExpiresUtc.ToString())
                               });
        }


        private async Task<JObject> GenerateLocalAccessTokenResponse(ApplicationUser user)
        {
            TimeSpan tokenExpiration = TimeSpan.FromDays(1.0);
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var identity = await user.GenerateUserIdentityAsync(userManager, "Bearer");
            var clientInfo = AppAuthorizeAttribute.GenerateClientInfo(user.UserName, tokenExpiration);
            string userClientId = clientInfo.Key;
            var userClientClaims = clientInfo.Value;
            identity.AddClaims(userClientClaims);
            IList<string> roles = await userManager.GetRolesAsync(user.Id);
            var data = (IDictionary<string, string>)new Dictionary<string, string>();
            data.Add("userName", user.UserName);
            data.Add("userClientId", userClientId);
            data.Add("roles", string.Join(",", roles));
            var props = new AuthenticationProperties(data)
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration)
            };
            IList<Claim> claims = await userManager.GetClaimsAsync(user.Id);
            var displayName = claims.FirstOrDefault(m => m.Type == "displayName");
            var ticket = new AuthenticationTicket(identity, props);
            string accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            var tokenResponse = new JObject(new[]
                                            {
                                                new JProperty("userName", user.UserName),
                                                new JProperty("displayName",displayName != null ? displayName.Value : user.UserName),
                                                new JProperty("roles", string.Join(",", roles)),
                                                new JProperty("access_token", accessToken),
                                                new JProperty("token_type", "bearer"),
                                                new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture)),
                                                new JProperty("userClientId", userClientId),
                                                new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                                                (object)new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
                                            });
            return tokenResponse;
        }
        #region Helpers



        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();
            if (result.Succeeded)
                return null;
            if (result.Errors != null)
            {
                foreach (string errorMessage in result.Errors)
                    ModelState.AddModelError("", errorMessage);
            }
            if (ModelState.IsValid)
                return BadRequest();
            return BadRequest(ModelState);
        }
        private class ExternalLoginData
        {
            public string LoginProvider { get; private set; }

            public string ProviderKey { get; private set; }

            public string UserName { get; private set; }

            public string ExternalAccessToken { get; private set; }

            public IList<Claim> GetClaims()
            {
                var list = (IList<Claim>)new List<Claim>();
                list.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", ProviderKey, null, LoginProvider));
                if (UserName != null)
                    list.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", UserName, null, LoginProvider));
                return list;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                    return null;
                var first = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (first == null || string.IsNullOrEmpty(first.Issuer) || string.IsNullOrEmpty(first.Value))
                    return null;
                if (first.Issuer == "LOCAL AUTHORITY")
                    return null;
                return new ExternalLoginData
                       {
                    LoginProvider = first.Issuer,
                    ProviderKey = first.Value,
                    UserName = identity.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"),
                    ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken")
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                if (strengthInBits % 8 != 0)
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                var numArray = new byte[strengthInBits / 8];
                _random.GetBytes(numArray);
                return HttpServerUtility.UrlTokenEncode(numArray);
            }
        }

        #endregion
    }
}
