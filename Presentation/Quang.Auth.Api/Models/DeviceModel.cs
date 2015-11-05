using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class FilterDeviceModel
    {
        public string ClientId { get; set; }

        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }
    public class FilterRequestDeviceModel
    {
        public string ClientId { get; set; }

        public string Keyword { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }
    public class DeviceModel
    {
        public long Id { get; set; }

        public string ClientId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long? RequestDeviceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActived { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceSecret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IMEI { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Platform { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PlatformVersion { get; set; }
    }
}