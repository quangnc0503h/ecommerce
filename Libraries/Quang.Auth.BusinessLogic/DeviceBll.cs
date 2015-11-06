using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.BusinessLogic
{
    public static class DeviceBll
    {
        public static async Task<Device> GetOneDevice(int deviceId)
        {
            return  await (DeviceDal.GetOneDevice(deviceId));
        }

        public static async Task<Device> GetOneDeviceByKey(string clientId, string deviceKey)
        {
            return  await (DeviceDal.GetDevice(clientId, deviceKey));
        }

        
        public static async Task<IEnumerable<Device>> GetDevicePaging(int pageSize, int pageNumber, string clientId, string keyword)
        {
            return await DeviceDal.GetPaging(pageSize, pageNumber, clientId, keyword);
        }
        public static async Task<long> GetDeviceTotal(string clientId, string keyword)
        {
            return await DeviceDal.GetTotal(clientId, keyword);
        }
        public static async Task<long> GetRequestDeviceTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            return await RequestDeviceDal.GetTotal(clientId, keyword, dateFrom, dateTo);
        }
        public static async Task<IEnumerable<RequestDevice>> GetRequestDevicePaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            return await RequestDeviceDal.GetPaging(pageSize, pageNumber, clientId, keyword, dateFrom, dateTo);
        }
        public static async Task<IEnumerable<Device>> GetAllDevices()
        {
            return  await DeviceDal.GetAllDevices();
        }

        public static async Task<long> DeleteDevice(int deviceId)
        {
            return await (DeviceDal.Delete(deviceId));
        }

        public static async Task<long> DeleteDevice(IEnumerable<int> Ids)
        {
            return  await (DeviceDal.Delete(Ids));
        }

        public static async Task<long> DeleteRequestDevice(int Id)
        {
            return await (RequestDeviceDal.Delete(Id));
        }

        public static async Task<long> DeleteRequestDevice(IEnumerable<int> Ids)
        {
            return  await (RequestDeviceDal.Delete(Ids));
        }

        public static async Task<long> InsertDevice(Device input)
        {
        
            var result = await DeviceDal.Insert(input);
            if (result > 0)
                await RequestDeviceDal.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return (result);
        }

        public static async Task<long> UpdateDevice(Device input)
        {
            
            long result = await DeviceDal.Update(input);
            if (result > 0)
                await RequestDeviceDal.UpdateRequestDeviceStatus(input.ClientId, input.DeviceKey, true);
            return (result);
        }
        public static async Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            return await (DeviceDal.GetDevice(clientId, deviceKey, deviceSecret));
        }
        public static async Task<int> CreateRequestDevice(RequestDevice newApp)
        {
            int num = 0;
            string str = (string)null;
            var device = await DeviceDal.GetDevice(newApp.ClientId, newApp.DeviceKey);
            if (device == null)
            {
                var exit = await RequestDeviceDal.IsExistRequestDevice(newApp.ClientId, newApp.DeviceKey);
                if (!exit)
                    await RequestDeviceDal.CreateRequestDevice(newApp);
                else
                     await RequestDeviceDal.UpdateRequestDevice(newApp);
                RequestDevice requestDevice = await RequestDeviceDal.GetRequestDevice(newApp.ClientId, newApp.DeviceKey);
                if (requestDevice != null)
                {
                    if (!requestDevice.IsApproved)
                        //str = string.Format("R{0}", (object)requestDevice.Id.ToString("D4"));
                        num = (int)requestDevice.Id;
                    else
                        num = 3;
                }
                else
                    num = 2;
            }
            else
                num = 1;
            return num;
        }
        public static async Task<int> IsExistDevice(string clientId, string deviceKey, int id)
        {
            var check = 0;
            
            Device device = await DeviceDal.GetDevice(clientId, deviceKey);
            if (device != null)
            {
                if (id > 0)
                {
                    if (device.Id != id)
                        check = 1;
                }
                else
                    check = 1;
            }
            return check;
        }
        public static async Task<int> CheckDevice(string clientId, string deviceKey)
        {
            var status = 0;           
            Device device = await DeviceDal.GetDevice(clientId, deviceKey);
            if (device != null)
            {
                if (!device.IsActived)
                {
                    status = 1;                    
                }
            }
            else
            {
                RequestDevice requestDevice = await RequestDeviceDal.GetRequestDevice(clientId, deviceKey);
                if (requestDevice != null && !requestDevice.IsApproved)
                {
                    status = 2;                    
                }
                else
                {
                    status = 3;                    
                }
            }
            return (status);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Client>> GetAllClients()
        {
            return await DeviceDal.GetAllClients();
        }
    }
}
