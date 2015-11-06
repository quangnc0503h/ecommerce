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
using Quang.Auth.Entities;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.BusinessLogic;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/Device")]
    public class DeviceController : ApiController
    {
        private const string DefaultClientId = "MSoatVe";
        private IDeviceBll _deviceBll;

        public DeviceController()
        {
        }

        public DeviceController(IDeviceBll deviceBll)
        {
            this._deviceBll = deviceBll;
        }

        [HttpGet]
        [Route("GetAllClients")]
        [AppAuthorize(Roles = "160")]
        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await this._deviceBll.GetAllClients();
        }

        [HttpPost]
        [Route("GetAll")]
        [AppAuthorize(Roles = "160")]
        public async Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput filter)
        {
            return await this._deviceBll.GetAll(filter);
        }

        [HttpPost]
        [AppAuthorize(Roles = "160")]
        [Route("GetAllRequest")]
        public async Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput filter)
        {
            return await this._deviceBll.GetAllRequest(filter);
        }

        [HttpPost]
        [AppAuthorize(Roles = "160")]
        [Route("GetOneDevice")]
        public async Task<GetOneDeviceOutput> GetOneDevice(GetByIdInput input)
        {
            Device result = await this._deviceBll.GetOneDevice(input.Id);
            return new GetOneDeviceOutput()
            {
                Device = result
            };
        }

        [AppAuthorize(Roles = "160")]
        [HttpPost]
        [Route("GetOneDeviceByKey")]
        public async Task<GetOneDeviceOutput> GetOneDeviceByKey(GetOneDeviceByKeyInput input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            Device result = await this._deviceBll.GetOneDeviceByKey(input.ClientId, input.DeviceKey);
            return new GetOneDeviceOutput()
            {
                Device = result
            };
        }

        [AppAuthorize(Roles = "160")]
        [HttpPost]
        [Route("CreateDevice")]
        public async Task<ResultUpdateOutput> CreateDevice(CreateDeviceInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            int test = await this._deviceBll.InsertDevice(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [Route("UpdateDevice")]
        [AppAuthorize(Roles = "160")]
        [HttpPost]
        public async Task<ResultUpdateOutput> UpdateDevice(UpdateDeviceInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            int test = await this._deviceBll.UpdateDevice(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [Route("DeleteDevice")]
        [HttpPost]
        [AppAuthorize(Roles = "160")]
        public async Task<ResultUpdateOutput> DeleteDevice(DeleteDeviceInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._deviceBll.DeleteDevice((IEnumerable<int>)input.Ids);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [AppAuthorize(Roles = "160")]
        [Route("DeleteRequestDevice")]
        [HttpPost]
        public async Task<ResultUpdateOutput> DeleteRequestDevice(DeleteDeviceInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._deviceBll.DeleteRequestDevice((IEnumerable<int>)input.Ids);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("InformNewDevice")]
        public async Task<InformNewAppOutput> InformNewDevice(InformNewAppInput input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            return await this._deviceBll.InformNewApp(input);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRequestName")]
        public async Task<string> GetRequestName(string deviceKey, string clientId = null)
        {
            string requestName = (string)null;
            if (string.IsNullOrEmpty(clientId))
                clientId = "MSoatVe";
            Device device = await this._deviceBll.GetOneDeviceByKey(clientId, deviceKey);
            if (device != null)
                requestName = device.RequestDeviceName;
            return requestName;
        }

        [AllowAnonymous]
        [Route("CheckDevice")]
        [HttpPost]
        public async Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            return await this._deviceBll.CheckDevice(input);
        }

        [HttpPost]
        [Route("IsExistDevice")]
        [Authorize]
        public async Task<IsExistDeviceIOutput> IsExistDevice(IsExistDeviceInput input)
        {
            if (string.IsNullOrEmpty(input.ClientId))
                input.ClientId = "MSoatVe";
            return await this._deviceBll.IsExistDevice(input.ClientId, input.DeviceKey, input.Id);
        }
    }
}
