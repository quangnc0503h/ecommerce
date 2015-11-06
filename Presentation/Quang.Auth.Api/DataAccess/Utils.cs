using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class Utils
    {
        public static string EncodeForLike(string param)
        {
            return param.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
        }
    }
}