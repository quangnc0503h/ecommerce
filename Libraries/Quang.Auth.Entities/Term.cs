using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Entities
{
    public class Term
    {
        public int Id { get; set; }

        public string RoleKey { get; set; }

        public string RoleKeyLabel { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    public class GrantGroupTerm
    {
        public Term Term { get; set; }

        public bool IsAccess { get; set; }
    }

    public class GrantUserTerm
    {
        public Term Term { get; set; }
        public bool IsCustom { get; set; }
        public bool IsAccess { get; set; }
        public bool GroupIsAccess { get; set; }
    }
}