using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class LoginHistoryBll : ILoginHistoryBll
    {
        private ILoginHistoryTable _loginHistoryTable;

        public MySQLDatabase Database { get; private set; }

        public LoginHistoryBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._loginHistoryTable = (ILoginHistoryTable)new LoginHistoryTable(this.Database);
        }

        public Task<LoginHistory> GetOneLoginHistory(int loginHistoryId)
        {
            return Task.FromResult<LoginHistory>(this._loginHistoryTable.GetOneLoginHistory(loginHistoryId));
        }

        public Task<DanhSachLoginHistoryOutput> GetAll(FilterLoginHistoryInput input)
        {
            int total = this._loginHistoryTable.GetTotal(input);
            IEnumerable<LoginHistory> paging = this._loginHistoryTable.GetPaging(input);
            return Task.FromResult<DanhSachLoginHistoryOutput>(new DanhSachLoginHistoryOutput()
            {
                DanhSachLoginHistories = paging,
                TotalCount = (long)total
            });
        }

        public Task<int> DeleteLoginHistory(int loginHistoryId)
        {
            return Task.FromResult<int>(this._loginHistoryTable.Delete(loginHistoryId));
        }

        public Task<int> DeleteLoginHistory(IEnumerable<int> Ids)
        {
            return Task.FromResult<int>(this._loginHistoryTable.Delete(Ids));
        }

        public Task<int> InsertLoginHistory(InsertLoginHistoryInput input)
        {
            Mapper.CreateMap<InsertLoginHistoryInput, LoginHistory>();
            return Task.FromResult<int>(this._loginHistoryTable.InsertHistory(Mapper.Map<LoginHistory>((object)input)));
        }

        public Task<long> CountSuccessLoggedIn(string username)
        {
            return Task.FromResult<long>(this._loginHistoryTable.CountSuccessLoggedIn(username));
        }
    }
}