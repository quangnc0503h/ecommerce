using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Cate.Api.Models;
using Quang.Cate.Api.Models.Localization;
using Quang.Cate.BusinessLogic;
using Quang.Common.Auth;
using StackExchange.Exceptional;

namespace Quang.Cate.Api.Controllers
{
    [RoutePrefix("api/language")]
    public class LanguageController : ApiController
    {
        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("GetAll")]
        public async Task<DataSourceResult> GetAll(DataSourceRequest command)
        {
            try
            {
                
                IEnumerable languages = await LanguageBll.GetPaging(command.PageSize, command.Page, "", command.Keyword);
                var gridModel = new DataSourceResult()
                                {
                                    Data = languages,
                                    Total =(int)( await LanguageBll.GetTotal(command.Keyword))
                                };
                return await Task.FromResult(gridModel);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResult();
            }
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("GetOneLanguage")]
        public async Task<LanguageModel> GetOneLanguage(GetOneInputModel input)
        {
            try
            {
                var item = await LanguageBll.GetOneLanguage(input.Id);
               
                var model = new LanguageModel
                            {
                                Id = item.Id,
                                Name = item.Name,
                                DisplayOrder = item.DisplayOrder,
                                LanguageCulture = item.LanguageCulture,
                                Published = item.Published,UniqueSeoCode = item.UniqueSeoCode
                            };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new LanguageModel();
            }
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("CreateDevice")]
        public async Task<NotificationResultModel> CreateLanguage(LanguageModel input)
        {
            try
            {
                var model = new NotificationResultModel { Status = 1};
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("UpdateLanguage")]
        public async Task<NotificationResultModel> UpdateLanguage(LanguageModel input)
        {
            try
            {
                var model = new NotificationResultModel {Status = 1};
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
        }
    }
}
