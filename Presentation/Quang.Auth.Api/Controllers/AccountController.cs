using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
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
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private IUserBll _userBll;
        private ITermBll _termBll;
        private IGroupBll _groupBll;
        private IPermissionBll _permissionBll;
        private ILoginHistoryBll _loginHistoryBll;

        public ApplicationUserManager UserManager
        {
            get
            {
                return this._userManager ?? OwinContextExtensions.GetUserManager<ApplicationUserManager>(OwinHttpRequestMessageExtensions.GetOwinContext(this.Request));
            }
            private set
            {
                this._userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        private IAuthenticationManager Authentication
        {
            get
            {
                return OwinHttpRequestMessageExtensions.GetOwinContext(this.Request).Authentication;
            }
        }

        public AccountController()
        {
        }

        public AccountController(IUserBll userBll, ITermBll termBll, IGroupBll groupBll, IPermissionBll permissionBll, ILoginHistoryBll loginHistoryBll)
        {
            this._userBll = userBll;
            this._groupBll = groupBll;
            this._permissionBll = permissionBll;
            this._termBll = termBll;
            this._loginHistoryBll = loginHistoryBll;
        }

        [HostAuthentication("ExternalBearer")]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            AccountController.ExternalLoginData externalLoginData = AccountController.ExternalLoginData.FromIdentity(this.User.Identity as ClaimsIdentity);
            return new UserInfoViewModel()
            {
                Email = IdentityExtensions.GetUserName(this.User.Identity),
                HasRegistered = externalLoginData == null,
                LoginProvider = externalLoginData != null ? externalLoginData.LoginProvider : (string)null
            };
        }

        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            ClaimsIdentity identity = this.User.Identity as ClaimsIdentity;
            this.Authentication.SignOut("Cookies");
            AppAuthorizeAttribute.ClearAuthClientInfo(identity);
            return (IHttpActionResult)this.Ok();
        }

        [Route("Logout4Mobile")]
        public LogoutMobileOutput Logout4Mobile()
        {
            LogoutMobileOutput logoutMobileOutput = new LogoutMobileOutput()
            {
                Status = 0
            };
            try
            {
                ClaimsIdentity identity = this.User.Identity as ClaimsIdentity;
                this.Authentication.SignOut("Cookies");
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
            IdentityUser user = (IdentityUser)await this.UserManager.FindByIdAsync(IdentityExtensions.GetUserId<int>(this.User.Identity));
            ManageInfoViewModel manageInfoViewModel;
            if (user == null)
            {
                manageInfoViewModel = (ManageInfoViewModel)null;
            }
            else
            {
                List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();
                if (user.PasswordHash != null)
                    logins.Add(new UserLoginInfoViewModel()
                    {
                        LoginProvider = "Local",
                        ProviderKey = user.UserName
                    });
                manageInfoViewModel = new ManageInfoViewModel()
                {
                    LocalLoginProvider = "Local",
                    Email = user.UserName,
                    Logins = (IEnumerable<UserLoginInfoViewModel>)logins,
                    ExternalLoginProviders = this.GetExternalLogins(returnUrl, generateState)
                };
            }
            return manageInfoViewModel;
        }

        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                IdentityResult result = await this.UserManager.ChangePasswordAsync(IdentityExtensions.GetUserId<int>(this.User.Identity), model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    await this.LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                    httpActionResult = this.GetErrorResult(result);
                }
                else
                {
                    await this.LogHistory(LoginType.ChangePassword, LoginStatus.Success, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                    httpActionResult = (IHttpActionResult)this.Ok();
                }
            }
            return httpActionResult;
        }

        [Route("ChangePassword4Mobile")]
        public async Task<ChangePasswordMobileOutput> ChangePassword4Mobile(ChangePasswordBindingModel model)
        {
            ChangePasswordMobileOutput output = new ChangePasswordMobileOutput()
            {
                Status = 0
            };
            ChangePasswordMobileOutput passwordMobileOutput;
            if (!this.ModelState.IsValid)
            {
                output.Status = 1;
                output.ErrorMsg = "Invalid input";
                await this.LogHistory(LoginType.ChangePassword, LoginStatus.BadRequest, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                passwordMobileOutput = output;
            }
            else
            {
                IdentityResult result = await this.UserManager.ChangePasswordAsync(IdentityExtensions.GetUserId<int>(this.User.Identity), model.OldPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    output.Status = 1;
                    output.ErrorMsg = result.Errors == null || Enumerable.Count<string>(result.Errors) <= 0 ? "Unknown errors" : Enumerable.First<string>(result.Errors);
                    await this.LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                }
                else
                    await this.LogHistory(LoginType.ChangePassword, LoginStatus.InvalidOldPassword, SecurityUtils.GetConnectAppId(this.User), IdentityExtensions.GetUserName(this.User.Identity), SecurityUtils.GetConnectDeviceKey(this.User), (string)null);
                passwordMobileOutput = output;
            }
            return passwordMobileOutput;
        }

        [Route("GetMobileProfile")]
        public async Task<GetMobileProfileOutput> GetMobileProfile()
        {
            GetMobileProfileOutput output = await this._userBll.GetMobileProfile(IdentityExtensions.GetUserId<int>(this.User.Identity));
            return output;
        }

        [Route("UpdateMobileProfile")]
        [HttpPost]
        public async Task<UpdateMobileProfileOutput> UpdateMobileProfile(UpdateMobileProfileInput input)
        {
            UpdateMobileProfileOutput output = await this._userBll.UpdateMobileProfile(IdentityExtensions.GetUserId<int>(this.User.Identity), input);
            return output;
        }

        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                ApplicationUser user = await this.UserManager.FindByIdAsync(IdentityExtensions.GetUserId<int>(this.User.Identity));
                if (user == null)
                    httpActionResult = (IHttpActionResult)this.BadRequest("Not found user");
                else if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    httpActionResult = (IHttpActionResult)this.BadRequest("Not allow create password");
                }
                else
                {
                    IdentityResult result = await this.UserManager.AddPasswordAsync(IdentityExtensions.GetUserId<int>(this.User.Identity), model.NewPassword);
                    httpActionResult = result.Succeeded ? (IHttpActionResult)this.Ok() : this.GetErrorResult(result);
                }
            }
            return httpActionResult;
        }

        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                this.Authentication.SignOut("ExternalCookie");
                AuthenticationTicket ticket = this.AccessTokenFormat.Unprotect(model.ExternalAccessToken);
                if (ticket == null || ticket.Identity == null || ticket.Properties != null && ticket.Properties.ExpiresUtc.HasValue && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow)
                {
                    httpActionResult = (IHttpActionResult)this.BadRequest("External login failure.");
                }
                else
                {
                    AccountController.ExternalLoginData externalData = AccountController.ExternalLoginData.FromIdentity(ticket.Identity);
                    if (externalData == null)
                    {
                        httpActionResult = (IHttpActionResult)this.BadRequest("The external login is already associated with an account.");
                    }
                    else
                    {
                        IdentityResult result = await this.UserManager.AddLoginAsync(IdentityExtensions.GetUserId<int>(this.User.Identity), new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));
                        httpActionResult = result.Succeeded ? (IHttpActionResult)this.Ok() : this.GetErrorResult(result);
                    }
                }
            }
            return httpActionResult;
        }

        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                IdentityResult result;
                if (model.LoginProvider == "Local")
                    result = await this.UserManager.RemovePasswordAsync(IdentityExtensions.GetUserId<int>(this.User.Identity));
                else
                    result = await this.UserManager.RemoveLoginAsync(IdentityExtensions.GetUserId<int>(this.User.Identity), new UserLoginInfo(model.LoginProvider, model.ProviderKey));
                httpActionResult = result.Succeeded ? (IHttpActionResult)this.Ok() : this.GetErrorResult(result);
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
                httpActionResult = (IHttpActionResult)this.BadRequest(Uri.EscapeDataString(error));
            else if (!this.User.Identity.IsAuthenticated)
            {
                httpActionResult = (IHttpActionResult)new ChallengeResult(provider, (ApiController)this);
            }
            else
            {
                string redirectUriValidationResult = this.ValidateClientAndRedirectUri(this.Request, ref redirectUri);
                if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
                {
                    httpActionResult = (IHttpActionResult)this.BadRequest(redirectUriValidationResult);
                }
                else
                {
                    AccountController.ExternalLoginData externalLogin = AccountController.ExternalLoginData.FromIdentity(this.User.Identity as ClaimsIdentity);
                    if (externalLogin == null)
                        httpActionResult = (IHttpActionResult)this.InternalServerError();
                    else if (externalLogin.LoginProvider != provider)
                    {
                        this.Authentication.SignOut("ExternalCookie");
                        httpActionResult = (IHttpActionResult)new ChallengeResult(provider, (ApiController)this);
                    }
                    else
                    {
                        string email = string.Empty;
                        ClaimsIdentity externalIdentity = await AuthenticationManagerExtensions.GetExternalIdentityAsync(HttpContextExtensions.GetOwinContext(HttpContext.Current).Authentication, "ExternalCookie");
                        if (externalIdentity != null)
                        {
                            Claim claim = Enumerable.FirstOrDefault<Claim>(externalIdentity.Claims, (Func<Claim, bool>)(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"));
                            if (claim != null)
                                email = claim.Value;
                        }
                        ApplicationUser user = await this.UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
                        bool hasRegistered = user != null;
                        redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&external_email={5}", (object)redirectUri, (object)externalLogin.ExternalAccessToken, (object)externalLogin.LoginProvider, (object)hasRegistered.ToString(), (object)externalLogin.UserName, (object)email);
                        httpActionResult = (IHttpActionResult)this.Redirect(redirectUri);
                    }
                }
            }
            return httpActionResult;
        }

        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> authenticationTypes = AuthenticationManagerExtensions.GetExternalAuthenticationTypes(this.Authentication);
            List<ExternalLoginViewModel> list = new List<ExternalLoginViewModel>();
            string str = !generateState ? (string)null : AccountController.RandomOAuthStateGenerator.Generate(256);
            foreach (AuthenticationDescription authenticationDescription in authenticationTypes)
            {
                ExternalLoginViewModel externalLoginViewModel = new ExternalLoginViewModel()
                {
                    Name = authenticationDescription.Caption,
                    Url = this.Url.Route("ExternalLogin", (object)new
                    {
                        provider = authenticationDescription.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(this.Request.RequestUri, returnUrl).AbsoluteUri,
                        state = str
                    }),
                    State = str
                };
                list.Add(externalLoginViewModel);
            }
            return (IEnumerable<ExternalLoginViewModel>)list;
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.UserName = model.Email;
                applicationUser.Email = model.Email;
                ApplicationUser user = applicationUser;
                IdentityResult result = await this.UserManager.CreateAsync(user, model.Password);
                httpActionResult = result.Succeeded ? (IHttpActionResult)this.Ok() : this.GetErrorResult(result);
            }
            return httpActionResult;
        }

        [AllowAnonymous]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            IHttpActionResult httpActionResult;
            if (!this.ModelState.IsValid)
            {
                httpActionResult = (IHttpActionResult)this.BadRequest(this.ModelState);
            }
            else
            {
                ParsedExternalAccessToken verifiedAccessToken = await this.VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
                if (verifiedAccessToken == null)
                {
                    httpActionResult = (IHttpActionResult)this.BadRequest("Invalid Provider or External Access Token");
                }
                else
                {
                    AccountController.ExternalLoginData.FromIdentity(this.User.Identity as ClaimsIdentity);
                    ApplicationUser user = await this.UserManager.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.ProviderKey));
                    bool hasRegistered = user != null;
                    if (hasRegistered)
                    {
                        httpActionResult = (IHttpActionResult)this.BadRequest("External user is already registered");
                    }
                    else
                    {
                        ApplicationUser applicationUser = new ApplicationUser();
                        applicationUser.UserName = model.Provider.ToLower() + verifiedAccessToken.ProviderKey.ToLower();
                        applicationUser.Email = model.Email;
                        user = applicationUser;
                        IdentityResult result = await this.UserManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            httpActionResult = this.GetErrorResult(result);
                        }
                        else
                        {
                            ExternalLoginInfo info = new ExternalLoginInfo()
                            {
                                DefaultUserName = user.UserName,
                                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.ProviderKey)
                            };
                            result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
                            if (!result.Succeeded)
                            {
                                httpActionResult = this.GetErrorResult(result);
                            }
                            else
                            {
                                result = await this.UserManager.AddClaimAsync(user.Id, new Claim("displayName", model.UserName));
                                JObject accessTokenResponse = await this.GenerateLocalAccessTokenResponse(user);
                                httpActionResult = (IHttpActionResult)this.Ok<JObject>(accessTokenResponse);
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
                httpActionResult = (IHttpActionResult)this.BadRequest("Provider or external access token is not sent");
            }
            else
            {
                ParsedExternalAccessToken verifiedAccessToken = await this.VerifyExternalAccessToken(provider, externalAccessToken);
                if (verifiedAccessToken == null)
                {
                    httpActionResult = (IHttpActionResult)this.BadRequest("Invalid Provider or External Access Token");
                }
                else
                {
                    ApplicationUser user = await this.UserManager.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.ProviderKey));
                    bool hasRegistered = user != null;
                    if (!hasRegistered)
                    {
                        httpActionResult = (IHttpActionResult)this.BadRequest("External user is not registered");
                    }
                    else
                    {
                        JObject accessTokenResponse = await this.GenerateLocalAccessTokenResponse(user);
                        httpActionResult = (IHttpActionResult)this.Ok<JObject>(accessTokenResponse);
                    }
                }
            }
            return httpActionResult;
        }
        private async Task LogHistory(LoginType loginType, LoginStatus status, string clientId, string username, string device, string apiKey)
        {
            string clientIp =SecurityUtils.GetClientIPAddress();
            string clientUri = HttpContext.Current.Request.Url.AbsoluteUri;
            int num = await this._loginHistoryBll.InsertLoginHistory(new InsertLoginHistoryInput()
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

        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {
            string queryString1 = this.GetQueryString(this.Request, "redirect_uri");
            if (string.IsNullOrWhiteSpace(queryString1))
                return "redirect_uri is required";
            Uri result;
            if (!Uri.TryCreate(queryString1, UriKind.Absolute, out result))
                return "redirect_uri is invalid";
            string queryString2 = this.GetQueryString(this.Request, "client_id");
            if (string.IsNullOrWhiteSpace(queryString2))
                return "client_Id is required";
            if (queryString2 != "ngAuthApp")
                return string.Format("Client_id '{0}' is not registered in the system.", (object)queryString2);
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

            var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }
        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

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

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

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
            ClaimsIdentity identity = new ClaimsIdentity("Bearer");
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.UserName));
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                IssuedUtc = new DateTimeOffset?((DateTimeOffset)DateTime.UtcNow),
                ExpiresUtc = new DateTimeOffset?((DateTimeOffset)DateTime.UtcNow.Add(timeSpan))
            };
            AuthenticationTicket data = new AuthenticationTicket(identity, properties);
            string str = Startup.OAuthOptions.AccessTokenFormat.Protect(data);
            return new JObject(new object[7]
            {
        (object) new JProperty("userName", (object) user.UserName),
        (object) new JProperty("roles", (object) string.Empty),
        (object) new JProperty("access_token", (object) str),
        (object) new JProperty("token_type", (object) "bearer"),
        (object) new JProperty("expires_in", (object) timeSpan.TotalSeconds.ToString()),
        (object) new JProperty(".issued", (object) data.Properties.IssuedUtc.ToString()),
        (object) new JProperty(".expires", (object) data.Properties.ExpiresUtc.ToString())
            });
        }


        private async Task<JObject> GenerateLocalAccessTokenResponse(ApplicationUser user)
        {
            TimeSpan tokenExpiration = TimeSpan.FromDays(1.0);
            ApplicationUserManager userManager = OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current));
            ClaimsIdentity identity = await user.GenerateUserIdentityAsync((Microsoft.AspNet.Identity.UserManager<ApplicationUser, int>)userManager, "Bearer");
            KeyValuePair<string, IList<Claim>> clientInfo = AppAuthorizeAttribute.GenerateClientInfo(user.UserName, tokenExpiration);
            string userClientId = clientInfo.Key;
            IList<Claim> userClientClaims = clientInfo.Value;
            identity.AddClaims((IEnumerable<Claim>)userClientClaims);
            IList<string> roles = await userManager.GetRolesAsync(user.Id);
            IDictionary<string, string> data = (IDictionary<string, string>)new Dictionary<string, string>();
            data.Add("userName", user.UserName);
            data.Add("userClientId", userClientId);
            data.Add("roles", string.Join(",", (IEnumerable<string>)roles));
            AuthenticationProperties props = new AuthenticationProperties(data)
            {
                IssuedUtc = new DateTimeOffset?((DateTimeOffset)DateTime.UtcNow),
                ExpiresUtc = new DateTimeOffset?((DateTimeOffset)DateTime.UtcNow.Add(tokenExpiration))
            };
            IList<Claim> claims = await userManager.GetClaimsAsync(user.Id);
            Claim displayName = Enumerable.FirstOrDefault<Claim>((IEnumerable<Claim>)claims, (Func<Claim, bool>)(m => m.Type == "displayName"));
            AuthenticationTicket ticket = new AuthenticationTicket(identity, props);
            string accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            JObject tokenResponse = new JObject(new object[9]
            {
        (object) new JProperty("userName", (object) user.UserName),
        (object) new JProperty("displayName", displayName != null ? (object) displayName.Value : (object) user.UserName),
        (object) new JProperty("roles", (object) string.Join(",", (IEnumerable<string>) roles)),
        (object) new JProperty("access_token", (object) accessToken),
        (object) new JProperty("token_type", (object) "bearer"),
        (object) new JProperty("expires_in", (object) tokenExpiration.TotalSeconds.ToString()),
        (object) new JProperty("userClientId", (object) userClientId),
        (object) new JProperty(".issued", (object) ticket.Properties.IssuedUtc.ToString()),
        (object) new JProperty(".expires", (object) ticket.Properties.ExpiresUtc.ToString())
            });
            return tokenResponse;
        }
        #region Helpers



        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return (IHttpActionResult)this.InternalServerError();
            if (result.Succeeded)
                return (IHttpActionResult)null;
            if (result.Errors != null)
            {
                foreach (string errorMessage in result.Errors)
                    this.ModelState.AddModelError("", errorMessage);
            }
            if (this.ModelState.IsValid)
                return (IHttpActionResult)this.BadRequest();
            return (IHttpActionResult)this.BadRequest(this.ModelState);
        }
        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }

            public string ProviderKey { get; set; }

            public string UserName { get; set; }

            public string ExternalAccessToken { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> list = (IList<Claim>)new List<Claim>();
                list.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", this.ProviderKey, (string)null, this.LoginProvider));
                if (this.UserName != null)
                    list.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", this.UserName, (string)null, this.LoginProvider));
                return list;
            }

            public static AccountController.ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                    return (AccountController.ExternalLoginData)null;
                Claim first = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                if (first == null || string.IsNullOrEmpty(first.Issuer) || string.IsNullOrEmpty(first.Value))
                    return (AccountController.ExternalLoginData)null;
                if (first.Issuer == "LOCAL AUTHORITY")
                    return (AccountController.ExternalLoginData)null;
                return new AccountController.ExternalLoginData()
                {
                    LoginProvider = first.Issuer,
                    ProviderKey = first.Value,
                    UserName = IdentityExtensions.FindFirstValue(identity, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"),
                    ExternalAccessToken = IdentityExtensions.FindFirstValue(identity, "ExternalAccessToken")
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = (RandomNumberGenerator)new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                if (strengthInBits % 8 != 0)
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                byte[] numArray = new byte[strengthInBits / 8];
                AccountController.RandomOAuthStateGenerator._random.GetBytes(numArray);
                return HttpServerUtility.UrlTokenEncode(numArray);
            }
        }

        #endregion
    }
}
