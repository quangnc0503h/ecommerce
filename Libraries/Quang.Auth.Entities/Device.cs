using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class Device
    {
        public long Id { get; set; }

        public long RequestDeviceId { get; set; }

        public bool IsActived { get; set; }

        public string DeviceKey { get; set; }

        public string DeviceSecret { get; set; }

        public string DeviceName { get; set; }

        public string DeviceDescription { get; set; }

        public string SerialNumber { get; set; }

        public string IMEI { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
}
