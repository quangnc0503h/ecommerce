using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class FilterLoginHistoryModel
    {
        public int[] Type { get; set; }

        public string UserName { get; set; }

        public DateTime? LoginTimeFrom { get; set; }

        public DateTime? LoginTimeTo { get; set; }

        public int[] LoginStatus { get; set; }

        public string RefreshToken { get; set; }

        public string[] AppId { get; set; }

        public string ClientUri { get; set; }

        public string ClientIP { get; set; }

        public string ClientUA { get; set; }

        public string ClientDevice { get; set; }

        public string ClientApiKey { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
}