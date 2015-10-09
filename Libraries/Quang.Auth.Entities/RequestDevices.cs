using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class RequestDevices
    {
        public long Id { get; set; }

        public bool IsApproved { get; set; }

        public string DeviceKey { get; set; }

        public string SerialNumber { get; set; }

        public string IMEI { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
