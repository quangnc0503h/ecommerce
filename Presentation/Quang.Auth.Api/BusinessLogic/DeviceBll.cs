using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
namespace Quang.Auth.Api.BusinessLogic
{
    public class DeviceBll : IDeviceBll
    {
        private readonly IDeviceTable _deviceTable;
        private readonly IRequestDeviceTable _requestDeviceTable;

        public MySQLDatabase Database { get; private set; }
        //public object Mapper { get; private set; }

        public DeviceBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _deviceTable = new DeviceTable(Database);
            _requestDeviceTable = new RequestDeviceTable(Database);
        }

        public Task<Device> GetOneDevice(int deviceId)
        {
            return Task.FromResult(_deviceTable.GetOneDevice(deviceId));
        }

        public Task<Device> GetOneDeviceByKey(string clientId, string deviceKey)
        {
            return Task.FromResult(_deviceTable.GetDevice(clientId, deviceKey));
        }

        public Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput input)
        {
            int total = _deviceTable.GetTotal(input.ClientId, input.Keyword);
            IEnumerable<Device> paging = _deviceTable.GetPaging(input.PageSize, input.PageNumber, input.ClientId, input.Keyword);
            return Task.FromResult(new DanhSachDeviceOutput
                                   {
                DanhSachDevices = paging,
                TotalCount = total
            });
        }

        public Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput input)
        {
            int total = _requestDeviceTable.GetTotal(input.ClientId, input.Keyword, input.DateFrom, input.DateTo);
            IEnumerable<RequestDevice> paging = _requestDeviceTable.GetPaging(input.PageSize, input.PageNumber, input.ClientId, input.Keyword, input.DateFrom, input.DateTo);
            return Task.FromResult(new DanhSachRequestDeviceOutput
                                   {
                DanhSachRequestDevices = paging,
                TotalCount = total
            });
        }

        public Task<IEnumerable<Device>> GetAllDevices()
        {
            return Task.FromResult(_deviceTable.GetAllDevices());
        }

        public Task<int> DeleteDevice(int deviceId)
        {
            return Task.FromResult(_deviceTable.Delete(deviceId));
        }

        public Task<int> DeleteDevice(IEnumerable<int> Ids)
        {
            return Task.FromResult(_deviceTable.Delete(Ids));
        }

        public Task<int> DeleteRequestDevice(int Id)
        {
            return Task.FromResult(_requestDeviceTable.Delete(Id));
        }

        public Task<int> DeleteRequestDevice(IEnumerable<int> Ids)
        {
            return Task.FromResult(_requestDeviceTable.Delete(Ids));
        }

        public Task<int> InsertDevice(CreateDeviceInput input)
        {
            Mapper.CreateMap<CreateDeviceInput, Device>();
            int result = _deviceTable.Insert(Mapper.Map<Device>(input));
            if (result > 0)
                _requestDeviceTable.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return Task.FromResult(result);
        }

        public Task<int> UpdateDevice(UpdateDeviceInput input)
        {
            Mapper.CreateMap<UpdateDeviceInput, Device>();
            int result = _deviceTable.Update(Mapper.Map<Device>(input));
            if (result > 0)
                _requestDeviceTable.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return Task.FromResult(result);
        }

        public Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            return Task.FromResult(_deviceTable.GetDevice(clientId, deviceKey, deviceSecret));
        }

        public Task<InformNewAppOutput> InformNewApp(InformNewAppInput newApp)
        {
            int num = 0;
            string str = null;
            if (_deviceTable.GetDevice(newApp.ClientId, newApp.DeviceKey) == null)
            {
                if (!_requestDeviceTable.IsExistRequestDevice(newApp.ClientId, newApp.DeviceKey))
                    _requestDeviceTable.CreateRequestDevice(newApp);
                else
                    _requestDeviceTable.UpdateRequestDevice(newApp);
                RequestDevice requestDevice = _requestDeviceTable.GetRequestDevice(newApp.ClientId, newApp.DeviceKey);
                if (requestDevice != null)
                {
                    if (!requestDevice.IsApproved)
                        str = string.Format("R{0}", requestDevice.Id.ToString("D4"));
                    else
                        num = 3;
                }
                else
                    num = 2;
            }
            else
                num = 1;
            return Task.FromResult(new InformNewAppOutput
                                   {
                Status = num,
                DeviceName = str
            });
        }

        public Task<IsExistDeviceIOutput> IsExistDevice(string clientId, string deviceKey, int id)
        {
            var result = new IsExistDeviceIOutput
                         {
                Check = 0
            };
            Device device = _deviceTable.GetDevice(clientId, deviceKey);
            if (device != null)
            {
                if (id > 0)
                {
                    if (device.Id != id)
                        result.Check = 1;
                }
                else
                    result.Check = 1;
            }
            return Task.FromResult(result);
        }

        public Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input)
        {
            var result = new CheckDeviceOutput
                         {
                Status = 0
            };
            Device device = _deviceTable.GetDevice(input.ClientId, input.DeviceKey);
            if (device != null)
            {
                if (!device.IsActived)
                {
                    result.Status = 1;
                    result.Description = "Device is not actived";
                }
            }
            else
            {
                RequestDevice requestDevice = _requestDeviceTable.GetRequestDevice(input.ClientId, input.DeviceKey);
                if (requestDevice != null && !requestDevice.IsApproved)
                {
                    result.Status = 2;
                    result.Description = "Device is waiting for approval";
                }
                else
                {
                    result.Status = 3;
                    result.Description = "Device not exist";
                }
            }
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Client>> GetAllClients()
        {
            return Task.FromResult(_deviceTable.GetAllClients());
        }
    }
}