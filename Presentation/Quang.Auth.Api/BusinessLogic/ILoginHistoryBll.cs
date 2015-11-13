using System.Collections.Generic;
using System.Threading.Tasks;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface ILoginHistoryBll
    {
        Task<LoginHistory> GetOneLoginHistory(int loginHistoryId);

        Task<DanhSachLoginHistoryOutput> GetAll(FilterLoginHistoryInput input);

        Task<int> DeleteLoginHistory(int loginHistoryId);

        Task<int> DeleteLoginHistory(IEnumerable<int> Ids);

        Task<int> InsertLoginHistory(InsertLoginHistoryInput input);

        Task<long> CountSuccessLoggedIn(string username);
    }
}