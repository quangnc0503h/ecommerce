using System.Collections.Generic;
using System.Threading.Tasks;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using Quang.Common.Auth;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface ITermBll
    {
        Task<Term> GetOneTerm(int termId);

        Task<DanhSachTermOutput> GetAll(FilterTermInput input);

        Task<IEnumerable<Term>> GetAllTerms();

        Task<IEnumerable<Quang.Auth.Entities.User>> GetGrantedUsersByTerm(int termId);

        Task<IEnumerable<ActionRoleItem>> GetMissingTerms();

        Task<int> DeleteTerm(int termId);

        Task<int> DeleteTerm(IEnumerable<int> Ids);

        Task<int> InsertTerm(CreateTermInput input);

        Task<int> UpdateTerm(UpdateTermInput input);

        IEnumerable<ActionRoleItem> GetListRoleOptions();

        IDictionary<string, ActionRoleItem> GetListRoleDictionary();

        Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(string userId);

        Task<int> UpdateUserGrant(UpdateUserGrantInput input);

        Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(int groupId);

        Task<int> UpdateGroupGrant(UpdateGroupGrantInput input);

        Task SynchTermsToRoles();
    }
}