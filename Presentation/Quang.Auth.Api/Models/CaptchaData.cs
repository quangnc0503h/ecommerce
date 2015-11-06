using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class CaptchaData
    {
        [Required]
        public string CaptchaResponse { get; set; }

        [Required]
        public string CaptchaChallenge { get; set; }

        [Required]
        [JsonProperty("c")]
        public string ClientId { get; set; }

        public string UserHostAddress { get; set; }
    }

}