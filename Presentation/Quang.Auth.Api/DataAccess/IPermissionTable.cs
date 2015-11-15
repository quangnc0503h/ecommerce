using System.Collections.Generic;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
    public interface IPermissionTable
    {
        Permission GetOnePermission(int permissionId);

        int Delete(int permissionId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Permission permission);

        int Update(Permission permission);

        int GetTotal(string keyword);

        IEnumerable<Permission> GetPaging(int pageSize, int pageNumber, string keyword);

        IEnumerable<Permission> GetAllPermissions();

        IEnumerable<PermissionGrant> GetPermissionGrants(int permissionId);

        int UpdatePermissionGrants(int permissionId, IEnumerable<PermissionGrant> permissionGrant);

        IEnumerable<Permission> GetUserPermissions(int userId);

        IEnumerable<Permission> GetGroupPermissions(int groupId);

        int UpdateUserPermissions(int userId, IEnumerable<int> permissionIds);

        int UpdateGroupPermissions(int groupId, IEnumerable<int> permissionIds);

        IEnumerable<PermissionGrant> GetAllPermissionGrantBelongToUser(int userId);

        IEnumerable<int> GetAllUserIdHasPermission(int permissionId);

        int DeleteUserPermissions(int userId, IEnumerable<int> permissionIds);

        int DeleteUserPermissions(int userId);

        int DeleteGroupPermissions(int groupId, IEnumerable<int> permissionIds);

        int DeleteGroupPermissions(int groupId);

        int DeletePermissionGrants(int permissionId);
    }
}