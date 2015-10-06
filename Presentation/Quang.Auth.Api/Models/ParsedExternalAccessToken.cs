using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class ParsedExternalAccessToken
    {
        public string ProviderKey { get; set; }
        public string AppId { get; set; }
    }
}