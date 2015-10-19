using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Auth.Api.Models;
using Quang.Common.Auth;
using StackExchange.Exceptional;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/device")]
    public class DeviceController : BaseApiController
    {
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetAll")]
        public async Task<DataSourceResultModel> GetAll(FilterDeviceModel filter)
        {
            try
            {
                var model = new DataSourceResultModel {};
                return await Task.FromResult( model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetAllRequest")]
        public async Task<DataSourceResultModel> GetAllRequest(FilterRequestDeviceModel filter)
        {
            try
            {
                var model = new DataSourceResultModel { };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DataSourceResultModel();
            }
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetOneDevice")]
        public async Task<DeviceModel> GetOneDevice(GetOneInputModel input)
        {
            try
            {
                var model = new DeviceModel { };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DeviceModel();
            }
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetOneDeviceByKey")]
        public async Task<DeviceModel> GetOneDeviceByKey(string deviceKey)
        {
            try
            {
                var model = new DeviceModel { };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new DeviceModel();
            }
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("CreateDevice")]
        public async Task<NotificationResultModel> CreateDevice(DeviceModel input)
        {
            try
            {
                var model = new NotificationResultModel { };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("DeleteDevice")]
        public async Task<NotificationResultModel> DeleteDevice(DeleteInputModel input)
        {
            try
            {
                var model = new NotificationResultModel { };
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }

        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("DeleteRequestDevice")]
        public async Task<NotificationResultModel> DeleteRequestDevice(DeleteInputModel input)
        {
            try
            {
                var model = new NotificationResultModel { };
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
