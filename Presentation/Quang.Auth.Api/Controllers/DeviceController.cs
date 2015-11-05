using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Auth.Api.Models;
using Quang.Auth.BusinessLogic;
using Quang.Common.Auth;
using StackExchange.Exceptional;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/device")]
    public class DeviceController : BaseApiController
    {
        [HttpGet]
        [Route("GetAllClients")]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await DeviceBll.GetAllClients();
        }


        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetAll")]
        public async Task<DataSourceResultModel> GetAll(FilterDeviceModel filter)
        {
            try
            {
                
                var data = await DeviceBll.GetDevicePaging(filter.PageSize, filter.PageNumber, filter.ClientId, filter.Keyword);
                var total = await DeviceBll.GetDeviceTotal(filter.ClientId, filter.Keyword);
                var model = new DataSourceResultModel {
                    Data = data,
                    Total = total
                };
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
                var data = await DeviceBll.GetRequestDevicePaging(filter.PageSize, filter.PageNumber, filter.ClientId, filter.Keyword,filter.DateFrom, filter.DateTo);
                var total = await DeviceBll.GetRequestDeviceTotal(filter.ClientId, filter.Keyword, filter.DateFrom, filter.DateTo);
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
        [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetOneDevice")]
        public async Task<DeviceModel> GetOneDevice(GetOneInputModel input)
        {
            try
            {
                var device = await DeviceBll.GetOneDevice((int)input.Id);

                var model = new DeviceModel {
                    Id = device.Id,
                    DeviceDescription = device.DeviceDescription,
                    DeviceKey = device.DeviceKey,
                    DeviceName = device.DeviceName,
                    DeviceSecret = device.DeviceSecret,
                    IMEI = device.IMEI,
                    IsActived = device.IsActived,
                    Manufacturer = device.Manufacturer,
                    Model = device.Model,
                    Platform = device.Platform,
                    PlatformVersion = device.PlatformVersion,
                    RequestDeviceId = device.RequestDeviceId,
                    SerialNumber = device.SerialNumber
                };
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
        public async Task<DeviceModel> GetOneDeviceByKey(GetOneDeviceByKeyModel input)
        {
            try
            {
                var device = await DeviceBll.GetOneDeviceByKey(input.ClientId, input.DeviceKey);
                var model = new DeviceModel
                {
                    Id = device.Id,
                    DeviceDescription = device.DeviceDescription,
                    DeviceKey = device.DeviceKey,
                    DeviceName = device.DeviceName,
                    DeviceSecret = device.DeviceSecret,
                    IMEI = device.IMEI,
                    IsActived = device.IsActived,
                    Manufacturer = device.Manufacturer,
                    Model = device.Model,
                    Platform = device.Platform,
                    PlatformVersion = device.PlatformVersion,
                    RequestDeviceId = device.RequestDeviceId,
                    SerialNumber = device.SerialNumber
                };
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
                var model = new NotificationResultModel { Status=1 };
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                var device = new Device() {
                    DeviceDescription = input.DeviceDescription,
                    DeviceKey = input.DeviceKey,
                    DeviceName = input.DeviceName,
                    DeviceSecret = input.DeviceSecret,
                    IMEI = input.IMEI,
                    IsActived = input.IsActived,
                    Manufacturer = input.Manufacturer,
                    Model = input.Model,
                    Platform = input.Platform,
                    PlatformVersion = input.PlatformVersion,
                    RequestDeviceId = input.RequestDeviceId,
                    SerialNumber = input.SerialNumber
                };
                var test = await DeviceBll.InsertDevice(device);
                if (test > 0)
                    model.Status = 0;
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
        [Route("UpdateDevice")]
        public async Task<NotificationResultModel> UpdateDevice(DeviceModel input)
        {
            try
            {
                var model = new NotificationResultModel { Status=1};
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                var device = new Device()
                {
                    Id = input.Id,
                    DeviceDescription = input.DeviceDescription,
                    DeviceKey = input.DeviceKey,
                    DeviceName = input.DeviceName,
                    DeviceSecret = input.DeviceSecret,
                    IMEI = input.IMEI,
                    IsActived = input.IsActived,
                    Manufacturer = input.Manufacturer,
                    Model = input.Model,
                    Platform = input.Platform,
                    PlatformVersion = input.PlatformVersion,
                    RequestDeviceId = input.RequestDeviceId,
                    SerialNumber = input.SerialNumber
                };
                var test = await DeviceBll.UpdateDevice(device);
                if (test > 0)
                    model.Status = 0;
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
                var model = new NotificationResultModel {Status =1 };
                var test = await DeviceBll.DeleteDevice((IEnumerable<int>)input.Ids);
                if (test > 0)
                    model.Status = 0;
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
                var model = new NotificationResultModel { Status = 1 };
                var test = await DeviceBll.DeleteRequestDevice((IEnumerable<int>)input.Ids);
                if (test > 0)
                    model.Status = 0;
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("InformNewDevice")]
        public async Task<NotificationResultModel> InformNewDevice(RequestClientModel input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                var model = new NotificationResultModel() { Status = 1 };
                var requestDevice = new RequestDevice()
                {
                    ClientId = input.ClientId,
                    DeviceKey = input.DeviceKey,
                    IMEI = input.IMEI,
                    Manufacturer = input.Manufacturer,
                    Model = input.Model,
                    Platform = input.Platform,
                    PlatformVersion = input.PlatformVersion,

                };
                var test = await DeviceBll.CreateRequestDevice(requestDevice);
                
                    if (test > 0)
                        model.Status = 0;
                return await Task.FromResult(model);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new NotificationResultModel();
            }
           
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetRequestName")]
        public async Task<string> GetRequestName(string deviceKey, string clientId = null)
        {
            string requestName = (string)null;
            if (string.IsNullOrEmpty(clientId))
                clientId = "MSoatVe";
            Device device = await DeviceBll.GetOneDeviceByKey(clientId, deviceKey);
            if (device != null)
                requestName = device.RequestDeviceName;
            return requestName;
        }

        [AllowAnonymous]
        [Route("CheckDevice")]
        [HttpPost]
        public async Task<NotificationResultModel> CheckDevice(GetOneDeviceByKeyModel input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            var model = new NotificationResultModel()
            {
                Status = 1
            };
            model.Status = await DeviceBll.CheckDevice(input.ClientId, input.DeviceKey);
            return model;
        }

        [HttpPost]
        [Route("IsExistDevice")]
        [Authorize]
        public async Task<NotificationResultModel> IsExistDevice(GetOneDeviceByKeyModel input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            var model = new NotificationResultModel()
            {
                Status = 0
            };
            model.Status = await DeviceBll.IsExistDevice(input.ClientId, input.DeviceKey, input.Id);
            return model;
        }
    }
}
