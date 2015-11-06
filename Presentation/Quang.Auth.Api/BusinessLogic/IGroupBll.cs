using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface IGroupBll
    {
        Task<Group> GetOneGroup(int groupId);

        Task<DanhSachGroupOutput> GetAll(FilterGroupInput input);

        Task<DanhSachGroupOutput> GetAllWithTree(FilterGroupInput input);

        Task<int> DeleteGroup(IEnumerable<int> Ids);

        Task<int> InsertGroup(CreateGroupInput input);

        Task<int> UpdateGroup(UpdateGroupInput input);

        Task<IList<Group>> GetListGroupOptions();

        Task<IList<Group>> GetListGroupOptions(int? parentId);
    }
    public interface ILoginHistoryBll
    {
        Task<LoginHistory> GetOneLoginHistory(int loginHistoryId);

        Task<DanhSachLoginHistoryOutput> GetAll(FilterLoginHistoryInput input);

        Task<int> DeleteLoginHistory(int loginHistoryId);

        Task<int> DeleteLoginHistory(IEnumerable<int> Ids);

        Task<int> InsertLoginHistory(InsertLoginHistoryInput input);

        Task<long> CountSuccessLoggedIn(string username);
    }
    public interface IPermissionBll
    {
        Task<Permission> GetOnePermission(int permissionId);

        Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput input);

        Task<int> DeletePermission(IEnumerable<int> Ids);

        Task<int> InsertPermission(CreatePermissionInput input);

        Task<int> UpdatePermission(UpdatePermissionInput input);

        Task<IEnumerable<Permission>> GetAllPermissions();

        Task<GetPermissionGrantsOutput> GetPermissionGrants(int permissionId);

        Task<int> UpdatePermissionGrants(UpdatePermissionGrantsInput input);

        Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(int userId);

        Task<int> UpdateUserPermissions(UpdateUserPermissionInput input);

        Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(int groupId);

        Task<int> UpdateGroupPermissions(UpdateGroupPermissionInput input);

        Task<int> GenerateRolesForGroup(int groupId);

        Task<int> GenerateRolesForUser(int userId);

        Task<int> GenerateRolesForUserByPermission(int permissionId);

        Task<int> ReUpdateAllUserRoles();
    }
    public interface IRefreshTokenBll
    {
        Task<Client> GetOneClient(string clientId);

        Task<IEnumerable<RefreshToken>> GetAllRefreshTokens();

        Task<RefreshToken> GetOneRefreshToken(string refreshTokenId);

        Task<RefreshToken> FindRefreshToken(string clientId, string subject);

        Task<int> AddRefreshToken(RefreshToken token);

        Task<int> RemoveRefreshToken(string refreshTokenId);

        Task<int> InsertRefreshToken(RefreshToken token);
    }
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