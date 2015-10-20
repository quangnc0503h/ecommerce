using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Cate.Api.Models;
using Quang.Common.Auth;
using StackExchange.Exceptional;

namespace Quang.Cate.Api.Controllers
{
    [RoutePrefix("api/language")]
    public class LanguageController : ApiController
    {
        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Users)]
        [Route("GetAll")]
        public async Task<DataSourceResult> GetAll(DataSourceRequest command)
        {
            try
            {
                var gridModel = new DataSourceResult();

                return await Task.FromResult(gridModel);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResult();
            }
        }
    }
}
