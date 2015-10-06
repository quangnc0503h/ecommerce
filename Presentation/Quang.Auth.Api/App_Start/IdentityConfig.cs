using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Quang.Auth.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api
{
    public class UserStore : IUserStore<ApplicationUser, long>
    {
        public Task CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
        public Task<ApplicationUser> FindByIdAsync(long userId)
        {
            throw new NotImplementedException();
        }
        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }
        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {

        }
    }
    public class ApplicationUserManager : UserManager<ApplicationUser, long>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, long> store)
           : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore());
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, long>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, long>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class RoleStore : IRoleStore<ApplicationRole, int>
    {
        public Task CreateAsync(ApplicationRole role) { throw new NotImplementedException(); }
        public Task DeleteAsync(ApplicationRole role) { throw new NotImplementedException(); }
        public Task<ApplicationRole> FindByIdAsync(int roleId) { throw new NotImplementedException(); }
        public Task<ApplicationRole> FindByNameAsync(string roleName) { throw new NotImplementedException(); }
        public Task UpdateAsync(ApplicationRole role) { throw new NotImplementedException(); }
        public void Dispose() {  }
    }
    public class ApplicationRoleManager : RoleManager<ApplicationRole, int>
    {
        public ApplicationRoleManager(RoleStore store)
            : base(store)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var manager = new ApplicationRoleManager(new RoleStore());

            return manager;
        }

      
    }
}