using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class GetOneDeviceByKeyModel
    {
        public int Id { get; set; }
        public string ClientId { get; set; }

        public string DeviceKey { get; set; }
    }
}