using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System.Text.RegularExpressions;

namespace Quang.Auth.BusinessLogic
{
    public static class PermissionBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<long> Delete(long permissionId)
        {
            return await PermissionDal.Delete(permissionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            return await PermissionDal.Delete(Ids);
        }
        public static async Task<long> DeletePermission(IEnumerable<long> Ids)
        {
            var result = await Delete(Ids);
            if (result > 0)
            {
                IList<long> userIds = new List<long>();
                foreach (var permissionId in Ids)
                {
                    userIds = userIds.Concat(await PermissionDal.GetAllUserIdHasPermission(permissionId)).ToList();
                    await PermissionDal.DeletePermissionGrants(permissionId);
                }
                foreach (var userId in userIds.Distinct())
                {
                    await GenerateRolesForUser(userId);
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async static Task<long> Insert(Permission permission)
        {
            return await PermissionDal.Insert(permission);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async static Task<long> Update(Permission permission)
        {
            return await PermissionDal.Update(permission);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<Permission> GetOnePermission(long permissionId)
        {
            return await PermissionDal.GetOnePermission(permissionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(string keyword)
        {
            return await PermissionDal.GetTotal(keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Permission>> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            return await PermissionDal.GetPaging(pageSize, pageNumber, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<IEnumerable<Permission>> GetAllPermissions()
        {
            return await PermissionDal.GetAllPermissions();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<PermissionGrant>> GetPermissionGrants(long permissionId)
        {
            return await PermissionDal.GetPermissionGrants(permissionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <param name="permissionGrants"></param>
        /// <returns></returns>
        public async static Task<long> UpdatePermissionGrants(long permissionId, IEnumerable<PermissionGrant> allowGrants, IEnumerable<PermissionGrant> denyGrants)
        {
            foreach (var item in allowGrants)
            {
                item.Type = 1;
                item.PermissionId = permissionId;
                if (item.IsExactPattern)
                {
                    item.TermPattern = null;
                }
                else
                {
                    item.TermExactPattern = null;
                }
            }
            foreach (var item in denyGrants)
            {
                item.Type = 0;
                item.PermissionId = permissionId;
                if (item.IsExactPattern)
                {
                    item.TermPattern = null;
                }
                else
                {
                    item.TermExactPattern = null;
                }
            }
            var permissionGrants = allowGrants.Concat(denyGrants)
                .Where(m => (m.IsExactPattern && !string.IsNullOrEmpty(m.TermExactPattern)) || (!m.IsExactPattern && !string.IsNullOrEmpty(m.TermPattern))).ToArray();
            var result = await PermissionDal.UpdatePermissionGrants(permissionId, permissionGrants);

            // Reupdate role for all groups/users
            await GenerateRolesForUserByPermission(permissionId);

            return result;
        }

        public async static Task<long> DeletePermissionGrant(IEnumerable<long> Ids)
        {
            return await PermissionDal.DeletePermissionGrant(Ids);
        }
     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(long groupId)
        {
            var allPermissions = await PermissionDal.GetAllPermissions();
            var groupPermissions = (await PermissionDal.GetGroupPermissions(groupId)).Select(m => m.Id).ToArray();
            var result = allPermissions.Select(m => new PermissionItemGrant { IsGranted = groupPermissions.Contains(m.Id), Permission = m }).ToArray();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissionIds"></param>
        /// <returns></returns>
        public async static Task<long> UpdateUserPermissions(long userId, IEnumerable<long> permissionIds)
        {
            return await PermissionDal.UpdateUserPermissions(userId, permissionIds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissionIds"></param>
        /// <returns></returns>
        public async static Task<long> DeleteUserPermissions(long userId, IEnumerable<long> permissionIds)
        {
            return await PermissionDal.DeleteUserPermissions(userId, permissionIds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permissionIds"></param>
        /// <returns></returns>
        public static async Task<long> UpdateGroupPermissions(long groupId, IEnumerable<long> permissionIds)
        {
            return await PermissionDal.UpdateGroupPermissions(groupId, permissionIds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="permissionIds"></param>
        /// <returns></returns>
        public async static Task<long> DeleteGroupPermissions(long groupId, IEnumerable<long> permissionIds)
        {
            return await PermissionDal.DeleteGroupPermissions(groupId, permissionIds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<PermissionGrant>> GetAllPermissionGrantBelongToUser(long userId)
        {
            return await PermissionDal.GetAllPermissionGrantBelongToUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<long>> GetAllUserIdHasPermission(long permissionId)
        {
            return await PermissionDal.GetAllUserIdHasPermission(permissionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<long> DeleteUserPermissions(long userId)
        {
            return await PermissionDal.DeleteUserPermissions(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<long> DeleteGroupPermissions(long groupId)
        {
            return await PermissionDal.DeleteGroupPermissions(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        public async static Task<long> DeletePermissionGrants(long permissionId)
        {
            return await PermissionDal.DeletePermissionGrants(permissionId);
        }

        public async static Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(long userId)
        {
            var allPermissions = await PermissionDal.GetAllPermissions();
            var userPermissions = (await PermissionDal.GetUserPermissions(userId)).Select(m => m.Id).ToArray();
            var result = allPermissions.Select(m => new PermissionItemGrant { IsGranted = userPermissions.Contains(m.Id), Permission = m }).ToArray();

            return (result);
        }

        public async static Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(int groupId)
        {
            var allPermissions = await PermissionDal.GetAllPermissions();
            var groupPermissions = (await PermissionDal.GetGroupPermissions(groupId)).Select(m => m.Id).ToArray();
            var result = allPermissions.Select(m => new PermissionItemGrant { IsGranted = groupPermissions.Contains(m.Id), Permission = m }).ToArray();

            return (result);
        }


        public static async Task<int> GenerateRolesForGroup(long groupId)
        {
            var result = 0;
            var users = await UserDal.GetUsersByGroup(groupId);
            foreach (var user in users)
            {
                await GenerateRolesForUser(user.Id);
            }
            result = users.Count();
            return result;
        }
        public async static Task<long> GenerateRolesForUser(long userId)
        {
            int result = 0;
            
            
            var allTerms = await TermBll.GetAllTerms();
            var permissionGrants = await GetAllPermissionGrantBelongToUser(userId);

            IEnumerable<Term> allowTerms = new List<Term>();
            IEnumerable<Term> denyTerms = new List<Term>();
            foreach (var permissionGrant in permissionGrants)
            {
                // Allow
                if (permissionGrant.Type == 1)
                {
                    allowTerms = allowTerms.Concat(GetTermFromPermissionGrant(permissionGrant, allTerms));
                }
                // Deny
                else if (permissionGrant.Type == 0)
                {
                    denyTerms = denyTerms.Concat(GetTermFromPermissionGrant(permissionGrant, allTerms));
                }
            }
            var newRoles = allowTerms.Where(m => !denyTerms.Any(n => n.Id == m.Id)).Select(m => m.RoleKey).Distinct().ToArray();
            var currentRoles = await UserRolesDal.GetRolesByUserId(userId);
            var delRoles = currentRoles.Where(m => !newRoles.Contains(m)).ToArray();
            var addRoles = newRoles.Where(m => !currentRoles.Contains(m)).ToArray();

            // Delete
            foreach(var delRole in delRoles)
            {
                var roleId = await RoleDal.GetRoleId(delRole);
                await UserRolesDal.Delete(userId, roleId);
            }            
            // Add
            foreach(var role in addRoles)
            {
                var roleId = await RoleDal.GetRoleId(role);
                await UserRolesDal.Insert(userId, roleId);
            }
                        // success
            result = 1;

            return result;
        }
        private static IEnumerable<Term> GetTermFromPermissionGrant(PermissionGrant permissionGrant, IEnumerable<Term> allTerms)
        {
            IList<Term> result = new List<Term>();
            if (permissionGrant.IsExactPattern)
            {
                var term = allTerms.FirstOrDefault(m => m.RoleKey == permissionGrant.TermExactPattern);
                if (term != null)
                {
                    result.Add(term);
                }
            }
            else
            {
                IEnumerable<Term> terms;
                if (permissionGrant.TermPattern == "*" || permissionGrant.TermPattern == ".*")
                {
                    result = allTerms.ToList();
                }
                else
                {
                    foreach (var term in allTerms)
                    {
                        try
                        {
                            if (Regex.IsMatch(term.RoleKeyLabel, permissionGrant.TermPattern))
                            {
                                result.Add(term);
                            }
                        }
                        catch
                        {
                            terms = new Term[] { };
                        }
                    }
                }
            }
            return result;
        }
        public static async Task<long> GenerateRolesForUserByPermission(long permissionId)
        {
            var userIds = await GetAllUserIdHasPermission(permissionId);
            foreach (var userId in userIds)
            {
                await GenerateRolesForUser(userId);
            }
            var result = 1;
            return result;
        }

        public static async Task<int> ReUpdateAllUserRoles()
        {
            var users = (await UserDal.GetAllUsers()).ToArray();
            foreach (var user in users)
            {
                await GenerateRolesForUser(user.Id);
            }
            var result = 1;
            return result;
        }
    }
}
