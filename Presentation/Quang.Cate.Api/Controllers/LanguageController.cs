﻿using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Cate.Api.Models;
using Quang.Cate.Api.Models.Localization;
using Quang.Cate.BusinessLogic;
using Quang.Cate.Entities;
using Quang.Common.Auth;
using StackExchange.Exceptional;
using System.Collections.Generic;
//using System.Web.Mvc;

namespace Quang.Cate.Api.Controllers
{
    [RoutePrefix("api/language")]
    //[AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
    public class LanguageController : ApiController
    {
        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("GetAll")]
        public async Task<DataSourceResult> GetAll(DataSourceRequest command)
        {
            try
            {
                
                IEnumerable languages = await LanguageBll.GetPaging(command.PageSize, command.PageNumber, "", command.Keyword);
                var gridModel = new DataSourceResult
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
                                Published = item.Published,
                                UniqueSeoCode = item.UniqueSeoCode
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
        [Route("CreateLanguage")]
        public async Task<NotificationResultModel> CreateLanguage(LanguageModel input)
        {
            try
            {
                var model = new NotificationResultModel { Status = 1};
                var language = new Language
                               {
                                   Name = input.Name,
                                   DisplayOrder = input.DisplayOrder,
                                   LanguageCulture = input.LanguageCulture,
                                   Published = input.Published,
                                   UniqueSeoCode = input.UniqueSeoCode
                               };
                model.Status = ((int)(await LanguageBll.Insert(language))) > 0? 0 : 1;
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
                var language = new Language
                               {
                                   Id = input.Id,
                                   DisplayOrder = input.DisplayOrder,
                                   LanguageCulture = input.LanguageCulture,
                                   Name = input.Name,
                                   Published = input.Published,
                                   UniqueSeoCode = input.UniqueSeoCode
                               };
                model.Status = ((int) (await LanguageBll.Update(language))) > 0? 0 : 1;
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.DanhMuc.Languages)]
        [Route("DeleteLanguage")]
        public async Task<NotificationResultModel> DeleteLanguage(DeleteInputModel input)
        {
            try
            {
                var model = new NotificationResultModel { Status = 1};
                model.Status = ((int) (await LanguageBll.Delete(input.Ids))) > 0? 0 : 1;
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }

        }

        public async Task<DataSourceResult> GetCultures()
        {
            try
            {
                List<SelectListItemModel> data = new List<SelectListItemModel>();
                var cultures = System.Globalization.CultureInfo.GetCultures(
                    System.Globalization.CultureTypes.SpecificCultures)
                    .OrderBy(x => x.EnglishName);
                foreach(var item in cultures)
                {
                    var model = new SelectListItemModel {
                        Text = item.EnglishName,
                        Value = item.IetfLanguageTag
                    };
                    data.Add(model);
                }
                var gridModel = new DataSourceResult
                {
                    Data = data,
                    Total = cultures.Count()
                };
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
