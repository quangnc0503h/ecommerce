using System.Collections.Generic;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
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