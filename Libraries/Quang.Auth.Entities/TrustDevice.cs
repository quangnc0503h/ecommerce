using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public class TrustDevice
    {
        public long Id { get; set; }

        public string DeviceSerial { get; set; }

        public int DeviceGroup { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
