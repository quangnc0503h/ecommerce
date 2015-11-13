using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class PermissionBll : IPermissionBll
    {
        private readonly IPermissionTable _permissionTable;
        private readonly IUserTable _userTable;

        public MySQLDatabase Database { get; private set; }

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        public PermissionBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _permissionTable = new PermissionTable(Database);
            _userTable = new UserTable(Database);
        
        }

        public Task<Permission> GetOnePermission(int permissionId)
        {
            return Task.FromResult(_permissionTable.GetOnePermission(permissionId));
        }

        public Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput input)
        {
            int total = _permissionTable.GetTotal(input.Keyword);
            IEnumerable<Permission> paging = _permissionTable.GetPaging(input.PageSize, input.PageNumber, input.Keyword);
            return Task.FromResult(new DanhSachPermissionOutput
                                   {
                DanhSachPermissions = paging,
                TotalCount = total
            });
        }

        public async Task<int> DeletePermission(IEnumerable<int> Ids)
        {
            int result = _permissionTable.Delete(Ids);
            if (result > 0)
            {
                IList<int> userIds = new List<int>();
                foreach (int permissionId in Ids)
                {
                    userIds = userIds.Concat(_permissionTable.GetAllUserIdHasPermission(permissionId)).ToList();
                    _permissionTable.DeletePermissionGrants(permissionId);
                }
                foreach (int userId in userIds.Distinct())
                {
                    int num = await GenerateRolesForUser(userId);
                }
            }
            return result;
        }

        public Task<int> InsertPermission(CreatePermissionInput input)
        {
            Mapper.CreateMap<CreatePermissionInput, Permission>();
            return Task.FromResult(_permissionTable.Insert(Mapper.Map<Permission>(input)));
        }

        public Task<int> UpdatePermission(UpdatePermissionInput input)
        {
            Mapper.CreateMap<UpdatePermissionInput, Permission>();
            return Task.FromResult(_permissionTable.Update(Mapper.Map<Permission>(input)));
        }

        public Task<IEnumerable<Permission>> GetAllPermissions()
        {
            return Task.FromResult(_permissionTable.GetAllPermissions());
        }

        public Task<GetPermissionGrantsOutput> GetPermissionGrants(int permissionId)
        {
            IEnumerable<PermissionGrant> permissionGrants = _permissionTable.GetPermissionGrants(permissionId);
            return Task.FromResult(new GetPermissionGrantsOutput
                                   {
                AllowGrants = permissionGrants.Where(m => m.Type == 1).ToArray(),
                DenyGrants = permissionGrants.Where(m => m.Type == 0).ToArray()
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
                    permissionGrant.TermPattern = null;
                }
                else
                {
                    permissionGrant.TermExactPattern = null;
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
            PermissionGrant[] permissionGrants = input.AllowGrants.Concat(input.DenyGrants).Where(m =>
                                                                                                             {
                                                                                                                 if (m.IsExactPattern && !string.IsNullOrEmpty(m.TermExactPattern))
                                                                                                                     return true;
                                                                                                                 if (!m.IsExactPattern)
                                                                                                                     return !string.IsNullOrEmpty(m.TermPattern);
                                                                                                                 return false;
                                                                                                             }).ToArray();
            int result = _permissionTable.UpdatePermissionGrants(input.PermissionId, permissionGrants);
            int num = await GenerateRolesForUserByPermission(input.PermissionId);
            return result;
        }

        public Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(int userId)
        {
            IEnumerable<Permission> allPermissions = _permissionTable.GetAllPermissions();
            int[] userPermissions = _permissionTable.GetUserPermissions(userId).Select(m => m.Id).ToArray();
            return Task.FromResult((IEnumerable<PermissionItemGrant>)allPermissions.Select(m => new PermissionItemGrant()
                                                                                                {
                                                                                                    IsGranted = userPermissions.Contains(m.Id),
                                                                                                    Permission = m
                                                                                                }).ToArray());
        }

        public Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(int groupId)
        {
            IEnumerable<Permission> allPermissions = _permissionTable.GetAllPermissions();
            int[] groupPermissions = _permissionTable.GetGroupPermissions(groupId).Select(m => m.Id).ToArray();
            return Task.FromResult((IEnumerable<PermissionItemGrant>)allPermissions.Select(m => new PermissionItemGrant()
                                                                                                {
                                                                                                    IsGranted = groupPermissions.Contains(m.Id),
                                                                                                    Permission = m
                                                                                                }).ToArray());
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
            IEnumerable<User> users = _userTable.GetUsersByGroup(groupId);
            foreach (User user in users)
            {
                await GenerateRolesForUser(user.Id);
            }
            int result = users.Count();
            return result;
        }

        public async Task<int> GenerateRolesForUser(int userId)
        {
            int result = 0;
            var unityContainer = UnityConfig.GetConfiguredContainer() as UnityContainer;
            var termBll = unityContainer.Resolve<ITermBll>();
            IEnumerable<Term> allTerms = await termBll.GetAllTerms();
            IEnumerable<PermissionGrant> permissionGrants = _permissionTable.GetAllPermissionGrantBelongToUser(userId);
            IEnumerable<Term> allowTerms = new List<Term>();
            var denyTerms = (IEnumerable<Term>)new List<Term>();
            foreach (PermissionGrant permissionGrant in permissionGrants)
            {
                if (permissionGrant.Type == 1)
                    allowTerms = allowTerms.Concat(this.GetTermFromPermissionGrant(permissionGrant, allTerms));
                else if (permissionGrant.Type == 0)
                    denyTerms = denyTerms.Concat(GetTermFromPermissionGrant(permissionGrant, allTerms));
            }
            var newRoles = allowTerms.Where(m => denyTerms.All(n => n.Id != m.Id)).Select(m => m.RoleKey).Distinct().ToArray();
            IList<string> currentRoles = await UserManager.GetRolesAsync(userId);
            string[] delRoles = currentRoles.Where(m => !newRoles.Contains(m)).ToArray();
            string[] addRoles = newRoles.Where(m => !currentRoles.Contains(m)).ToArray();
            await UserManager.RemoveFromRolesAsync(userId, delRoles);
            await UserManager.AddToRolesAsync(userId, addRoles);
            result = 1;
            return result;
        }

        private IEnumerable<Term> GetTermFromPermissionGrant(PermissionGrant permissionGrant, IEnumerable<Term> allTerms)
        {
            IList<Term> list = new List<Term>();
            if (permissionGrant.IsExactPattern)
            {
                Term term = allTerms.FirstOrDefault(m => m.RoleKey == permissionGrant.TermExactPattern);
                if (term != null)
                    list.Add(term);
            }
            else if (permissionGrant.TermPattern == "*" || permissionGrant.TermPattern == ".*")
            {
                list = allTerms.ToList();
            }
            else
            {
                foreach (var term in allTerms)
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
            return list;
        }

        public async Task<int> GenerateRolesForUserByPermission(int permissionId)
        {
            IEnumerable<int> userIds = _permissionTable.GetAllUserIdHasPermission(permissionId);
            foreach (int userId in userIds)
            {
                int num = await GenerateRolesForUser(userId);
            }
            const int result = 1;
            return result;
        }

        public async Task<int> ReUpdateAllUserRoles()
        {
            ApplicationUser[] users = UserManager.Users.ToArray();
            foreach (ApplicationUser applicationUser in users)
            {
                int num = await GenerateRolesForUser(applicationUser.Id);
            }
           
            const int result = 1;
            return result;
        }
    }
}