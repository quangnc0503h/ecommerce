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
    //[Authorize]
    [RoutePrefix("api/Permission")]
    public class PermissionController : ApiController
    {
        [HttpPost]
        //[Authorize (Roles = ActionRole.HeThong.Permissions)]
        [Route("GetAll")]
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<DataSourceResultModel> GetAll(FilterPermissionModel filter)
        {
            try
            {
                var model = new DataSourceResultModel
                {
                    Total = await PermissionBll.GetTotal(filter.Keyword),
                    Data = await PermissionBll.GetPaging(filter.PageSize, filter.PageNumber, filter.Keyword)
                };
                return model;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
           
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetOnePermission")]
        public async Task<PermissionModel> GetOnePermission(GetOneInputModel input)
        {
            try
            {
                var result = await PermissionBll.GetOnePermission(input.Id);
                return new PermissionModel { Id = result.Id, Name = result.Name, Description=result.Description };
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new PermissionModel();
            }
            
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("UpdatePermission")]
        public async Task<NotificationResultModel> UpdatePermission(PermissionModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var entity = new Permission
                {
                    Id = input.Id, Name = input.Name, Description = input.Description
                };
                var test = await PermissionBll.Update(entity);
                if (test > 0)
                {
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
      //  [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("DeletePermission")]
        public async Task<NotificationResultModel> DeletePermission(DeleteInputModel input)
        {
            
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var test = await PermissionBll.DeletePermission(input.Ids);
                if (test > 0)
                {
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
        [Route("ListAllPermission")]
        public async Task<DataSourceResultModel> ListAllPermission()
        {
            try
            {
                var permissions = await PermissionBll.GetAllPermissions();
                var result = new DataSourceResultModel { Data = permissions };
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
            
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetPermissionGrants")]
        public async Task<GetPermissionGrantsOutputModel> GetPermissionGrants(GetOneInputModel input)
        {
            try
            {
                var grants = await PermissionBll.GetPermissionGrants(input.Id);
                var result = new GetPermissionGrantsOutputModel();
                result.AllowGrants = grants.Where(m => m.Type == 1).ToArray();
                result.DenyGrants = grants.Where(m => m.Type == 0).ToArray();

                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new GetPermissionGrantsOutputModel();
            }
           
        }
        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("UpdatePermissionGrants")]
        public async Task<NotificationResultModel> UpdatePermissionGrants(PermissionGrantsModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var test = await PermissionBll.UpdatePermissionGrants(input.PermissionId, input.AllowGrants, input.DenyGrants);
                if (test > 0)
                {
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
        //[AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("GetUserPermissions")]
        public async Task<DataSourceResultModel> GetUserPermissions(GetOneInputModel input)
        {
            try
            {
                var result = new DataSourceResultModel { Data = await PermissionBll.GetUserPermissions(input.Id) };
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
          
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("GetGroupPermissions")]
        public async Task<DataSourceResultModel> GetGroupPermissions(GetOneInputModel input)
        {
         

            try
            {
                var result = new DataSourceResultModel { Data = await PermissionBll.GetGroupPermissions(input.Id) };
                return result;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
        }

        [HttpPost]
     //   [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("UpdateUserPermissions")]
        public async Task<NotificationResultModel> UpdateUserPermissions(UserPermissionModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var status = await PermissionBll.UpdateUserPermissions(input.UserId, input.PermissionIds);
                if (status > 0)
                {
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
        //[AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("UpdateGroupPermissions")]
        public async Task<NotificationResultModel> UpdateGroupPermissions(GroupPermissionModel input)
        {
            var result = new NotificationResultModel { Status = 1 };
            try
            {
                var status = await PermissionBll.UpdateGroupPermissions(input.GroupId, input.PermissionIds);
                if (status > 0)
                {
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
    }
}
