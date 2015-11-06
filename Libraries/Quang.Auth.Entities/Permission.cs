using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Entities
{
    public class Permission
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class PermissionGrant
    {
        public long Id { get; set; }
        public long PermissionId { get; set; }
        public int Type { get; set; }
        public bool IsExactPattern { get; set; }
        public string TermPattern { get; set; }
        public string TermExactPattern { get; set; }
    }

    public class PermissionItemGrant
    {
        public Permission Permission { get; set; }
        public bool IsGranted { get; set; }
    }
}