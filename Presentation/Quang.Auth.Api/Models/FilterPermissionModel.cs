using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class FilterPermissionModel
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }

    public class PermissionModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}