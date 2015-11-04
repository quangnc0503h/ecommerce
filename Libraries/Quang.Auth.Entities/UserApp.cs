using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Entities
{
    public class UserApp
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public bool IsActive { get; set; }

        public AppApiType ApiType { get; set; }

        public string ApiName { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public string AppHosts { get; set; }

        public string AppIps { get; set; }
    }

    public enum AppApiType
    {
        None = 0,
        ClientApi = 1
    }
    public enum ApplicationTypes
    {
        JavaScript,
        NativeConfidential,
    }
}