using Quang.RedisCache;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
namespace Quang.Common.Auth
{
    public class WebAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly ICacheClient _redisCache = (ICacheClient)new RedisCacheClient(RedisConnection.SecurityConn, (ISerializer)null, 0);

        public bool IsDevice { get; set; }

        public string DeviceGroup { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (base.AuthorizeCore(httpContext))
            {
                ClaimsIdentity identity = httpContext.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    Claim first1 = identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                    Claim first2 = identity.FindFirst("Dsvn:ClientId");
                    Claim first3 = identity.FindFirst("Dsvn:ClientCode");
                    if (first1 != null && first2 != null && first3 != null)
                    {
                        string key = string.Format("AuthTokenClientId.{0}", (object)first2.Value);
                        AuthorizationClientInfo authorizationClientInfo = WebAuthorizeAttribute._redisCache.Get<AuthorizationClientInfo>(key);
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
    }
}
