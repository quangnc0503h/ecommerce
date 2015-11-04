using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Common.Auth
{
    public class AuthorizationClientInfo
    {
        public string UserName { get; set; }

        public string ClientCode { get; set; }

        public AuthorizationClientInfo()
          : this((string)null, (string)null)
        {
        }

        public AuthorizationClientInfo(string userName, string clientCode)
        {
            this.UserName = userName;
            this.ClientCode = clientCode;
        }
    }
}
