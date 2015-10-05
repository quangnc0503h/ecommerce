using Quang.Auth.Api.Models;
using Quang.Auth.BusinessLogic;
using Quang.Auth.Entities;
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
    [RoutePrefix("api/Term")]
    public class TermController : ApiController
    {
        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetAll")]
        public async Task<DataSourceResultModel> GetAll(FilterInputModel filter)
        {
            try
            {
                var model = new DataSourceResultModel
                {
                    Total = await TermBll.GetTotal(filter.Keyword),
                    Data = await TermBll.GetPaging(filter.PageSize, filter.PageNumber, filter.Keyword)
                };
                return model;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
          //  return await _termBll.GetAll(filter);
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetGrantedUsersByTerm")]
        public async Task<DataSourceResultModel> GetGrantedUsersByTerm(GetOneInputModel input)
        {
            try
            {
                return new DataSourceResultModel { Data= await TermBll.GetGrantedUsersByTerm(input.Id) };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }

       

        [HttpGet]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetMissingTerms")]
        public async Task<DataSourceResultModel> GetMissingTerms()
        {
            try
            {
                return new DataSourceResultModel { Data = await TermBll.GetMissingTerms() };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetOneTerm")]
        public async Task<TermModel> GetOneTerm(GetOneInputModel input)
        {
            try
            {
                var result = await TermBll.GetOneTerm(input.Id);
                var model = new TermModel {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description, RoleKey = result.RoleKey
                };
                return model;
            }
            catch(Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
                return new TermModel();
            }
           
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("CreateTerm")]
        public async Task<NotificationResultModel> CreateTerm(TermModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var entity = new Term
                {
                    Id  = input.Id, Description = input.Description, Name = input.Name, RoleKey = input.RoleKey
                };
                var test = await TermBll.Insert(entity);
                if (test > 0)
                {
                    await TermBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return result;
            }
         
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("UpdateTerm")]
        public async Task<NotificationResultModel> UpdateTerm(TermModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var entity = new Term
                {
                    Id = input.Id,
                    Description = input.Description,
                    Name = input.Name,
                    RoleKey = input.RoleKey
                };
                var test = await TermBll.Update(entity);
                if (test > 0)
                {
                    await TermBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return result;
            }
          
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("DeleteTerm")]
        public async Task<NotificationResultModel> DeleteTerm(DeleteInputModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var test = await TermBll.Delete(input.Ids);
                if (test > 0)
                {
                    await TermBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return result;
            }

           
        }
        [HttpGet]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("ListRoleOptions")]
        public  Task<DataSourceResultModel> ListRoleOptions()
        {
            try
            {
                var result =  TermBll.GetListRoleOptions();

                return Task.FromResult(new DataSourceResultModel { Data = result });
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return Task.FromResult(new DataSourceResultModel());
            }
         
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("GetGrantTermsUser")]
        public async Task<DataSourceResultModel> GetGrantTermsUser(GetOneInputModel input)
        {
            try
            {
                var grantTermsUser = await TermBll.GetGrantTermsUser(input.Id);

                return new DataSourceResultModel { Data = grantTermsUser };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
     
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("UpdateUserGrant")]
        public async Task<NotificationResultModel> UpdateUserGrant(UserGrantModel input)
        {
            
            var success = await TermBll.UpdateUserGrant(input.UserId,  input.UserGrants);
            var result = new NotificationResultModel { Status = 1 };
            if (success > 0)
            {
                //await _termBll.ReUpdateUserRole(input.UserId);
                result.Status = 0;
            }
            return result;
        }
        [HttpPost]
    //    [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("GetGrantTermsGroup")]
        public async Task<DataSourceResultModel> GetGrantTermsGroup(GetOneInputModel input)
        {
            try
            {
                var grantTermsGroup = await TermBll.GetGrantTermsGroup(input.Id);

                return new DataSourceResultModel { Data = grantTermsGroup };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }

        [HttpPost]
    //    [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("UpdateGroupGrant")]
        public async Task<NotificationResultModel> UpdateGroupGrant(GroupGrantModel input)
        {
            var success = await TermBll.UpdateGroupGrant(input.GroupId, input.GroupGrants);
            var result = new NotificationResultModel { Status = 1 };
            if (success > 0)
            {
                //await _termBll.ReUpdateGroupRole(input.GroupId);
                result.Status = 0;
            }
            return result;
        }
    }
}
