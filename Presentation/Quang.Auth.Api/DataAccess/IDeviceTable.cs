using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public interface IDeviceTable
    {
        Device GetOneDevice(int deviceId);

        int Delete(int deviceId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Device device);

        int Update(Device device);

        IEnumerable<Device> GetAllDevices();

        int GetTotal(string clientId, string keyword);

        IEnumerable<Device> GetPaging(int pageSize, int pageNumber, string clientId, string keyword);

        Device GetDevice(string clientId, string deviceKey);

        Device GetDevice(string clientId, string deviceKey, string deviceSecret);

        IEnumerable<Client> GetAllClients();
    }
    public interface IGroupTable
    {
        Group GetOneGroup(int groupId);

        int Delete(int groupId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Group group);

        int Update(Group group);

        IDictionary<int, Group> GetAllGroups();

        int GetTotal(int? parentId, string keyword);

        IEnumerable<Group> GetPaging(int pageSize, int pageNumber, int? parentId, string keyword);
    }
    public interface ILoginHistoryTable
    {
        LoginHistory GetOneLoginHistory(int loginHistoryId);

        int Delete(int id);

        int Delete(IEnumerable<int> ids);

        int GetTotal(FilterLoginHistoryInput input);

        IEnumerable<LoginHistory> GetPaging(FilterLoginHistoryInput input);

        int InsertHistory(LoginHistory input);

        long CountSuccessLoggedIn(string username);
    }
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
    public interface IRefreshTokenTable
    {
        Client GetOneClient(string clientId);

        IEnumerable<RefreshToken> GetAllRefreshTokens();

        RefreshToken GetOneRefreshToken(string refreshTokenId);

        RefreshToken FindRefreshToken(string clientId, string subject);

        int AddRefreshToken(RefreshToken token);

        int RemoveRefreshToken(string refreshTokenId);

        int InsertRefreshToken(RefreshToken token);
    }
    public interface IRequestDeviceTable
    {
        int Delete(int id);

        int Delete(IEnumerable<int> ids);

        int GetTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo);

        IEnumerable<RequestDevice> GetPaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo);

        int CreateRequestDevice(InformNewAppInput newApp);

        int UpdateRequestDevice(InformNewAppInput newApp);

        int UpdateRequestDevice(InformNewAppInput newApp, bool isApproved);

        int UpdateRequestDeviceStatus(string clientId, string deviceKey, bool isApproved);

        bool IsExistRequestDevice(string clientId, string deviceKey);

        RequestDevice GetRequestDevice(string clientId, string deviceKey);
    }
    public interface ITermTable
    {
        Term GetOneTerm(int termId);

        int Delete(int termId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Term term);

        int Update(Term term);

        IEnumerable<Term> GetAllTerms();

        int GetTotal(string keyword);

        IEnumerable<Term> GetPaging(int pageSize, int pageNumber, string keyword);

        IEnumerable<Term> GetTermsByGroup(int groupId);

        IDictionary<Term, bool> GetTermsByUser(int userId);

        IDictionary<string, bool> GetUsersByTerm(int termId);

        IEnumerable<Term> GetGroupTermsBelongToUser(int userId);

        IEnumerable<string> GetGroupUsersBelongToTerm(int termId);

        IDictionary<Term, bool> GetUserTermsWithGroupAccess(int userId);

        int addTermToUser(int userId, int termId, bool isAccess);

        int removeTermFromUser(int userId, int termId);

        int addTermToGroup(int groupId, int termId);

        int removeTermFromGroup(int groupId, int termId);
    }
    public interface IUserTable
    {
        User GetOneUser(int userId);

        IEnumerable<User> GetAllUsers();

        IEnumerable<User> GetUsersByGroup(int groupId);

        IEnumerable<User> GetUsersByRole(int roleId);

        int GetTotal(int? groupId, string keyword);

        IEnumerable<User> GetPaging(int pageSize, int pageNumber, int? groupId, string keyword);

        IEnumerable<Group> GetGroupsByUser(int userId);

        int addUserToGroup(int groupId, int userId);

        int removeUserFromGroup(int groupId, int userId);

        UserApp GetUserApp(int userId, AppApiType appType);

        UserApp GetUserApp(string userApiKey, bool? isActive);

        int InsertUserApp(UserApp userApp);

        int UpdateUserApp(UserApp userApp);
    }
}