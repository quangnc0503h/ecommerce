using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface IDeviceBll
    {
        Task<Device> GetOneDevice(int deviceId);

        Task<Device> GetOneDeviceByKey(string clientId, string deviceKey);

        Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput input);

        Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput input);

        Task<IEnumerable<Device>> GetAllDevices();

        Task<int> DeleteDevice(int deviceId);

        Task<int> DeleteDevice(IEnumerable<int> Ids);

        Task<int> DeleteRequestDevice(int Id);

        Task<int> DeleteRequestDevice(IEnumerable<int> Ids);

        Task<int> InsertDevice(CreateDeviceInput input);

        Task<int> UpdateDevice(UpdateDeviceInput input);

        Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret);

        Task<InformNewAppOutput> InformNewApp(InformNewAppInput newApp);

        Task<IsExistDeviceIOutput> IsExistDevice(string clientId, string deviceKey, int id);

        Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input);

        Task<IEnumerable<Client>> GetAllClients();
    }
}