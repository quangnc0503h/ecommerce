using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class LoginHistoryBll : ILoginHistoryBll
    {
        private readonly ILoginHistoryTable _loginHistoryTable;

        public MySQLDatabase Database { get; private set; }

        public LoginHistoryBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _loginHistoryTable = new LoginHistoryTable(Database);
        }

        public Task<LoginHistory> GetOneLoginHistory(int loginHistoryId)
        {
            return Task.FromResult(_loginHistoryTable.GetOneLoginHistory(loginHistoryId));
        }

        public Task<DanhSachLoginHistoryOutput> GetAll(FilterLoginHistoryInput input)
        {
            int total = _loginHistoryTable.GetTotal(input);
            IEnumerable<LoginHistory> paging = _loginHistoryTable.GetPaging(input);
            return Task.FromResult(new DanhSachLoginHistoryOutput
                                   {
                DanhSachLoginHistories = paging,
                TotalCount = total
            });
        }

        public Task<int> DeleteLoginHistory(int loginHistoryId)
        {
            return Task.FromResult(_loginHistoryTable.Delete(loginHistoryId));
        }

        public Task<int> DeleteLoginHistory(IEnumerable<int> Ids)
        {
            return Task.FromResult(_loginHistoryTable.Delete(Ids));
        }

        public Task<int> InsertLoginHistory(InsertLoginHistoryInput input)
        {
            Mapper.CreateMap<InsertLoginHistoryInput, LoginHistory>();
            return Task.FromResult(_loginHistoryTable.InsertHistory(Mapper.Map<LoginHistory>(input)));
        }

        public Task<long> CountSuccessLoggedIn(string username)
        {
            return Task.FromResult(_loginHistoryTable.CountSuccessLoggedIn(username));
        }
    }
}