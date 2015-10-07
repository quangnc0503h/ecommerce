﻿using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity;


namespace Quang.Auth.Api.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole(string name) : base(name)
        {
        }

        public ApplicationRole(string name, int id) : base(name, id)
        {
        }
    }

    public class ApplicationDbContext : MySQLDatabase
    {
        public ApplicationDbContext(string connectionName)
      : base(connectionName)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext("Security");
        }
    }
}