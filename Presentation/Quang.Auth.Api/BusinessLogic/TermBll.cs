using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class TermBll : ITermBll
    {
        private ITermTable _termTable;
        private IUserTable _userTable;
        private GroupTable _groupTable;

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

        public TermBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._termTable = (ITermTable)new TermTable(this.Database);
            this._userTable = (IUserTable)new UserTable(this.Database);
            this._groupTable = new GroupTable(this.Database);
        }

        public Task<Term> GetOneTerm(int termId)
        {
            Term term = this._termTable.GetOneTerm(termId);
            if (term != null)
            {
                ActionRoleItem actionRoleItem = Enumerable.FirstOrDefault<ActionRoleItem>(Enumerable.Where<ActionRoleItem>(this.GetListRoleOptions(), (Func<ActionRoleItem, bool>)(m => m.RoleKey == term.RoleKey)));
                if (actionRoleItem != null)
                    term.RoleKeyLabel = actionRoleItem.RoleKeyLabel;
            }
            return Task.FromResult<Term>(term);
        }

        public Task<DanhSachTermOutput> GetAll(FilterTermInput input)
        {
            int total = this._termTable.GetTotal(input.Keyword);
            IEnumerable<Term> paging = this._termTable.GetPaging(input.PageSize, input.PageNumber, input.Keyword);
            IDictionary<string, ActionRoleItem> listRoleDictionary = this.GetListRoleDictionary();
            foreach (Term term in paging)
            {
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
            }
            return Task.FromResult<DanhSachTermOutput>(new DanhSachTermOutput()
            {
                DanhSachTerms = paging,
                TotalCount = (long)total
            });
        }

        public Task<IEnumerable<Term>> GetAllTerms()
        {
            IEnumerable<Term> allTerms = this._termTable.GetAllTerms();
            IDictionary<string, ActionRoleItem> listRoleDictionary = this.GetListRoleDictionary();
            foreach (Term term in allTerms)
            {
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
            }
            return Task.FromResult<IEnumerable<Term>>(allTerms);
        }

        public async Task<IEnumerable<Quang.Auth.Entities.User>> GetGrantedUsersByTerm(int termId)
        {
            IEnumerable<Quang.Auth.Entities.User> users = (IEnumerable<Quang.Auth.Entities.User>)new List<Quang.Auth.Entities.User>();
            Term term = this._termTable.GetOneTerm(termId);
            if (term != null)
            {
                ApplicationRole role = await this.RoleManager.FindByNameAsync(term.RoleKey);
                if (role != null)
                    users = this._userTable.GetUsersByRole(role.Id);
            }
            return users;
        }

        public Task<IEnumerable<ActionRoleItem>> GetMissingTerms()
        {
            IEnumerable<Term> terms = this._termTable.GetAllTerms();
            return Task.FromResult<IEnumerable<ActionRoleItem>>((IEnumerable<ActionRoleItem>)Enumerable.ToArray<ActionRoleItem>(Enumerable.Where<ActionRoleItem>(this.GetListRoleOptions(), (Func<ActionRoleItem, bool>)(m => !Enumerable.Any<Term>(terms, (Func<Term, bool>)(n => n.RoleKey == m.RoleKey))))));
        }

        public Task<int> DeleteTerm(int termId)
        {
            return Task.FromResult<int>(this._termTable.Delete(termId));
        }

        public Task<int> DeleteTerm(IEnumerable<int> Ids)
        {
            return Task.FromResult<int>(this._termTable.Delete(Ids));
        }

        public Task<int> InsertTerm(CreateTermInput input)
        {
            Mapper.CreateMap<CreateTermInput, Term>();
            return Task.FromResult<int>(this._termTable.Insert(Mapper.Map<Term>((object)input)));
        }

        public Task<int> UpdateTerm(UpdateTermInput input)
        {
            Mapper.CreateMap<UpdateTermInput, Term>();
            return Task.FromResult<int>(this._termTable.Update(Mapper.Map<Term>((object)input)));
        }

        public IEnumerable<ActionRoleItem> GetListRoleOptions()
        {
            return (IEnumerable<ActionRoleItem>)Enumerable.ToArray<ActionRoleItem>(Enumerable.Select<KeyValuePair<string, ActionRoleItem>, ActionRoleItem>((IEnumerable<KeyValuePair<string, ActionRoleItem>>)this.GetListRoleDictionary(), (Func<KeyValuePair<string, ActionRoleItem>, ActionRoleItem>)(m => m.Value)));
        }

        public IDictionary<string, ActionRoleItem> GetListRoleDictionary()
        {
            return ActionRole.ToListDictionary();
        }

        public Task<IEnumerable<Term>> GetTermsByGroup(int groupId)
        {
            return Task.FromResult<IEnumerable<Term>>(this._termTable.GetTermsByGroup(groupId));
        }

        public Task<IDictionary<Term, bool>> GetTermsByUser(int userId)
        {
            return Task.FromResult<IDictionary<Term, bool>>(this._termTable.GetTermsByUser(userId));
        }

        public async Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(string userId)
        {
            IList<GrantUserTerm> items = (IList<GrantUserTerm>)new List<GrantUserTerm>();
            ApplicationUser user = (ApplicationUser)null;
            if (user == null && !string.IsNullOrEmpty(userId) && Regex.IsMatch(userId, "^[1-9]([0-9]*)$"))
                user = await this.UserManager.FindByIdAsync(int.Parse(userId));
            if (user == null && !string.IsNullOrEmpty(userId))
                user = await this.UserManager.FindByNameAsync(userId);
            if (user != null)
            {
                IDictionary<Term, bool> termsWithGroupAccess = this._termTable.GetUserTermsWithGroupAccess(user.Id);
                IDictionary<Term, bool> termsByUser = this._termTable.GetTermsByUser(user.Id);
                IDictionary<string, ActionRoleItem> listRoleDictionary = this.GetListRoleDictionary();
                foreach (KeyValuePair<Term, bool> keyValuePair1 in (IEnumerable<KeyValuePair<Term, bool>>)termsWithGroupAccess)
                {
                    KeyValuePair<Term, bool> term = keyValuePair1;
                    if (listRoleDictionary.ContainsKey(term.Key.RoleKey) && listRoleDictionary[term.Key.RoleKey] != null)
                        term.Key.RoleKeyLabel = listRoleDictionary[term.Key.RoleKey].RoleKeyLabel;
                    KeyValuePair<Term, bool> keyValuePair2 = Enumerable.SingleOrDefault<KeyValuePair<Term, bool>>(Enumerable.Where<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)termsByUser, (Func<KeyValuePair<Term, bool>, bool>)(m => m.Key.Id == term.Key.Id)));
                    bool flag1 = keyValuePair2.Key != null;
                    bool flag2 = flag1 && keyValuePair2.Value;
                    items.Add(new GrantUserTerm()
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
            input.UserGrants = input.UserGrants ?? (IEnumerable<GrantUserTerm>)new GrantUserTerm[0];
            IDictionary<Term, bool> currentUserTerms = this._termTable.GetTermsByUser(input.UserId);
            GrantUserTerm[] newUserTerms = Enumerable.ToArray<GrantUserTerm>(Enumerable.Where<GrantUserTerm>(Enumerable.Where<GrantUserTerm>(input.UserGrants, (Func<GrantUserTerm, bool>)(m => m.IsCustom)), (Func<GrantUserTerm, bool>)(m =>
            {
                if (Enumerable.Any<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)currentUserTerms, (Func<KeyValuePair<Term, bool>, bool>)(n => n.Key.Id == m.Term.Id)))
                    return Enumerable.Any<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)currentUserTerms, (Func<KeyValuePair<Term, bool>, bool>)(n =>
                    {
                        if (n.Key.Id == m.Term.Id)
                            return n.Value != m.IsAccess;
                        return false;
                    }));
                return true;
            })));
            foreach (KeyValuePair<Term, bool> keyValuePair in Enumerable.ToArray<KeyValuePair<Term, bool>>(Enumerable.Where<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)currentUserTerms, (Func<KeyValuePair<Term, bool>, bool>)(m =>
            {
                if (!Enumerable.Any<GrantUserTerm>(input.UserGrants, (Func<GrantUserTerm, bool>)(n =>
                {
                    if (n.Term.Id == m.Key.Id)
                        return !n.IsCustom;
                    return false;
                })))
                    return Enumerable.Any<GrantUserTerm>((IEnumerable<GrantUserTerm>)newUserTerms, (Func<GrantUserTerm, bool>)(n =>
                    {
                        if (n.Term.Id == m.Key.Id && n.IsCustom)
                            return n.IsAccess != m.Value;
                        return false;
                    }));
                return true;
            }))))
                this._termTable.removeTermFromUser(input.UserId, keyValuePair.Key.Id);
            foreach (GrantUserTerm grantUserTerm in newUserTerms)
                this._termTable.addTermToUser(input.UserId, grantUserTerm.Term.Id, grantUserTerm.IsAccess);
            return Task.FromResult<int>(1);
        }

        public Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(int groupId)
        {
            IList<GrantGroupTerm> list = (IList<GrantGroupTerm>)new List<GrantGroupTerm>();
            IEnumerable<Term> allTerms = this._termTable.GetAllTerms();
            IEnumerable<Term> termsByGroup = this._termTable.GetTermsByGroup(groupId);
            IDictionary<string, ActionRoleItem> listRoleDictionary = this.GetListRoleDictionary();
            foreach (Term term1 in allTerms)
            {
                Term term = term1;
                if (listRoleDictionary.ContainsKey(term.RoleKey) && listRoleDictionary[term.RoleKey] != null)
                    term.RoleKeyLabel = listRoleDictionary[term.RoleKey].RoleKeyLabel;
                bool flag = Enumerable.SingleOrDefault<Term>(Enumerable.Where<Term>(termsByGroup, (Func<Term, bool>)(m => m.Id == term.Id))) != null;
                list.Add(new GrantGroupTerm()
                {
                    Term = term,
                    IsAccess = flag
                });
            }
            return Task.FromResult<IEnumerable<GrantGroupTerm>>((IEnumerable<GrantGroupTerm>)list);
        }

        public Task<int> UpdateGroupGrant(UpdateGroupGrantInput input)
        {
            input.GroupGrants = input.GroupGrants ?? (IEnumerable<GrantGroupTerm>)new GrantGroupTerm[0];
            IEnumerable<Term> currentGroupTerms = this._termTable.GetTermsByGroup(input.GroupId);
            GrantGroupTerm[] grantGroupTermArray = Enumerable.ToArray<GrantGroupTerm>(Enumerable.Where<GrantGroupTerm>(input.GroupGrants, (Func<GrantGroupTerm, bool>)(m =>
            {
                if (m.IsAccess)
                    return !Enumerable.Any<Term>(currentGroupTerms, (Func<Term, bool>)(n => n.Id == m.Term.Id));
                return false;
            })));
            foreach (Term term in Enumerable.ToArray<Term>(Enumerable.Where<Term>(currentGroupTerms, (Func<Term, bool>)(m => Enumerable.Any<GrantGroupTerm>(input.GroupGrants, (Func<GrantGroupTerm, bool>)(n =>
            {
                if (!n.IsAccess)
                    return n.Term.Id == m.Id;
                return false;
            }))))))
                this._termTable.removeTermFromGroup(input.GroupId, term.Id);
            foreach (GrantGroupTerm grantGroupTerm in grantGroupTermArray)
                this._termTable.addTermToGroup(input.GroupId, grantGroupTerm.Term.Id);
            return Task.FromResult<int>(1);
        }

        public async Task ReUpdateUserRole(int userId)
        {
            IList<string> currentUserRoles = await this.UserManager.GetRolesAsync(userId);
            IDictionary<Term, bool> userTerms = this._termTable.GetTermsByUser(userId);
            IEnumerable<Term> userTermsInGroup = this._termTable.GetGroupTermsBelongToUser(userId);
            IEnumerable<Term> userRoles = Enumerable.Where<Term>(userTermsInGroup, (Func<Term, bool>)(m => !Enumerable.Any<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)userTerms, (Func<KeyValuePair<Term, bool>, bool>)(n =>
            {
                if (!n.Value)
                    return n.Key.Id == m.Id;
                return false;
            }))));
            userRoles = (IEnumerable<Term>)Enumerable.ToArray<Term>(Enumerable.Union<Term>(userRoles, Enumerable.Select<KeyValuePair<Term, bool>, Term>(Enumerable.Where<KeyValuePair<Term, bool>>((IEnumerable<KeyValuePair<Term, bool>>)userTerms, (Func<KeyValuePair<Term, bool>, bool>)(m => m.Value)), (Func<KeyValuePair<Term, bool>, Term>)(m => m.Key)), (IEqualityComparer<Term>)new TermBll.TermComparer()));
            string[] newUserRoles = Enumerable.ToArray<string>(Enumerable.Select<Term, string>(Enumerable.Where<Term>(userRoles, (Func<Term, bool>)(m => !Enumerable.Any<string>((IEnumerable<string>)currentUserRoles, (Func<string, bool>)(n => n == m.RoleKey)))), (Func<Term, string>)(m => m.RoleKey)));
            string[] oldUserRoles = Enumerable.ToArray<string>(Enumerable.Where<string>((IEnumerable<string>)currentUserRoles, (Func<string, bool>)(m => !Enumerable.Any<Term>(userRoles, (Func<Term, bool>)(n => n.RoleKey == m)))));
            IdentityResult identityResult1 = await this.UserManager.RemoveFromRolesAsync(userId, oldUserRoles);
            IdentityResult identityResult2 = await this.UserManager.AddToRolesAsync(userId, newUserRoles);
        }

        public async Task ReUpdateGroupRole(int groupId)
        {
            IEnumerable<Quang.Auth.Entities.User> users = this._userTable.GetUsersByGroup(groupId);
            foreach (Quang.Auth.Entities.User user in users)
                await this.ReUpdateUserRole(user.Id);
        }

        public async Task SynchTermsToRoles()
        {
            ApplicationRole[] currentRoles = Enumerable.ToArray<ApplicationRole>((IEnumerable<ApplicationRole>)this.RoleManager.Roles);
            IEnumerable<Term> terms = this._termTable.GetAllTerms();
            string[] newRoles = Enumerable.ToArray<string>(Enumerable.Select<Term, string>(Enumerable.Where<Term>(terms, (Func<Term, bool>)(m => !Enumerable.Any<ApplicationRole>((IEnumerable<ApplicationRole>)currentRoles, (Func<ApplicationRole, bool>)(n => n.Name == m.RoleKey)))), (Func<Term, string>)(m => m.RoleKey)));
            ApplicationRole[] oldRoles = Enumerable.ToArray<ApplicationRole>(Enumerable.Where<ApplicationRole>((IEnumerable<ApplicationRole>)currentRoles, (Func<ApplicationRole, bool>)(m => !Enumerable.Any<Term>(terms, (Func<Term, bool>)(n => n.RoleKey == m.Name)))));
            foreach (ApplicationRole role in oldRoles)
            {
                IdentityResult identityResult = await this.RoleManager.DeleteAsync(role);
            }
            bool flag=false;
            int num1 = flag ? 1 : 0;
            foreach (string name in newRoles)
            {
                IdentityResult async = await this.RoleManager.CreateAsync(new ApplicationRole(name));
            }
            int num2 = flag ? 1 : 0;
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
                    Id = item.Id
                }.GetHashCode();
            }
        }
    }
}