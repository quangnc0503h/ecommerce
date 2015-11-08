using System.Globalization;
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
        private static readonly ICacheClient _redisCache = new RedisCacheClient(RedisConnection.SecurityConn);
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
            string key1 = GenerateClientId();
            string clientCode = SecurityUtils.MD5Hash(string.Format("dsvn_v2_2015_{0}_{1}", "0.0.0.0", HttpContext.Current.Request.UserAgent));
            //Log.Debug("ua:" + clientCode + ":" + HttpContext.Current.Request.UserAgent);
            var authorizationClientInfo = new AuthorizationClientInfo(userName, clientCode);
            IList<Claim> list = new List<Claim>();
            list.Add(new Claim("Dsvn:ClientId", key1));
            list.Add(new Claim("Dsvn:ClientCode", clientCode));
            string key2 = string.Format("AuthTokenClientId.{0}", key1);
            
            _redisCache.Add(key2, authorizationClientInfo, expiresIn);
            return new KeyValuePair<string, IList<Claim>>(key1, list);
        }

        public static void UpdateClientInfoCacheTime(ClaimsIdentity identity, TimeSpan newExpiresIn)
        {
            if (identity == null || identity.Claims == null)
                return;
            Claim claim = identity.Claims.SingleOrDefault(m => m.Type == "Dsvn:ClientId");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return;
            string key = string.Format("AuthTokenClientId.{0}", claim.Value);
            var authorizationClientInfo = _redisCache.Get<AuthorizationClientInfo>(key);
            if (authorizationClientInfo == null)
                return;
            _redisCache.Remove(key);
            _redisCache.Add(key, authorizationClientInfo, newExpiresIn);
        }

        public static IEnumerable<Claim> GetDeviceInfoClaims(string deviceKey, string deviceName, IEnumerable<string> deviceGroupKeys)
        {
            string str = deviceGroupKeys != null ? string.Join(",", deviceGroupKeys.ToArray()) : string.Empty;
            var list = (IList<Claim>)new List<Claim>();
            list.Add(new Claim("Dsvn:DeviceKey", deviceKey));
            list.Add(new Claim("Dsvn:DeviceName", deviceName));
            list.Add(new Claim("Dsvn:DeviceGroups", str));
            return list;
        }

        public static bool ValidateOAuthAuthorizationHeader(string username, string headerValue, out string realAccessToken)
        {
            realAccessToken = null;
            if (!string.IsNullOrEmpty(headerValue))
            {
                string key = string.Format("AuthTokenClientId.{0}", headerValue.Substring(0, 34));
                var authorizationClientInfo = _redisCache.Get<AuthorizationClientInfo>(key);
                string str = SecurityUtils.MD5Hash(string.Format("dsvn_v2_2015_{0}_{1}", "0.0.0.0", HttpContext.Current.Request.UserAgent));
                //Log.Debug("ua:" + str + ":" + HttpContext.Current.Request.UserAgent);
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
            _redisCache.Remove(key);
        }

        private static string GenerateClientId()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var stringBuilder = new StringBuilder();
            string str = Guid.NewGuid().ToString("N");
            int num1 = random.Next(0, str.Length - 1);
            int num2 = random.Next(0, str.Length - 1);
            for (int index = 0; index < str.Length; ++index)
            {
                stringBuilder.Append(random.Next(0, 2) > 0
                    ? str[index].ToString(CultureInfo.InvariantCulture).ToUpper()
                    : str[index].ToString(CultureInfo.InvariantCulture));
                if (index == num1 || index == num2)
                    stringBuilder.Append("_");
            }
            return stringBuilder.ToString();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (base.IsAuthorized(actionContext))
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var first1 = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    var first2 = identity.FindFirst("Dsvn:ClientId");
                    var first3 = identity.FindFirst("Dsvn:ClientCode");
                    if (first1 != null && first2 != null && first3 != null)
                    {
                        string key = string.Format("AuthTokenClientId.{0}", first2.Value);
                        var authorizationClientInfo = _redisCache.Get<AuthorizationClientInfo>(key);
                        if (authorizationClientInfo != null && authorizationClientInfo.UserName == first1.Value && authorizationClientInfo.ClientCode == first3.Value)
                        {
                            if (IsDevice || !string.IsNullOrEmpty(DeviceGroup))
                                return isDeviceAuthorized(identity);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool isDeviceAuthorized(ClaimsIdentity identity)
        {
            var first1 = identity.FindFirst("Dsvn:DeviceKey");
            if (first1 != null && !string.IsNullOrEmpty(first1.Value))
            {
                if (string.IsNullOrEmpty(DeviceGroup))
                    return true;
                Claim first2 = identity.FindFirst("Dsvn:DeviceGroups");
                if (first2 != null && !string.IsNullOrEmpty(first2.Value))
                    return first2.Value.Split(',').Contains(DeviceGroup);
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            if (string.IsNullOrEmpty(Roles))
                return;
            var objectContent = actionContext.Response.Content as ObjectContent<HttpError>;
            if (objectContent == null)
                return;
            var httpError1 = objectContent.Value as HttpError;
            if (httpError1 == null || !string.IsNullOrEmpty(httpError1.MessageDetail))
                return;
            var httpError2 = new HttpError(httpError1.Message);
            if (actionContext.ControllerContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                IDictionary<string, ActionRoleItem> dictionary = ActionRole.ToListDictionary();
                string str = Roles;
                if (dictionary.ContainsKey(Roles))
                    str = dictionary[Roles].RoleKeyLabel;
                httpError2.MessageDetail = string.Format("You must have permission in [{0}] to access this page.\n\nPls contact admin to support.", (object)str);
                actionContext.Response.Content = new ObjectContent<HttpError>(httpError2, objectContent.Formatter);
            }
            else
            {
                httpError2.MessageDetail = "You need login to access this page.";
                actionContext.Response.Content = new ObjectContent<HttpError>(httpError2, objectContent.Formatter);
            }
        }
    }
}
