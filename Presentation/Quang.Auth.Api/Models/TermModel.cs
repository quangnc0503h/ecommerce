using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class TermModel
    {
        public long Id { get; set; }

        public string RoleKey { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    
    public class UserGrantModel
    {
        public long UserId { get; set; }
        public IEnumerable<GrantUserTerm> UserGrants { get; set; }
    }
    public class GroupGrantModel
    {
        public long GroupId { get; set; }
        public IEnumerable<GrantGroupTerm> GroupGrants { get; set; }
    }
}