using System.Collections.Generic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
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
}