using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class CaptchaOutput
    {
        public bool Status;
        public string Msg;
        public string AccessToken;
        public string ClientId;
    }
}