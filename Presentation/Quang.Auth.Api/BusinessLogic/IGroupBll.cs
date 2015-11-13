using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
}