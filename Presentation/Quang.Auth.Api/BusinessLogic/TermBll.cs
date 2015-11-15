using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class TermBll : ITermBll
    {
        private readonly ITermTable _termTable;
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

        public TermBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _termTable = new TermTable(Database);
            _userTable = new UserTable(Database);
            //new GroupTable(Database);
        }

        public Task<Term> GetOneTerm(int termId)
        {
            Term term = _termTable.GetOneTerm(termId);
            if (term != null)
            {
                var actionRoleItem = GetListRoleOptions().FirstOrDefault(m => m.RoleKey == term.RoleKey);
                if (actionRoleItem != null)
                    term.RoleKeyLabel = actionRoleItem.RoleKeyLabel;
            }
            return Task.FromResult(term);
        }

        public Task<DanhSachTermOutput> GetAll(FilterTermInput input)
        {
            int total = _termTable.GetTotal(input.Keyword);
            IEnumerable<Term> paging = _termTable.GetPaging(input.PageSize, input.PageNumber, input.Keyword);
            IDictionary<string, ActionRoleItem> listRoleDictionary = GetListRoleDictionary();
            var danhSachTerms = paging as Term[] ?? paging.ToArray();
            foreach (var term in danhSachTerms)
            {
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
            }
            return Task.FromResult(new DanhSachTermOutput
                                   {
                DanhSachTerms = danhSachTerms,
                TotalCount = total
            });
        }

        public Task<IEnumerable<Term>> GetAllTerms()
        {
            IEnumerable<Term> allTerms = _termTable.GetAllTerms();
            IDictionary<string, ActionRoleItem> listRoleDictionary = GetListRoleDictionary();
            foreach (Term term in allTerms)
            {
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
            }
            return Task.FromResult(allTerms);
        }

        public async Task<IEnumerable<User>> GetGrantedUsersByTerm(int termId)
        {
            IEnumerable<User> users = new List<User>();
            Term term = _termTable.GetOneTerm(termId);
            if (term != null)
            {
                var role = await RoleManager.FindByNameAsync(term.RoleKey);
                if (role != null)
                    users = _userTable.GetUsersByRole(role.Id);
            }
            return users;
        }

        public Task<IEnumerable<ActionRoleItem>> GetMissingTerms()
        {
            IEnumerable<Term> terms = _termTable.GetAllTerms();
            return Task.FromResult<IEnumerable<ActionRoleItem>>(GetListRoleOptions().Where(m => terms.All(n => n.RoleKey != m.RoleKey)).ToArray());
        }

        public Task<int> DeleteTerm(int termId)
        {
            return Task.FromResult(_termTable.Delete(termId));
        }

        public Task<int> DeleteTerm(IEnumerable<int> Ids)
        {
            return Task.FromResult(_termTable.Delete(Ids));
        }

        public Task<int> InsertTerm(CreateTermInput input)
        {
            Mapper.CreateMap<CreateTermInput, Term>();
            return Task.FromResult(_termTable.Insert(Mapper.Map<Term>(input)));
        }

        public Task<int> UpdateTerm(UpdateTermInput input)
        {
            Mapper.CreateMap<UpdateTermInput, Term>();
            return Task.FromResult(_termTable.Update(Mapper.Map<Term>(input)));
        }

        public IEnumerable<ActionRoleItem> GetListRoleOptions()
        {
            return GetListRoleDictionary().Select(m => m.Value).ToArray();
        }

        public IDictionary<string, ActionRoleItem> GetListRoleDictionary()
        {
            return ActionRole.ToListDictionary();
        }

        public Task<IEnumerable<Term>> GetTermsByGroup(int groupId)
        {
            return Task.FromResult(_termTable.GetTermsByGroup(groupId));
        }

        public Task<IDictionary<Term, bool>> GetTermsByUser(int userId)
        {
            return Task.FromResult(_termTable.GetTermsByUser(userId));
        }

        public async Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(string userId)
        {
            IList<GrantUserTerm> items = new List<GrantUserTerm>();
            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(userId) && Regex.IsMatch(userId, "^[1-9]([0-9]*)$"))
                user = await UserManager.FindByIdAsync(int.Parse(userId));
            if (user == null && !string.IsNullOrEmpty(userId))
                user = await UserManager.FindByNameAsync(userId);
            if (user != null)
            {
                var termsWithGroupAccess = _termTable.GetUserTermsWithGroupAccess(user.Id);
                var termsByUser = _termTable.GetTermsByUser(user.Id);
                var listRoleDictionary = GetListRoleDictionary();
                foreach (KeyValuePair<Term, bool> keyValuePair1 in termsWithGroupAccess)
                {
                    var term = keyValuePair1;
                    if (listRoleDictionary.ContainsKey(term.Key.RoleKey) && listRoleDictionary[term.Key.RoleKey] != null)
                        term.Key.RoleKeyLabel = listRoleDictionary[term.Key.RoleKey].RoleKeyLabel;
                    var keyValuePair2 = termsByUser.SingleOrDefault(m => m.Key.Id == term.Key.Id);
                    bool flag1 = keyValuePair2.Key != null;
                    bool flag2 = flag1 && keyValuePair2.Value;
                    items.Add(new GrantUserTerm
                              {
                        Term = term.Key,
                        IsCustom = flag1,
                        IsAccess = flag2,
                        GroupIsAccess = term.Value
                    });
                }
            }
            return (IEnumerable<GrantUserTerm>)items;
        }

        public Task<int> UpdateUserGrant(UpdateUserGrantInput input)
        {
            input.UserGrants = input.UserGrants ?? new GrantUserTerm[0];
            var currentUserTerms = _termTable.GetTermsByUser(input.UserId);
            var newUserTerms = input.UserGrants.Where(m => m.IsCustom).Where(m =>
                                                                                         {
                                                                                             if (currentUserTerms.Any(n => n.Key.Id == m.Term.Id))
                                                                                                 return currentUserTerms.Any(n =>
                                                                                                                             {
                                                                                                                                 if (n.Key.Id == m.Term.Id)
                                                                                                                                     return n.Value != m.IsAccess;
                                                                                                                                 return false;
                                                                                                                             });
                                                                                             return true;
                                                                                         }).ToArray();
            foreach (var keyValuePair in currentUserTerms.Where(m =>
                                                                                    {
                                                                                        if (!input.UserGrants.Any(n =>
                                                                                                                  {
                                                                                                                      if (n.Term.Id == m.Key.Id)
                                                                                                                          return !n.IsCustom;
                                                                                                                      return false;
                                                                                                                  }))
                                                                                            return newUserTerms.Any(n =>
                                                                                                                    {
                                                                                                                        if (n.Term.Id == m.Key.Id && n.IsCustom)
                                                                                                                            return n.IsAccess != m.Value;
                                                                                                                        return false;
                                                                                                                    });
                                                                                        return true;
                                                                                    }).ToArray())
                _termTable.removeTermFromUser(input.UserId, keyValuePair.Key.Id);
            foreach (GrantUserTerm grantUserTerm in newUserTerms)
                _termTable.addTermToUser(input.UserId, grantUserTerm.Term.Id, grantUserTerm.IsAccess);
            return Task.FromResult(1);
        }

        public Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(int groupId)
        {
            var list = new List<GrantGroupTerm>();
            var allTerms = _termTable.GetAllTerms();
            var termsByGroup = _termTable.GetTermsByGroup(groupId);
            var listRoleDictionary = GetListRoleDictionary();
            foreach (Term term1 in allTerms)
            {
                Term term = term1;
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
                bool flag = termsByGroup.SingleOrDefault(m => m.Id == term.Id) != null;
                list.Add(new GrantGroupTerm
                         {
                    Term = term,
                    IsAccess = flag
                });
            }
            return Task.FromResult((IEnumerable<GrantGroupTerm>)list);
        }

        public Task<int> UpdateGroupGrant(UpdateGroupGrantInput input)
        {
            input.GroupGrants = input.GroupGrants ?? new GrantGroupTerm[0];
            IEnumerable<Term> currentGroupTerms = _termTable.GetTermsByGroup(input.GroupId);
            GrantGroupTerm[] grantGroupTermArray = input.GroupGrants.Where(m =>
                                                                          {
                                                                              if (m.IsAccess)
                                                                                  return currentGroupTerms.All(n => n.Id != m.Term.Id);
                                                                              return false;
                                                                          }).ToArray();
            foreach (var term in currentGroupTerms.Where(m => input.GroupGrants.Any(n =>
                                                                                     {
                                                                                         if (!n.IsAccess)
                                                                                             return n.Term.Id == m.Id;
                                                                                         return false;
                                                                                     })).ToArray())
                _termTable.removeTermFromGroup(input.GroupId, term.Id);
            foreach (GrantGroupTerm grantGroupTerm in grantGroupTermArray)
                _termTable.addTermToGroup(input.GroupId, grantGroupTerm.Term.Id);
            return Task.FromResult(1);
        }

        public async Task ReUpdateUserRole(int userId)
        {
            var currentUserRoles = await UserManager.GetRolesAsync(userId);
            var userTerms = _termTable.GetTermsByUser(userId);
            var userTermsInGroup = _termTable.GetGroupTermsBelongToUser(userId);
            var userRoles = userTermsInGroup.Where(m => !userTerms.Any(n =>
                                                                                    {
                                                                                        if (!n.Value)
                                                                                            return n.Key.Id == m.Id;
                                                                                        return false;
                                                                                    }));
            userRoles = userRoles.Union(userTerms.Where(m => m.Value).Select(m => m.Key), new TermComparer()).ToArray();
            string[] newUserRoles = userRoles.ToArray().Where(m => currentUserRoles.All(n => n != m.RoleKey)).Select(m => m.RoleKey).ToArray();
            string[] oldUserRoles = currentUserRoles.Where(m => !userRoles.Any(n => n.RoleKey == m)).ToArray();
            await UserManager.RemoveFromRolesAsync(userId, oldUserRoles);
            await UserManager.AddToRolesAsync(userId, newUserRoles);
        }

        public async Task ReUpdateGroupRole(int groupId)
        {
            IEnumerable<User> users = _userTable.GetUsersByGroup(groupId);
            foreach (var user in users)
                await ReUpdateUserRole(user.Id);
        }

        public async Task SynchTermsToRoles()
        {
            var currentRoles = RoleManager.Roles.ToArray();
            IEnumerable<Term> terms = _termTable.GetAllTerms();
            string[] newRoles = terms.Where(m => currentRoles.All(n => n.Name != m.RoleKey)).Select(m => m.RoleKey).ToArray();
            ApplicationRole[] oldRoles = currentRoles.Where(m => !terms.Any(n => n.RoleKey == m.Name)).ToArray();
            foreach (ApplicationRole role in oldRoles)
            {
                await RoleManager.DeleteAsync(role);
            }
            foreach (string name in newRoles)
            {
                await RoleManager.CreateAsync(new ApplicationRole(name));
            }
        }

        public class TermComparer : IEqualityComparer<Term>
        {
            public bool Equals(Term item1, Term item2)
            {
                if (item1 == null && item2 == null)
                    return true;
                if (item1 != null && item2 == null || item1 == null && item2 != null)
                    return false;
                return item1.Id == item2.Id;
            }

            public int GetHashCode(Term item)
            {
                return new
                {
                    item.Id
                }.GetHashCode();
            }
        }
    }
}