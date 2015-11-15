using Quang.Auth.Entities;
using System.Collections.Generic;

namespace Quang.Auth.Api.DataAccess
{
    public interface IDeviceTable
    {
        Device GetOneDevice(int deviceId);

        int Delete(int deviceId);

        int Delete(IEnumerable<int> Ids);

        int Insert(Device device);

        int Update(Device device);

        IEnumerable<Device> GetAllDevices();

        int GetTotal(string clientId, string keyword);

        IEnumerable<Device> GetPaging(int pageSize, int pageNumber, string clientId, string keyword);

        Device GetDevice(string clientId, string deviceKey);

        Device GetDevice(string clientId, string deviceKey, string deviceSecret);

        IEnumerable<Client> GetAllClients();
    }
}