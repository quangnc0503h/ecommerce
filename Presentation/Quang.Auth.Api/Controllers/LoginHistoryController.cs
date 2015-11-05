using Quang.Auth.Api.Models;
using Quang.Auth.BusinessLogic;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/LoginHistory")]
    public class LoginHistoryController : ApiController
    {
        [HttpPost]
        [Route("GetAll")]
        [AppAuthorize(Roles = "160")]
        public async Task<DataSourceResultModel> GetAll(FilterLoginHistoryModel filter)
        {
            try
            {
                var data = await LoginHistoryBll.GetPaging(filter.Type,filter.UserName, filter.LoginTimeFrom, filter.LoginTimeTo, filter.LoginStatus,filter.RefreshToken, filter.AppId, filter.ClientUri, filter.ClientIP, filter.ClientUA, filter.ClientDevice, filter.ClientApiKey,filter.PageSize, filter.PageNumber);
                var total = await LoginHistoryBll.GetTotal(filter.Type, filter.UserName, filter.LoginTimeFrom, filter.LoginTimeTo, filter.LoginStatus, filter.RefreshToken, filter.AppId, filter.ClientUri, filter.ClientIP, filter.ClientUA, filter.ClientDevice, filter.ClientApiKey);
                var model = new DataSourceResultModel
                {
                    Data = data,
                    Total = total
                };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }

        [HttpPost]
        [Route("GetOneLoginHistory")]
        [AppAuthorize(Roles = "160")]
        public async Task<LoginHistoryModel> GetOneLoginHistory(GetOneInputModel input)
        {
            LoginHistory result = await LoginHistoryBll.GetOneLoginHistory((int)input.Id);
            return new LoginHistoryModel()
            {
                LoginHistory = result
            };
        }
    }
}
