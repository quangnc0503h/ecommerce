using Quang.RedisCache;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Quang.Common.Auth
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly ICacheClient _redisCache = (ICacheClient)new RedisCacheClient(RedisConnection.SecurityConn, (ISerializer)null, 0);
        public const string ClaimTypeDeviceKey = "Dsvn:DeviceKey";
        public const string ClaimTypeDeviceName = "Dsvn:DeviceName";
        public const string ClaimTypeDeviceGroups = "Dsvn:DeviceGroups";
        internal const string ClaimTypeClientId = "Dsvn:ClientId";
        internal const string ClaimTypeClientCode = "Dsvn:ClientCode";
        internal const string UserClientCodeFormatValue = "dsvn_v2_2015_{0}_{1}";
        internal const string CurrentTokenClientIdCacheKey = "AuthTokenClientId.{0}";

        public bool IsDevice { get; set; }

        public string DeviceGroup { get; set; }

        public static KeyValuePair<string, IList<Claim>> GenerateClientInfo(string userName, TimeSpan expiresIn)
        {
            string key1 = AppAuthorizeAttribute.GenerateClientId();
            string clientCode = SecurityUtils.MD5Hash(string.Format("dsvn_v2_2015_{0}_{1}", (object)"0.0.0.0", (object)HttpContext.Current.Request.UserAgent));
            Log.Debug("ua:" + clientCode + ":" + HttpContext.Current.Request.UserAgent);
            AuthorizationClientInfo authorizationClientInfo = new AuthorizationClientInfo(userName, clientCode);
            IList<Claim> list = (IList<Claim>)new List<Claim>();
            list.Add(new Claim("Dsvn:ClientId", key1));
            list.Add(new Claim("Dsvn:ClientCode", clientCode));
            string key2 = string.Format("AuthTokenClientId.{0}", (object)key1);
            AppAuthorizeAttribute._redisCache.Add<AuthorizationClientInfo>(key2, authorizationClientInfo, expiresIn);
            return new KeyValuePair<string, IList<Claim>>(key1, list);
        }

        public static void UpdateClientInfoCacheTime(ClaimsIdentity identity, TimeSpan newExpiresIn)
        {
            if (identity == null || identity.Claims == null)
                return;
            Claim claim = Enumerable.SingleOrDefault<Claim>(identity.Claims, (Func<Claim, bool>)(m => m.Type == "Dsvn:ClientId"));
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return;
            string key = string.Format("AuthTokenClientId.{0}", (object)claim.Value);
            AuthorizationClientInfo authorizationClientInfo = AppAuthorizeAttribute._redisCache.Get<AuthorizationClientInfo>(key);
            if (authorizationClientInfo == null)
                return;
            AppAuthorizeAttribute._redisCache.Remove(key);
            AppAuthorizeAttribute._redisCache.Add<AuthorizationClientInfo>(key, authorizationClientInfo, newExpiresIn);
        }

        public static IEnumerable<Claim> GetDeviceInfoClaims(string deviceKey, string deviceName, IEnumerable<string> deviceGroupKeys)
        {
            string str = deviceGroupKeys != null ? string.Join(",", Enumerable.ToArray<string>(deviceGroupKeys)) : string.Empty;
            IList<Claim> list = (IList<Claim>)new List<Claim>();
            list.Add(new Claim("Dsvn:DeviceKey", deviceKey));
            list.Add(new Claim("Dsvn:DeviceName", deviceName));
            list.Add(new Claim("Dsvn:DeviceGroups", str));
            return (IEnumerable<Claim>)list;
        }

        public static bool ValidateOAuthAuthorizationHeader(string username, string headerValue, out string realAccessToken)
        {
            realAccessToken = (string)null;
            if (!string.IsNullOrEmpty(headerValue))
            {
                string key = string.Format("AuthTokenClientId.{0}", (object)headerValue.Substring(0, 34));
                AuthorizationClientInfo authorizationClientInfo = AppAuthorizeAttribute._redisCache.Get<AuthorizationClientInfo>(key);
                string str = SecurityUtils.MD5Hash(string.Format("dsvn_v2_2015_{0}_{1}", (object)"0.0.0.0", (object)HttpContext.Current.Request.UserAgent));
                Log.Debug("ua:" + str + ":" + HttpContext.Current.Request.UserAgent);
                if (authorizationClientInfo != null && authorizationClientInfo.UserName == username && authorizationClientInfo.ClientCode == str)
                {
                    realAccessToken = headerValue.Substring(34);
                    return true;
                }
            }
            return false;
        }

        public static void ClearAuthClientInfo(ClaimsIdentity identity)
        {
            if (identity == null)
                identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            if (identity == null)
                return;
            Claim first = identity.FindFirst("Dsvn:ClientId");
            if (first == null)
                return;
            string key = string.Format("AuthTokenClientId.{0}", (object)first.Value);
            AppAuthorizeAttribute._redisCache.Remove(key);
        }

        private static string GenerateClientId()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder stringBuilder = new StringBuilder();
            string str = Guid.NewGuid().ToString("N");
            int num1 = random.Next(0, str.Length - 1);
            int num2 = random.Next(0, str.Length - 1);
            for (int index = 0; index < str.Length; ++index)
            {
                if (random.Next(0, 2) > 0)
                    stringBuilder.Append(str[index].ToString().ToUpper());
                else
                    stringBuilder.Append(str[index].ToString());
                if (index == num1 || index == num2)
                    stringBuilder.Append("_");
            }
            return stringBuilder.ToString();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (base.IsAuthorized(actionContext))
            {
                ClaimsIdentity identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    Claim first1 = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    Claim first2 = identity.FindFirst("Dsvn:ClientId");
                    Claim first3 = identity.FindFirst("Dsvn:ClientCode");
                    if (first1 != null && first2 != null && first3 != null)
                    {
                        string key = string.Format("AuthTokenClientId.{0}", (object)first2.Value);
                        AuthorizationClientInfo authorizationClientInfo = AppAuthorizeAttribute._redisCache.Get<AuthorizationClientInfo>(key);
                        if (authorizationClientInfo != null && authorizationClientInfo.UserName == first1.Value && authorizationClientInfo.ClientCode == first3.Value)
                        {
                            if (this.IsDevice || !string.IsNullOrEmpty(this.DeviceGroup))
                                return this.isDeviceAuthorized(identity);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool isDeviceAuthorized(ClaimsIdentity identity)
        {
            Claim first1 = identity.FindFirst("Dsvn:DeviceKey");
            if (first1 != null && !string.IsNullOrEmpty(first1.Value))
            {
                if (string.IsNullOrEmpty(this.DeviceGroup))
                    return true;
                Claim first2 = identity.FindFirst("Dsvn:DeviceGroups");
                if (first2 != null && !string.IsNullOrEmpty(first2.Value))
                    return Enumerable.Contains<string>((IEnumerable<string>)first2.Value.Split(','), this.DeviceGroup);
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            if (string.IsNullOrEmpty(this.Roles))
                return;
            ObjectContent<HttpError> objectContent = actionContext.Response.Content as ObjectContent<HttpError>;
            if (objectContent == null)
                return;
            HttpError httpError1 = objectContent.Value as HttpError;
            if (httpError1 == null || !string.IsNullOrEmpty(httpError1.MessageDetail))
                return;
            HttpError httpError2 = new HttpError(httpError1.Message);
            if (actionContext.ControllerContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                IDictionary<string, ActionRoleItem> dictionary = ActionRole.ToListDictionary();
                string str = this.Roles;
                if (dictionary.ContainsKey(this.Roles))
                    str = dictionary[this.Roles].RoleKeyLabel;
                httpError2.MessageDetail = string.Format("You must have permission in [{0}] to access this page.\n\nPls contact admin to support.", (object)str);
                actionContext.Response.Content = (HttpContent)new ObjectContent<HttpError>(httpError2, objectContent.Formatter);
            }
            else
            {
                httpError2.MessageDetail = "You need login to access this page.";
                actionContext.Response.Content = (HttpContent)new ObjectContent<HttpError>(httpError2, objectContent.Formatter);
            }
        }
    }
}
