using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.RedisCache
{
    public static class RedisConnection
    {
        //public static readonly string DanhMucConn = ConfigurationManager.AppSettings.Get("DanhMucConn");
        //public static readonly string KeHoachConn = ConfigurationManager.AppSettings.Get("KeHoachConn");
        public static readonly string SecurityConn = ConfigurationManager.AppSettings.Get("SecurityConn");
        //public static readonly string BanVeConn = ConfigurationManager.AppSettings.Get("BanVeConn");
    }
}
