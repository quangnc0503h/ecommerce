using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;

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
    public class LoginHistoryController : BaseApiController
    {
        private readonly ILoginHistoryBll _loginHistoryBll;

        public LoginHistoryController()
        {
        }

        public LoginHistoryController(ILoginHistoryBll loginHistoryBll)
        {
            this._loginHistoryBll = loginHistoryBll;
        }

        [HttpPost]
        [Route("GetAll")]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<DanhSachLoginHistoryOutput> GetAll(FilterLoginHistoryInput filter)
        {
            return await this._loginHistoryBll.GetAll(filter);
        }

        [HttpPost]
        [Route("GetOneLoginHistory")]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<GetOneLoginHistoryOutput> GetOneLoginHistory(GetByIdInput input)
        {
            LoginHistory result = await this._loginHistoryBll.GetOneLoginHistory(input.Id);
            return new GetOneLoginHistoryOutput()
            {
                LoginHistory = result
            };
        }
    }
}
