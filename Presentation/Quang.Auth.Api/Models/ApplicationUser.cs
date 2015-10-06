using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quang.Auth.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Quang.Auth.Api.Models
{
    public class ApplicationUser : User
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, long> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : Role
    {
        public ApplicationRole(string name) : base(name)
        {
        }

        public ApplicationRole(string name, int id) : base(name, id)
        {
        }
    }
}