using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api
{
    public class Helper
    {
        public static string GetHash(string input)
        {
            return Convert.ToBase64String(new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)));
        }
    }
}