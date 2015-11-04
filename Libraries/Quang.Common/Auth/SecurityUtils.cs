using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Common.Auth
{
    public static class SecurityUtils
    {
        public static string MD5Hash(string data)
        {
            byte[] hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("X2"));
            return stringBuilder.ToString();
        }

        public static string GetClientIPAddress()
        {
            string str1 = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string str2;
            if (string.IsNullOrEmpty(str1))
                str2 = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            else
                str2 = str1.Split(',')[0];
            Log.Debug("ip:" + str2);
            return str2;
        }

        public static string GetClientHostName()
        {
            return HttpContext.Current.Request.UserHostName;
        }

        public static string GetConnectDeviceKey(this IPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                Claim first = claimsIdentity.FindFirst("Dsvn:DeviceKey");
                if (first != null)
                    return first.Value;
            }
            return (string)null;
        }

        public static string GetConnectDeviceName(this IPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                Claim first = claimsIdentity.FindFirst("Dsvn:DeviceName");
                if (first != null)
                    return first.Value;
            }
            return (string)null;
        }

        public static string[] GetConnectDeviceGroups(this IPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                Claim first = claimsIdentity.FindFirst("Dsvn:DeviceGroups");
                if (first != null && !string.IsNullOrEmpty(first.Value))
                    return first.Value.Split(',');
            }
            return new string[0];
        }
    }
}
