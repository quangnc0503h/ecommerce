using System.Collections.Generic;
using System.Threading.Tasks;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.BusinessLogic
{
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
}