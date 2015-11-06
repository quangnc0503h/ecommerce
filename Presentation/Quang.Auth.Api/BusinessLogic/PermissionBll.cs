using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class PermissionBll : IPermissionBll
    {
        private IPermissionTable _permissionTable;
        private IUserTable _userTable;
        private IGroupTable _groupTable;
        private ITermTable _termTable;

        public MySQLDatabase Database { get; private set; }

        public ApplicationUserManager UserManager
        {
            get
            {
                return OwinContextExtensions.GetUserManager<ApplicationUserManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return OwinContextExtensions.GetUserManager<ApplicationRoleManager>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            }
        }

        public PermissionBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._permissionTable = (IPermissionTable)new PermissionTable(this.Database);
            this._userTable = (IUserTable)new UserTable(this.Database);
            this._groupTable = (IGroupTable)new GroupTable(this.Database);
            this._termTable = (ITermTable)new TermTable(this.Database);
        }

        public Task<Permission> GetOnePermission(int permissionId)
        {
            return Task.FromResult<Permission>(this._permissionTable.GetOnePermission(permissionId));
        }

        public Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput input)
        {
            int total = this._permissionTable.GetTotal(input.Keyword);
            IEnumerable<Permission> paging = this._permissionTable.GetPaging(input.PageSize, input.PageNumber, input.Keyword);
            return Task.FromResult<DanhSachPermissionOutput>(new DanhSachPermissionOutput()
            {
                DanhSachPermissions = paging,
                TotalCount = (long)total
            });
        }

        public async Task<int> DeletePermission(IEnumerable<int> Ids)
        {
            int result = this._permissionTable.Delete(Ids);
            if (result > 0)
            {
                IList<int> userIds = (IList<int>)new List<int>();
                foreach (int permissionId in Ids)
                {
                    userIds = (IList<int>)Enumerable.ToList<int>(Enumerable.Concat<int>((IEnumerable<int>)userIds, this._permissionTable.GetAllUserIdHasPermission(permissionId)));
                    this._permissionTable.DeletePermissionGrants(permissionId);
                }
                foreach (int userId in Enumerable.Distinct<int>((IEnumerable<int>)userIds))
                {
                    int num = await this.GenerateRolesForUser(userId);
                }
            }
            return result;
        }

        public Task<int> InsertPermission(CreatePermissionInput input)
        {
            Mapper.CreateMap<CreatePermissionInput, Permission>();
            return Task.FromResult<int>(this._permissionTable.Insert(Mapper.Map<Permission>((object)input)));
        }

        public Task<int> UpdatePermission(UpdatePermissionInput input)
        {
            Mapper.CreateMap<UpdatePermissionInput, Permission>();
            return Task.FromResult<int>(this._permissionTable.Update(Mapper.Map<Permission>((object)input)));
        }

        public Task<IEnumerable<Permission>> GetAllPermissions()
        {
            return Task.FromResult<IEnumerable<Permission>>(this._permissionTable.GetAllPermissions());
        }

        public Task<GetPermissionGrantsOutput> GetPermissionGrants(int permissionId)
        {
            IEnumerable<PermissionGrant> permissionGrants = this._permissionTable.GetPermissionGrants(permissionId);
            return Task.FromResult<GetPermissionGrantsOutput>(new GetPermissionGrantsOutput()
            {
                AllowGrants = (IEnumerable<PermissionGrant>)Enumerable.ToArray<PermissionGrant>(Enumerable.Where<PermissionGrant>(permissionGrants, (Func<PermissionGrant, bool>)(m => m.Type == 1))),
                DenyGrants = (IEnumerable<PermissionGrant>)Enumerable.ToArray<PermissionGrant>(Enumerable.Where<PermissionGrant>(permissionGrants, (Func<PermissionGrant, bool>)(m => m.Type == 0)))
            });
        }

        public async Task<int> UpdatePermissionGrants(UpdatePermissionGrantsInput input)
        {
            foreach (PermissionGrant permissionGrant in input.AllowGrants)
            {
                permissionGrant.Type = 1;
                permissionGrant.PermissionId = input.PermissionId;
                if (permissionGrant.IsExactPattern)
                {
                    permissionGrant.TermPattern = (string)null;
                }
                else
                {
                    permissionGrant.TermExactPattern = (string)null;
                    permissionGrant.TermPattern = permissionGrant.TermPattern == "*" ? ".*" : permissionGrant.TermPattern;
                }
            }
            foreach (PermissionGrant permissionGrant in input.DenyGrants)
            {
                permissionGrant.Type = 0;
                permissionGrant.PermissionId = input.PermissionId;
                if (permissionGrant.IsExactPattern)
                    permissionGrant.TermPattern = (string)null;
                else
                    permissionGrant.TermExactPattern = (string)null;
            }
            PermissionGrant[] permissionGrants = Enumerable.ToArray<PermissionGrant>(Enumerable.Where<PermissionGrant>(Enumerable.Concat<PermissionGrant>(input.AllowGrants, input.DenyGrants), (Func<PermissionGrant, bool>)(m =>
            {
                if (m.IsExactPattern && !string.IsNullOrEmpty(m.TermExactPattern))
                    return true;
                if (!m.IsExactPattern)
                    return !string.IsNullOrEmpty(m.TermPattern);
                return false;
            })));
            int result = this._permissionTable.UpdatePermissionGrants(input.PermissionId, (IEnumerable<PermissionGrant>)permissionGrants);
            int num = await this.GenerateRolesForUserByPermission(input.PermissionId);
            return result;
        }

        public Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(int userId)
        {
            IEnumerable<Permission> allPermissions = this._permissionTable.GetAllPermissions();
            int[] userPermissions = Enumerable.ToArray<int>(Enumerable.Select<Permission, int>(this._permissionTable.GetUserPermissions(userId), (Func<Permission, int>)(m => m.Id)));
            return Task.FromResult<IEnumerable<PermissionItemGrant>>((IEnumerable<PermissionItemGrant>)Enumerable.ToArray<PermissionItemGrant>(Enumerable.Select<Permission, PermissionItemGrant>(allPermissions, (Func<Permission, PermissionItemGrant>)(m => new PermissionItemGrant()
            {
                IsGranted = Enumerable.Contains<int>((IEnumerable<int>)userPermissions, m.Id),
                Permission = m
            }))));
        }

        public Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(int groupId)
        {
            IEnumerable<Permission> allPermissions = this._permissionTable.GetAllPermissions();
            int[] groupPermissions = Enumerable.ToArray<int>(Enumerable.Select<Permission, int>(this._permissionTable.GetGroupPermissions(groupId), (Func<Permission, int>)(m => m.Id)));
            return Task.FromResult<IEnumerable<PermissionItemGrant>>((IEnumerable<PermissionItemGrant>)Enumerable.ToArray<PermissionItemGrant>(Enumerable.Select<Permission, PermissionItemGrant>(allPermissions, (Func<Permission, PermissionItemGrant>)(m => new PermissionItemGrant()
            {
                IsGranted = Enumerable.Contains<int>((IEnumerable<int>)groupPermissions, m.Id),
                Permission = m
            }))));
        }

        public async Task<int> UpdateUserPermissions(UpdateUserPermissionInput input)
        {
            int result = this._permissionTable.UpdateUserPermissions(input.UserId, input.PermissionIds);
            if (result > 0)
            {
                int num = await this.GenerateRolesForUser(input.UserId);
            }
            return result;
        }

        public async Task<int> UpdateGroupPermissions(UpdateGroupPermissionInput input)
        {
            int result = this._permissionTable.UpdateGroupPermissions(input.GroupId, input.PermissionIds);
            if (result > 0)
            {
                int num = await this.GenerateRolesForGroup(input.GroupId);
            }
            return result;
        }

        public async Task<int> GenerateRolesForGroup(int groupId)
        {
            int result = 0;
            IEnumerable<User> users = this._userTable.GetUsersByGroup(groupId);
            foreach (User user in users)
            {
                int num = await this.GenerateRolesForUser(user.Id);
            }
            result = Enumerable.Count<User>(users);
            return result;
        }

        public async Task<int> GenerateRolesForUser(int userId)
        {
            int result = 0;
            UnityContainer unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            ITermBll termBll = UnityContainerExtensions.Resolve<ITermBll>((IUnityContainer)unityContainer);
            IEnumerable<Term> allTerms = await termBll.GetAllTerms();
            IEnumerable<PermissionGrant> permissionGrants = this._permissionTable.GetAllPermissionGrantBelongToUser(userId);
            IEnumerable<Term> allowTerms = (IEnumerable<Term>)new List<Term>();
            IEnumerable<Term> denyTerms = (IEnumerable<Term>)new List<Term>();
            foreach (PermissionGrant permissionGrant in permissionGrants)
            {
                if (permissionGrant.Type == 1)
                    allowTerms = Enumerable.Concat<Term>(allowTerms, this.GetTermFromPermissionGrant(permissionGrant, allTerms));
                else if (permissionGrant.Type == 0)
                    denyTerms = Enumerable.Concat<Term>(denyTerms, this.GetTermFromPermissionGrant(permissionGrant, allTerms));
            }
            string[] newRoles = Enumerable.ToArray<string>(Enumerable.Distinct<string>(Enumerable.Select<Term, string>(Enumerable.Where<Term>(allowTerms, (Func<Term, bool>)(m => !Enumerable.Any<Term>(denyTerms, (Func<Term, bool>)(n => n.Id == m.Id)))), (Func<Term, string>)(m => m.RoleKey))));
            IList<string> currentRoles = await this.UserManager.GetRolesAsync(userId);
            string[] delRoles = Enumerable.ToArray<string>(Enumerable.Where<string>((IEnumerable<string>)currentRoles, (Func<string, bool>)(m => !Enumerable.Contains<string>((IEnumerable<string>)newRoles, m))));
            string[] addRoles = Enumerable.ToArray<string>(Enumerable.Where<string>((IEnumerable<string>)newRoles, (Func<string, bool>)(m => !currentRoles.Contains(m))));
            IdentityResult identityResult1 = await this.UserManager.RemoveFromRolesAsync(userId, delRoles);
            IdentityResult identityResult2 = await this.UserManager.AddToRolesAsync(userId, addRoles);
            result = 1;
            return result;
        }

        private IEnumerable<Term> GetTermFromPermissionGrant(PermissionGrant permissionGrant, IEnumerable<Term> allTerms)
        {
            IList<Term> list = (IList<Term>)new List<Term>();
            if (permissionGrant.IsExactPattern)
            {
                Term term = Enumerable.FirstOrDefault<Term>(allTerms, (Func<Term, bool>)(m => m.RoleKey == permissionGrant.TermExactPattern));
                if (term != null)
                    list.Add(term);
            }
            else if (permissionGrant.TermPattern == "*" || permissionGrant.TermPattern == ".*")
            {
                list = (IList<Term>)Enumerable.ToList<Term>(allTerms);
            }
            else
            {
                foreach (Term term in allTerms)
                {
                    try
                    {
                        if (Regex.IsMatch(term.RoleKeyLabel, permissionGrant.TermPattern))
                            list.Add(term);
                    }
                    catch
                    {
                    }
                }
            }
            return (IEnumerable<Term>)list;
        }

        public async Task<int> GenerateRolesForUserByPermission(int permissionId)
        {
            IEnumerable<int> userIds = this._permissionTable.GetAllUserIdHasPermission(permissionId);
            foreach (int userId in userIds)
            {
                int num = await this.GenerateRolesForUser(userId);
            }
            int result = 1;
            return result;
        }

        public async Task<int> ReUpdateAllUserRoles()
        {
            ApplicationUser[] users = Enumerable.ToArray<ApplicationUser>((IEnumerable<ApplicationUser>)this.UserManager.Users);
            foreach (ApplicationUser applicationUser in users)
            {
                int num = await this.GenerateRolesForUser(applicationUser.Id);
            }
            bool flag =false;
            int num1 = flag ? 1 : 0;
            int result = 1;
            return result;
        }
    }
}