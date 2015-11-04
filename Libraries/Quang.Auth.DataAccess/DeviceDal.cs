using System.Text.RegularExpressions;
using Dapper;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.DataAccess
{
    public static class DeviceDal
    {
        public static async Task<long> Delete(int deviceId)
        {
            const string commandText = "Delete from Devices where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", deviceId);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        public static async Task<long> Delete(IEnumerable<int> Ids)
        {
            string commandText = "Delete from Devices where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            var parameters = new DynamicParameters();
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        public static async Task<long> Insert(Device device)
        {
            const string commandText = @" Insert into Devices (Id, ClientId, RequestDeviceId, IsActived, DeviceKey, DeviceSecret, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, DeviceName, DeviceDescription) 
                                         values (@id, @clientId, @requestDeviceId, @isActived, @deviceKey, @deviceSecret, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @deviceName, @deviceDescription)";
            var parameters = new DynamicParameters();
            if (device.Id > 0)
                parameters.Add("id", device.Id);
            else
                parameters.Add("id",0);
            parameters.Add("clientId", device.ClientId);
            parameters.Add("requestDeviceId", device.RequestDeviceId);
            parameters.Add("isActived", device.IsActived ? 1 : 0);
            parameters.Add("deviceKey", device.DeviceKey);
            parameters.Add("deviceSecret", device.DeviceSecret);
            parameters.Add("serialNumber", device.SerialNumber);
            parameters.Add("iMEI", device.IMEI);
            parameters.Add("manufacturer", device.Manufacturer);
            parameters.Add("model", device.Model);
            parameters.Add("platform", device.Platform);
            parameters.Add("platformVersion", device.PlatformVersion);
            parameters.Add("deviceName", device.DeviceName);
            parameters.Add("deviceDescription", device.DeviceDescription);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }

        public static async Task<long> Update(Device device)
        {
            var parameters = new DynamicParameters();
            const string commandText = "Update Devices Set " + "ClientId = @clientId, " + "IsActived = @isActived, " +
                                       "DeviceKey = @deviceKey, " + "DeviceSecret = @deviceSecret, " +
                                       "SerialNumber = @serialNumber, " + "IMEI = @iMEI, " + "Manufacturer = @manufacturer, " +
                                       "Model = @model, " + "Platform = @platform, " + "PlatformVersion = @platformVersion, " +
                                       "DeviceName = @deviceName, " + "DeviceDescription = @deviceDescription " +
                                       "where Id = @id";
            parameters.Add("id", device.Id);
            parameters.Add("clientId", device.ClientId);
            parameters.Add("@isActived", (device.IsActived ? 1 : 0));
            parameters.Add("deviceKey", device.DeviceKey);
            parameters.Add("deviceSecret", device.DeviceSecret);
            parameters.Add("serialNumber", device.SerialNumber);
            parameters.Add("iMEI", device.IMEI);
            parameters.Add("manufacturer", device.Manufacturer);
            parameters.Add("model", device.Model);
            parameters.Add("platform", device.Platform);
            parameters.Add("platformVersion", device.PlatformVersion);
            parameters.Add("@deviceName", device.DeviceName);
            parameters.Add("deviceDescription", device.DeviceDescription);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;



        }

        public static async Task<IEnumerable<Device>> GetAllDevices()
        {

            List<Device> devices;
            var parameters = new DynamicParameters();
            const string commandText = "Select * from Devices order by Id";
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Device>(commandText, parameters);
                devices = data.ToList();
            }

            return (IEnumerable<Device>)devices;

        }

        public static async Task<Device> GetOneDevice(long deviceId)
        {
            Device device;
            const string commandText = "Select * from Devices where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("@id", deviceId);
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Device>(commandText, parameters);
                device = data.First();
            }

            return device;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static async Task<long> GetTotal(string clientId, string keyword)
        {
            string commandText = "select count(*) from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            var parameters = new DynamicParameters();
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (!string.IsNullOrEmpty(clientId))
            {
                commandText += " AND ClientId=@clientId";
                parameters.Add("@clientId", clientId);
            }
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@rid", num);
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = id.Single();
            }

            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="clientId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Device>> GetPaging(int pageSize, int pageNumber, string clientId, string keyword)
        {
            var parameters = new DynamicParameters();
            string commandText = "select * from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            parameters.Add("param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (!string.IsNullOrEmpty(clientId))
            {
                commandText += " AND ClientId=@clientId";
                parameters.Add("clientId", clientId);
            }
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("rid", num);
            commandText = commandText + " order by Id limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            List<Device> devices;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<Device>(commandText, parameters);
                devices = data.ToList();
            }

            return (IEnumerable<Device>)devices;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="deviceKey"></param>
        /// <param name="deviceSecret"></param>
        /// <returns></returns>
        public static async Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            const string commandText = @"select * from Devices where ClientId=@clientId and DeviceKey = @deviceKey and DeviceSecret = @deviceSecret and IsActived = @isActived";
            var parameters = new DynamicParameters();
            List<Device> devices;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                parameters.Add("clientId", clientId);
                parameters.Add("deviceKey", deviceKey);
                parameters.Add("deviceSecret", deviceSecret);
                parameters.Add("isActived", true);
                var data = await conn.QueryAsync<Device>(commandText, parameters);
                devices = data.ToList();

            }

            return devices.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="deviceKey"></param>
        /// <returns></returns>
        public static async Task<Device> GetDevice(string clientId, string deviceKey)
        {
            const string commandText = @"select * from Devices where ClientId=@clientId and  DeviceKey = @deviceKey";
            var parameters = new DynamicParameters();
            List<Device> devices;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                parameters.Add("clientId", clientId);
                parameters.Add("deviceKey", deviceKey);
                var data = await conn.QueryAsync<Device>(commandText, parameters);
                devices = data.ToList();

            }

            return devices.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<Client>> GetAllClients()
        {
            const string commandText = "SELECT * FROM Clients ";
            var parameters = new Dictionary<string, object>() { };
            List<Client> clients;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                var data = await conn.QueryAsync<Client>(commandText, parameters);
                clients = data.ToList();
            }

            return clients;
        }

       
       
    }
}
