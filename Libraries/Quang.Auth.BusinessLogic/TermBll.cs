using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System.Threading.Tasks;
namespace Quang.Auth.BusinessLogic
{
    public  static class TermBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<long> Delete(long termId)
        {
            return await TermDal.Delete(termId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            return await TermDal.Delete(Ids);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public async static Task<long> Insert(Term term)
        {
            return await TermDal.Insert(term);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public async static Task<long> Update(Term term)
        {
            return await TermDal.Update(term);
        }
        public static async Task<IEnumerable<Term>> GetAllTerms()
        {
            var terms = await TermDal.GetAllTerms();
            var listRoleKey = GetListRoleDictionary();
            foreach (var term in terms)
            {
                if (listRoleKey[term.RoleKey] != null)
                {
                    term.RoleKeyLabel = listRoleKey[term.RoleKey].RoleKeyLabel;
                }
            }
            return (terms);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public static async Task<Term> GetOneTerm(long termId)
        {
            var term = await TermDal.GetOneTerm(termId);
            if (term != null)
            {
                var item = GetListRoleOptions().Where(m => m.RoleKey == term.RoleKey).FirstOrDefault();
                if (item != null)
                {
                    term.RoleKeyLabel = item.RoleKeyLabel;
                }
            }
            return (term);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(string keyword)
        {
            return await TermDal.GetTotal(keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Term>> GetPaging(int pageSize, int pageNumber, string keyword)
        {
            return await TermDal.GetPaging(pageSize, pageNumber, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Term>> GetTermsByGroup(long groupId)
        {
            return await TermDal.GetTermsByGroup(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IDictionary<Term, bool>> GetTermsByUser(long userId)
        {
            return await TermDal.GetTermsByUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<IDictionary<string, bool>> GetUsersByTerm(long termId)
        {
            return await TermDal.GetUsersByTerm(termId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Term>> GetGroupTermsBelongToUser(long userId)
        {
            return await TermDal.GetGroupTermsBelongToUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IDictionary<Term, bool>> GetUserTermsWithGroupAccess(long userId)
        {
            return await TermDal.GetUserTermsWithGroupAccess(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="termId"></param>
        /// <param name="isAccess"></param>
        /// <returns></returns>
        public async static Task<long> AddTermToUser(long userId, long termId, bool isAccess)
        {
            return await TermDal.AddTermToUser(userId, termId, isAccess);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<long> RemoveTermFromUser(long userId, long termId)
        {
            return await TermDal.RemoveTermFromUser(userId, termId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<long> AddTermToGroup(long groupId, long termId)
        {
            return await TermDal.AddTermToGroup(groupId, termId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<long> RemoveTermFromGroup(long groupId, long termId)
        {
            return await TermDal.RemoveTermFromGroup(groupId, termId);
        }

        public static async Task<IEnumerable<User>> GetGrantedUsersByTerm(long termId)
        {
            IEnumerable<User> users = new List<User>();
            var term = await TermDal.GetOneTerm(termId);
            if (term != null)
            {
                var role = await RoleDal.GetRoleByName(term.RoleKey);
                if (role != null)
                {
                    users = await UserDal.GetUsersByRole(role.Id);
                }
            }

            return users;
        }

        public static async Task<IEnumerable<ActionRoleItem>> GetMissingTerms()
        {
            var terms = await GetAllTerms();
            var roles = GetListRoleOptions();
            var missingRoles = roles.Where(m => !terms.Any(n => n.RoleKey == m.RoleKey)).ToArray();
            return (missingRoles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<string>> GetGroupUsersBelongToTerm(long termId)
        {
            return await TermDal.GetGroupUsersBelongToTerm(termId);
        }
        public  static IEnumerable<ActionRoleItem> GetListRoleOptions()
        {
            return GetListRoleDictionary().Select(m => m.Value).ToArray();
        }

        public static IDictionary<string, ActionRoleItem> GetListRoleDictionary()
        {
            return ActionRole.ToListDictionary();
        }
        public static async  Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(long userId)
        {
            IList<GrantUserTerm> items = new List<GrantUserTerm>();
            var terms = await TermDal.GetUserTermsWithGroupAccess(userId);
            var userTerms = await TermDal.GetTermsByUser(userId);
            var listRoleKey = GetListRoleDictionary();
            foreach (var term in terms)
            {
                if (listRoleKey[term.Key.RoleKey] != null)
                {
                    term.Key.RoleKeyLabel = listRoleKey[term.Key.RoleKey].RoleKeyLabel;
                }
                var userTerm = userTerms.Where(m => m.Key.Id == term.Key.Id).SingleOrDefault();
                var isCustom = userTerm.Key != null ? true : false;
                var isAccess = isCustom ? userTerm.Value : false;
                items.Add(new GrantUserTerm { Term = term.Key, IsCustom = isCustom, IsAccess = isAccess, GroupIsAccess = term.Value });
            }
            return (items);
        }
        public async static Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(long groupId)
        {
            IList<GrantGroupTerm> items = new List<GrantGroupTerm>();
            var terms = await TermDal.GetAllTerms();
            var groupTerms = await TermDal.GetTermsByGroup(groupId);
            var listRoleKey = GetListRoleDictionary();
            foreach (var term in terms)
            {
                if (listRoleKey[term.RoleKey] != null)
                {
                    term.RoleKeyLabel = listRoleKey[term.RoleKey].RoleKeyLabel;
                }
                var groupTerm = groupTerms.Where(m => m.Id == term.Id).SingleOrDefault();
                var isAccess = groupTerm != null ? true : false;
                items.Add(new GrantGroupTerm { Term = term, IsAccess = isAccess });
            }
            return (items);
        }
        public static async Task<int> UpdateUserGrant(long userId, IEnumerable<GrantUserTerm> userGrants)
        {
            userGrants = userGrants ?? new GrantUserTerm[] { };
            var currentUserTerms = await TermDal.GetTermsByUser(userId);
            var newUserTerms = userGrants.Where(m => m.IsCustom == true)
                                .Where(m => !currentUserTerms.Any(n => n.Key.Id == m.Term.Id)
                                            || currentUserTerms.Any(n => n.Key.Id == m.Term.Id && n.Value != m.IsAccess)
                                ).ToArray();
            var oldUserTerms = currentUserTerms.Where(m => userGrants.Any(n => n.Term.Id == m.Key.Id && n.IsCustom == false)
                                           || newUserTerms.Any(n => n.Term.Id == m.Key.Id && n.IsCustom == true && n.IsAccess != m.Value)
                                ).ToArray();

            // Remove old user terms
            foreach (var oldUserTerm in oldUserTerms)
            {
               await  TermDal.RemoveTermFromUser(userId, oldUserTerm.Key.Id);
            }

            // Add new user terms
            foreach (var newUserTerm in newUserTerms)
            {
                await TermDal.AddTermToUser(userId, newUserTerm.Term.Id, newUserTerm.IsAccess);
            }

            return 1;
        }
        public static async Task<int> UpdateGroupGrant(long groupId, IEnumerable<GrantGroupTerm> groupGrants)
        {
            groupGrants = groupGrants ?? new GrantGroupTerm[] { };
            var currentGroupTerms = await TermDal.GetTermsByGroup(groupId);
            var newGroupTerms = groupGrants.Where(m => m.IsAccess == true && !currentGroupTerms.Any(n => n.Id == m.Term.Id)).ToArray();
            var oldGroupTerms = currentGroupTerms.Where(m => groupGrants.Any(n => n.IsAccess == false && n.Term.Id == m.Id)).ToArray();

            // Remove old user terms
            foreach (var oldGroupTerm in oldGroupTerms)
            {
                await TermDal.RemoveTermFromGroup(groupId, oldGroupTerm.Id);
            }

            // Add new user terms
            foreach (var newGroupTerm in newGroupTerms)
            {
                await TermDal.AddTermToGroup(groupId, newGroupTerm.Term.Id);
            }

            return (1);
        }
        public static async Task ReUpdateUserRole(long userId)
        {
            var currentUserRoles = await UserRolesDal.GetRolesByUserId(userId);
            var userTerms = await GetTermsByUser(userId);
            var userTermsInGroup = await GetGroupTermsBelongToUser(userId);

            var userRoles = userTermsInGroup.Where(m => !userTerms.Any(n => n.Value == false && n.Key.Id == m.Id));
            userRoles = userRoles.Union(userTerms.Where(m => m.Value == true).Select(m => m.Key), new TermComparer()).ToArray();

            var newUserRoles = userRoles.Where(m => !currentUserRoles.Any(n => n == m.RoleKey)).Select(m => m.RoleKey).ToArray();
            var oldUserRoles = currentUserRoles.Where(m => !userRoles.Any(n => n.RoleKey == m)).ToArray();

            // Delete
            foreach (var delRole in oldUserRoles)
            {
                var roleId = await RoleDal.GetRoleId(delRole);
                await UserRolesDal.Delete(userId, roleId);
            }
            // Add
            foreach (var role in newUserRoles)
            {
                var roleId = await RoleDal.GetRoleId(role);
                await UserRolesDal.Insert(userId, roleId);
            }
            // Remove old roles from user
           // await UserManager.RemoveFromRolesAsync(userId, oldUserRoles);

            // Add new roles to user
           // await UserManager.AddToRolesAsync(userId, newUserRoles);
        }

        public static async Task ReUpdateGroupRole(long groupId)
        {
            var users = await UserBll.GetUsersByGroup(groupId);
            foreach (var user in users)
            {
                await ReUpdateUserRole(user.Id);
            }
        }
        public static async Task SynchTermsToRoles()
        {
            var currentRoles = await RoleDal.GetAllRoles();
            var terms =  await GetAllTerms();
            var newRoles = terms.Where(m => !currentRoles.Any(n => n.Name == m.RoleKey)).Select(m => m.RoleKey).ToArray();
            var oldRoles = currentRoles.Where(m => !terms.Any(n => n.RoleKey == m.Name)).ToArray();

            // Remove old roles
            foreach (var oldRole in oldRoles)
            {
                await RoleDal.Delete(oldRole.Id);
            }

            // Add new roles
            foreach (var newRole in newRoles)
            {
                var role = new Role { Name = newRole };
                await RoleDal.Insert(role);
            }

        }

        public class TermComparer : IEqualityComparer<Term>
        {
            public bool Equals(Term item1, Term item2)
            {
                if (item1 == null && item2 == null)
                {
                    return true;
                }
                else
                {
                    if ((item1 != null && item2 == null) || (item1 == null && item2 != null))
                    {
                        return false;
                    }
                    else
                    {
                        return item1.Id == item2.Id;
                    }
                }
            }

            public int GetHashCode(Term item)
            {
                return new { item.Id }.GetHashCode();
            }
        }
    }
}
