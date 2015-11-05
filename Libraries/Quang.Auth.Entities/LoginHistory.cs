using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class LoginHistory
    {
        public long Id { get; set; }

        public int Type { get; set; }

        public string UserName { get; set; }

        public DateTime LoginTime { get; set; }

        public int LoginStatus { get; set; }

        public string RefreshToken { get; set; }

        public string AppId { get; set; }

        public string ClientUri { get; set; }

        public string ClientIP { get; set; }

        public string ClientUA { get; set; }

        public string ClientDevice { get; set; }

        public string ClientApiKey { get; set; }

        public DateTime Created { get; set; }
    }
}
