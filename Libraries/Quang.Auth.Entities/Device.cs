using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class Device
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public int? RequestDeviceId { get; set; }

        public string RequestDeviceName
        {
            get
            {
                if (this.RequestDeviceId.HasValue)
                    return string.Format("R{0}", (object)this.RequestDeviceId.Value.ToString("D4"));
                return (string)null;
            }
        }

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
