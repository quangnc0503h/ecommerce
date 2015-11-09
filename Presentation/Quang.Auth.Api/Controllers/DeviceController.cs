using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using StackExchange.Exceptional;
using Quang.Auth.Entities;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.BusinessLogic;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/Device")]
    public class DeviceController : BaseApiController
    {
/*
        private const string DefaultClientId = "MSoatVe";
*/
        private readonly IDeviceBll _deviceBll;

        public DeviceController()
        {
        }

        public DeviceController(IDeviceBll deviceBll)
        {
            _deviceBll = deviceBll;
        }

        [HttpGet]
        [Route("GetAllClients")]
        //  [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            try
            {
                return await _deviceBll.GetAllClients();
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new List<Client>();
        }

        [HttpPost]
        [Route("GetAll")]
        //   [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput filter)
        {
            try
            {
                return await _deviceBll.GetAll(filter);
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DanhSachDeviceOutput();
        }

        [HttpPost]
        // [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetAllRequest")]
        public async Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput filter)
        {
            try
            {
                return await _deviceBll.GetAllRequest(filter);
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DanhSachRequestDeviceOutput();
        }

        [HttpPost]
        // [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("GetOneDevice")]
        public async Task<GetOneDeviceOutput> GetOneDevice(GetByIdInput input)
        {
            try
            {
                var result = await _deviceBll.GetOneDevice(input.Id);
                return new GetOneDeviceOutput
                       {
                    Device = result
                };
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new GetOneDeviceOutput();
        }

        //   [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [HttpPost]
        [Route("GetOneDeviceByKey")]
        public async Task<GetOneDeviceOutput> GetOneDeviceByKey(GetOneDeviceByKeyInput input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                var result = await _deviceBll.GetOneDeviceByKey(input.ClientId, input.DeviceKey);
                return new GetOneDeviceOutput
                {
                    Device = result
                };
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new GetOneDeviceOutput();

        }

        // [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [HttpPost]
        [Route("CreateDevice")]
        public async Task<ResultUpdateOutput> CreateDevice(CreateDeviceInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                int test = await _deviceBll.InsertDevice(input);
                if (test > 0)
                    result.Status = 0;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
         
           
            return result;
        }

        [Route("UpdateDevice")]
        //   [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [HttpPost]
        public async Task<ResultUpdateOutput> UpdateDevice(UpdateDeviceInput input)
        {
            var result = new ResultUpdateOutput
                         {
                             Status = 1
                         };
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                int test = await _deviceBll.UpdateDevice(input);
                if (test > 0)
                    result.Status = 0;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
         
            return result;
        }

        [Route("DeleteDevice")]
        [HttpPost]
        //   [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        public async Task<ResultUpdateOutput> DeleteDevice(DeleteDeviceInput input)
        {
            var result = new ResultUpdateOutput
                         {
                             Status = 1
                         };
            try
            {
                int test = await _deviceBll.DeleteDevice(input.Ids);
                if (test > 0)
                    result.Status = 0;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }

           
            return result;
        }

        //   [AppAuthorize(Roles = ActionRole.HeThong.Devices)]
        [Route("DeleteRequestDevice")]
        [HttpPost]
        public async Task<ResultUpdateOutput> DeleteRequestDevice(DeleteDeviceInput input)
        {
            var result = new ResultUpdateOutput
                         {
                             Status = 1
                         };
            try
            {
                int test = await _deviceBll.DeleteRequestDevice(input.Ids);
                if (test > 0)
                    result.Status = 0;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }

            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("InformNewDevice")]
        public async Task<InformNewAppOutput> InformNewDevice(InformNewAppInput input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                return await _deviceBll.InformNewApp(input);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new InformNewAppOutput();
            }

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRequestName")]
        public async Task<string> GetRequestName(string deviceKey, string clientId = null)
        {
            try
            {
                var requestName = (string)null;
                if (string.IsNullOrEmpty(clientId))
                    clientId = "MSoatVe";
                Device device = await _deviceBll.GetOneDeviceByKey(clientId, deviceKey);
                if (device != null)
                    requestName = device.RequestDeviceName;
                return requestName;
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return string.Empty;
            }

        }

        [AllowAnonymous]
        [Route("CheckDevice")]
        [HttpPost]
        public async Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                return await _deviceBll.CheckDevice(input);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new CheckDeviceOutput();
            }


        }

        [HttpPost]
        [Route("IsExistDevice")]
        //[Authorize]
        public async Task<IsExistDeviceIOutput> IsExistDevice(IsExistDeviceInput input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.ClientId))
                    input.ClientId = "MSoatVe";
                return await _deviceBll.IsExistDevice(input.ClientId, input.DeviceKey, input.Id);
            }
            catch (Exception ex)
            {

                ErrorStore.LogExceptionWithoutContext(ex);
                return new IsExistDeviceIOutput();
            }

        }
    }
}
