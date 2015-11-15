using System.Collections.Generic;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
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
}