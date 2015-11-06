
using Microsoft.AspNet.Identity.EntityFramework;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api
{
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Client> Clients { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AuthContext()
          : base("AuthContext")
        {
        }
    }
}