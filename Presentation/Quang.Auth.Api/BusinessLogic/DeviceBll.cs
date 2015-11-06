using AspNet.Identity.MySQL;
using Microsoft.AspNet.Identity.Owin;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
namespace Quang.Auth.Api.BusinessLogic
{
    public class DeviceBll : IDeviceBll
    {
        private IDeviceTable _deviceTable;
        private IRequestDeviceTable _requestDeviceTable;

        public MySQLDatabase Database { get; private set; }
        //public object Mapper { get; private set; }

        public DeviceBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._deviceTable = (IDeviceTable)new DeviceTable(this.Database);
            this._requestDeviceTable = (IRequestDeviceTable)new RequestDeviceTable(this.Database);
        }

        public Task<Device> GetOneDevice(int deviceId)
        {
            return Task.FromResult<Device>(this._deviceTable.GetOneDevice(deviceId));
        }

        public Task<Device> GetOneDeviceByKey(string clientId, string deviceKey)
        {
            return Task.FromResult<Device>(this._deviceTable.GetDevice(clientId, deviceKey));
        }

        public Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput input)
        {
            int total = this._deviceTable.GetTotal(input.ClientId, input.Keyword);
            IEnumerable<Device> paging = this._deviceTable.GetPaging(input.PageSize, input.PageNumber, input.ClientId, input.Keyword);
            return Task.FromResult<DanhSachDeviceOutput>(new DanhSachDeviceOutput()
            {
                DanhSachDevices = paging,
                TotalCount = (long)total
            });
        }

        public Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput input)
        {
            int total = this._requestDeviceTable.GetTotal(input.ClientId, input.Keyword, input.DateFrom, input.DateTo);
            IEnumerable<RequestDevice> paging = this._requestDeviceTable.GetPaging(input.PageSize, input.PageNumber, input.ClientId, input.Keyword, input.DateFrom, input.DateTo);
            return Task.FromResult<DanhSachRequestDeviceOutput>(new DanhSachRequestDeviceOutput()
            {
                DanhSachRequestDevices = paging,
                TotalCount = (long)total
            });
        }

        public Task<IEnumerable<Device>> GetAllDevices()
        {
            return Task.FromResult<IEnumerable<Device>>(this._deviceTable.GetAllDevices());
        }

        public Task<int> DeleteDevice(int deviceId)
        {
            return Task.FromResult<int>(this._deviceTable.Delete(deviceId));
        }

        public Task<int> DeleteDevice(IEnumerable<int> Ids)
        {
            return Task.FromResult<int>(this._deviceTable.Delete(Ids));
        }

        public Task<int> DeleteRequestDevice(int Id)
        {
            return Task.FromResult<int>(this._requestDeviceTable.Delete(Id));
        }

        public Task<int> DeleteRequestDevice(IEnumerable<int> Ids)
        {
            return Task.FromResult<int>(this._requestDeviceTable.Delete(Ids));
        }

        public Task<int> InsertDevice(CreateDeviceInput input)
        {
            Mapper.CreateMap<CreateDeviceInput, Device>();
            int result = this._deviceTable.Insert(Mapper.Map<Device>((object)input));
            if (result > 0)
                this._requestDeviceTable.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return Task.FromResult<int>(result);
        }

        public Task<int> UpdateDevice(UpdateDeviceInput input)
        {
            Mapper.CreateMap<UpdateDeviceInput, Device>();
            int result = this._deviceTable.Update(Mapper.Map<Device>((object)input));
            if (result > 0)
                this._requestDeviceTable.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return Task.FromResult<int>(result);
        }

        public Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            return Task.FromResult<Device>(this._deviceTable.GetDevice(clientId, deviceKey, deviceSecret));
        }

        public Task<InformNewAppOutput> InformNewApp(InformNewAppInput newApp)
        {
            int num = 0;
            string str = (string)null;
            if (this._deviceTable.GetDevice(newApp.ClientId, newApp.DeviceKey) == null)
            {
                if (!this._requestDeviceTable.IsExistRequestDevice(newApp.ClientId, newApp.DeviceKey))
                    this._requestDeviceTable.CreateRequestDevice(newApp);
                else
                    this._requestDeviceTable.UpdateRequestDevice(newApp);
                RequestDevice requestDevice = this._requestDeviceTable.GetRequestDevice(newApp.ClientId, newApp.DeviceKey);
                if (requestDevice != null)
                {
                    if (!requestDevice.IsApproved)
                        str = string.Format("R{0}", (object)requestDevice.Id.ToString("D4"));
                    else
                        num = 3;
                }
                else
                    num = 2;
            }
            else
                num = 1;
            return Task.FromResult<InformNewAppOutput>(new InformNewAppOutput()
            {
                Status = num,
                DeviceName = str
            });
        }

        public Task<IsExistDeviceIOutput> IsExistDevice(string clientId, string deviceKey, int id)
        {
            IsExistDeviceIOutput result = new IsExistDeviceIOutput()
            {
                Check = 0
            };
            Device device = this._deviceTable.GetDevice(clientId, deviceKey);
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
            return Task.FromResult<IsExistDeviceIOutput>(result);
        }

        public Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input)
        {
            CheckDeviceOutput result = new CheckDeviceOutput()
            {
                Status = 0
            };
            Device device = this._deviceTable.GetDevice(input.ClientId, input.DeviceKey);
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
                RequestDevice requestDevice = this._requestDeviceTable.GetRequestDevice(input.ClientId, input.DeviceKey);
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
            return Task.FromResult<CheckDeviceOutput>(result);
        }

        public Task<IEnumerable<Client>> GetAllClients()
        {
            return Task.FromResult<IEnumerable<Client>>(this._deviceTable.GetAllClients());
        }
    }
}