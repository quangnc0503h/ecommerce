using Dapper;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quang.Auth.DataAccess
{
    public static class RequestDeviceDal
    {
        public static async Task<long> Delete(int id)
        {
            const string commandText = "Delete from RequestDevices where Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", id);
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }
       

        public static async Task< long> Delete(IEnumerable<int> Ids)
        {
            string commandText = "Delete from RequestDevices where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            var parameters = new DynamicParameters();
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
            
        }

        public static async Task<long> GetTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            string commandText = "select count(*) from RequestDevices where IsApproved = false AND (Id = @id OR DeviceKey LIKE @param)";
            var parameters = new DynamicParameters();
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@id", (object)num);
            if (!string.IsNullOrEmpty(clientId))
            {
                commandText += " and ClientId = @clientId";
                parameters.Add("@clientId", (object)clientId);
            }
            if (dateFrom.HasValue)
            {
                commandText += " and Created >= @dateFrom";
                parameters.Add("@dateFrom", (object)dateFrom.Value.ToString("yyyy-MM-dd"));
            }
            if (dateTo.HasValue)
            {
                commandText += " and Created >= @dateTo";
                parameters.Add("@dateTo", (object)dateTo.Value.ToString("yyyy-MM-dd"));
            }
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = id.Single();
            }

            return results;
        }

        public static async Task< IEnumerable<RequestDevice>> GetPaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            var parameters = new DynamicParameters();
            string str = @" select *, EXISTS(SELECT * FROM TrustDevices WHERE TrustDevices.DeviceSerial = RequestDevices.SerialNumber) as IsTrust   
                            from RequestDevices where IsApproved = false and (Id = @id or DeviceKey LIKE @param)";
            int num1 = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num1 = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@id", (object)num1);
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (!string.IsNullOrEmpty(clientId))
            {
                str += " and ClientId = @clientId";
                parameters.Add("@clientId", (object)clientId);
            }
            if (dateFrom.HasValue)
            {
                str += " and Created >= @dateFrom";
                parameters.Add("@dateFrom", (object)dateFrom.Value.ToString("yyyy-MM-dd 00:00:00"));
            }
            if (dateTo.HasValue)
            {
                str += " and Created <= @dateTo";
                parameters.Add("@dateTo", (object)dateTo.Value.ToString("yyyy-MM-dd 23:59:59"));
            }
            string commandText = str + " order by Created limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<RequestDevice> requestDevices = new List<RequestDevice>();
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<RequestDevice>(commandText, parameters);
                requestDevices = data.ToList();
            }

           
            return (IEnumerable<RequestDevice>)requestDevices;
        }

        public static async Task< long> CreateRequestDevice(RequestDevice newApp)
        {
            const string commandText = @"Insert into RequestDevices (Id, ClientId, DeviceKey, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, IsApproved, Created, Updated) " + "values (@id, @clientId, @deviceKey, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @isApproved, @created, @updated)";
            var parameters = new DynamicParameters();
            parameters.Add("clientId", newApp.ClientId);
            parameters.Add("deviceKey", newApp.DeviceKey);
            parameters.Add("serialNumber", newApp.SerialNumber);
            parameters.Add("iMEI", newApp.IMEI);
            parameters.Add("manufacturer", newApp.Manufacturer);
            parameters.Add("model", newApp.Model);
            parameters.Add("platform", newApp.Platform);
            parameters.Add("platformVersion", newApp.PlatformVersion);
            parameters.Add("isApproved", false);
            parameters.Add("updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            parameters.Add("created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
          
        }

        public static async Task<long> UpdateRequestDevice(RequestDevice newApp)
        {
            return await UpdateRequestDevice(newApp, false);
        }

        public static async  Task<long> UpdateRequestDevice(RequestDevice newApp, bool isApproved)
        {
            if (isApproved)
            {
                const string commandText = @"Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, IsApproved = @isApproved, Updated = @updated Where DeviceKey = @deviceKey";
                var parameters = new DynamicParameters();
                parameters.Add("clientId", newApp.ClientId);
                parameters.Add("deviceKey", newApp.DeviceKey);
                parameters.Add("serialNumber", newApp.SerialNumber);
                parameters.Add("iMEI", newApp.IMEI);
                parameters.Add("manufacturer", newApp.Manufacturer);
                parameters.Add("model", newApp.Model);
                parameters.Add("platform", newApp.Platform);
                parameters.Add("platformVersion", newApp.PlatformVersion);
                parameters.Add("isApproved", isApproved);
                parameters.Add("updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //            if (isApproved)
                long results;
                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {

                    var data = await conn.QueryAsync<long>(commandText, parameters);
                    results = data.FirstOrDefault();
                }
                return results;
            }
            else
            {
                const string commandText = @"Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, Created = @created Where DeviceKey = @deviceKey and IsApproved = @isApproved";
                var parameters = new DynamicParameters();
                parameters.Add("clientId", newApp.ClientId);
                parameters.Add("deviceKey", newApp.DeviceKey);
                parameters.Add("serialNumber", (object)newApp.SerialNumber);
                parameters.Add("iMEI", (object)newApp.IMEI);
                parameters.Add("manufacturer", (object)newApp.Manufacturer);
                parameters.Add("model", (object)newApp.Model);
                parameters.Add("platform", (object)newApp.Platform);
                parameters.Add("platformVersion", (object)newApp.PlatformVersion);
                parameters.Add("isApproved", isApproved);
                parameters.Add("created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                long results;
                using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
                {

                    var data = await conn.QueryAsync<long>(commandText, parameters);
                    results = data.FirstOrDefault();
                }
                return results;
            }
           
        }

        public static async Task<long> UpdateRequestDeviceStatus(string clientId, string deviceKey, bool isApproved)
        {
            const string commandText = @"Update RequestDevices Set IsApproved = @isApproved, Updated = @updated Where ClientId = @clientId and DeviceKey = @deviceKey";
            var parameters = new DynamicParameters();
            parameters.Add("clientId", clientId);
            parameters.Add("deviceKey", deviceKey);
            parameters.Add("isApproved", isApproved);
            parameters.Add("updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

//            if (isApproved)
                long results;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var data = await conn.QueryAsync<long>(commandText, parameters);
                results = data.FirstOrDefault();
            }
            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="deviceKey"></param>
        /// <returns></returns>
        public static async Task<bool> IsExistRequestDevice(string clientId, string deviceKey)
        {
            const string commandText = @"select count(*) from RequestDevices where ClientId = @clientId and DeviceKey = @deviceKey and IsApproved = @isApproved";
            var parameters = new DynamicParameters();
            parameters.Add("clientId", clientId);
            parameters.Add("deviceKey", deviceKey);
            parameters.Add("isApproved", false);
            long results;

            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {

                var id = await conn.QueryAsync<long>(commandText, parameters);
                results = id.Single();
            }
            return results > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="deviceKey"></param>
        /// <returns></returns>
        public static  async Task<RequestDevice> GetRequestDevice(string clientId, string deviceKey)
        {
            const string commandText = @"select * from RequestDevices where ClientId=@clientId and DeviceKey = @deviceKey and IsApproved = @isApproved";
            var parameters = new DynamicParameters();
            List<RequestDevice> requestDevices;
            using (var conn = await DataAccessBase.GetOpenAsync(DataAccessBase.QuangAuthConn))
            {
                parameters.Add("clientId", clientId);
                parameters.Add("deviceKey", deviceKey);
                parameters.Add("isApproved", false);
                var data = await conn.QueryAsync<RequestDevice>(commandText, parameters);
                requestDevices = data.ToList();

            }

            return requestDevices.FirstOrDefault();
         
        }
    }
}
