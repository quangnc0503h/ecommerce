using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Group
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string ParentName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int TotalMembers { get; set; }
    }
}