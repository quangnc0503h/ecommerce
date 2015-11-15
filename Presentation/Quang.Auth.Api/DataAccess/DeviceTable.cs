using AspNet.Identity.MySQL;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class DeviceTable : IDeviceTable
    {
        private readonly MySQLDatabase _database;

        public DeviceTable(MySQLDatabase database)
        {
            _database = database;
        }

        public int Delete(int deviceId)
        {
            return _database.Execute("Delete from Devices where Id = @id", new Dictionary<string, object>{{"@id",deviceId}});
        }

        public int Delete(IEnumerable<int> Ids)
        {
            string commandText = "Delete from Devices where Id in (" + string.Join(",", Ids.ToArray()) + ")";
            var parameters = new Dictionary<string, object>();
            int num = _database.Execute(commandText, parameters);
            _database.Execute(commandText, new Dictionary<string, object>());
            return num;
        }

        public int Insert(Device device)
        {
            const string commandText = "Insert into Devices (Id, ClientId, RequestDeviceId, IsActived, DeviceKey, DeviceSecret, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, DeviceName, DeviceDescription) " + "values (@id, @clientId, @requestDeviceId, @isActived, @deviceKey, @deviceSecret, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @deviceName, @deviceDescription)";
            var parameters = new Dictionary<string, object>();
            if (device.Id > 0)
                parameters.Add("@id", device.Id);
            else
                parameters.Add("@id", null);
            parameters.Add("@clientId", device.ClientId);
            parameters.Add("@requestDeviceId", device.RequestDeviceId);
            parameters.Add("@isActived", (device.IsActived ? 1 : 0));
            parameters.Add("@deviceKey", device.DeviceKey);
            parameters.Add("@deviceSecret", device.DeviceSecret);
            parameters.Add("@serialNumber", device.SerialNumber);
            parameters.Add("@iMEI", device.IMEI);
            parameters.Add("@manufacturer", device.Manufacturer);
            parameters.Add("@model", device.Model);
            parameters.Add("@platform", device.Platform);
            parameters.Add("@platformVersion", device.PlatformVersion);
            parameters.Add("@deviceName", device.DeviceName);
            parameters.Add("@deviceDescription", device.DeviceDescription);
            return _database.Execute(commandText, parameters);
        }

        public int Update(Device device)
        {
            return
                _database.Execute("Update Devices Set " + "ClientId = @clientId, " + "IsActived = @isActived, " +
                    "DeviceKey = @deviceKey, " + "DeviceSecret = @deviceSecret, " + "SerialNumber = @serialNumber, " +
                    "IMEI = @iMEI, " + "Manufacturer = @manufacturer, " + "Model = @model, " + "Platform = @platform, " +
                    "PlatformVersion = @platformVersion, " + "DeviceName = @deviceName, " +
                    "DeviceDescription = @deviceDescription " + "where Id = @id", new Dictionary<string, object>()
                                                                                  {
                                                                                      {"@id", device.Id},
                                                                                      {"@clientId", device.ClientId},
                                                                                      {
                                                                                          "@isActived",
                                                                                          device.IsActived ? 1 : 0
                                                                                      },
                                                                                      {"@deviceKey", device.DeviceKey},
                                                                                      {
                                                                                          "@deviceSecret",
                                                                                          device.DeviceSecret
                                                                                      },
                                                                                      {
                                                                                          "@serialNumber",
                                                                                          device.SerialNumber
                                                                                      },
                                                                                      {"@iMEI", device.IMEI},
                                                                                      {
                                                                                          "@manufacturer",
                                                                                          device.Manufacturer
                                                                                      },
                                                                                      {"@model", device.Model},
                                                                                      {"@platform", device.Platform},
                                                                                      {
                                                                                          "@platformVersion",
                                                                                          device.PlatformVersion
                                                                                      },
                                                                                      {"@deviceName", device.DeviceName},
                                                                                      {
                                                                                          "@deviceDescription",
                                                                                          device.DeviceDescription
                                                                                      }
                                                                                  });
        }

        public IEnumerable<Device> GetAllDevices()
        {
            var list = new List<Device>();
            foreach (Dictionary<string, string> dictionary in this._database.Query("Select * from Devices order by Id"))
            {
                var device = new Device
                             {
                                 Id = int.Parse(dictionary["Id"]),
                                 ClientId = dictionary["ClientId"],
                                 IsActived = bool.Parse(dictionary["IsActived"]),
                                 DeviceKey = dictionary["DeviceKey"],
                                 DeviceSecret = dictionary["DeviceSecret"],
                                 DeviceName = dictionary["DeviceName"],
                                 DeviceDescription = dictionary["DeviceDescription"],
                                 SerialNumber = dictionary["SerialNumber"],
                                 IMEI = dictionary["IMEI"],
                                 Manufacturer = dictionary["Manufacturer"],
                                 Model = dictionary["Model"],
                                 Platform = dictionary["Platform"],
                                 PlatformVersion = dictionary["PlatformVersion"],
                                 RequestDeviceId = new int?()
                             };
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = int.Parse(dictionary["RequestDeviceId"]);
                list.Add(device);
            }
            return list;
        }

        public Device GetOneDevice(int deviceId)
        {
            var device = (Device)null;
            List<Dictionary<string, string>> list = _database.Query("Select * from Devices where Id = @id", new Dictionary<string, object>
                                                                                                            {
        {"@id",deviceId}});
            if (list != null && list.Count == 1)
            {
                var dictionary = list[0];
                device = new Device
                         {
                             Id = int.Parse(dictionary["Id"]),
                             ClientId = dictionary["ClientId"],
                             IsActived = bool.Parse(dictionary["IsActived"]),
                             DeviceKey = dictionary["DeviceKey"],
                             DeviceSecret = dictionary["DeviceSecret"],
                             DeviceName = dictionary["DeviceName"],
                             DeviceDescription = dictionary["DeviceDescription"],
                             SerialNumber = dictionary["SerialNumber"],
                             IMEI = dictionary["IMEI"],
                             Manufacturer = dictionary["Manufacturer"],
                             Model = dictionary["Model"],
                             Platform = dictionary["Platform"],
                             PlatformVersion = dictionary["PlatformVersion"],
                             RequestDeviceId = new int?()
                         };
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = int.Parse(dictionary["RequestDeviceId"]);
            }
            return device;
        }

        public int GetTotal(string clientId, string keyword)
        {
            string commandText = "select count(*) from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            var parameters = new Dictionary<string, object> {{"@param", "%" + Utils.EncodeForLike(keyword) + "%"}};
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
            return int.Parse(_database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<Device> GetPaging(int pageSize, int pageNumber, string clientId, string keyword)
        {
            var parameters = new Dictionary<string, object>();
            string str = "select * from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (!string.IsNullOrEmpty(clientId))
            {
                str += " AND ClientId=@clientId";
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
            string commandText = str + " order by Id limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            var list = new List<Device>();
            foreach (Dictionary<string, string> dictionary in _database.Query(commandText, parameters))
            {
                var device = new Device
                                {
                                    Id = int.Parse(dictionary["Id"]),
                                    ClientId = dictionary["ClientId"],
                                    IsActived = bool.Parse(dictionary["IsActived"]),
                                    DeviceKey = dictionary["DeviceKey"],
                                    DeviceSecret = dictionary["DeviceSecret"],
                                    DeviceName = dictionary["DeviceName"],
                                    DeviceDescription = dictionary["DeviceDescription"],
                                    SerialNumber = dictionary["SerialNumber"],
                                    IMEI = dictionary["IMEI"],
                                    Manufacturer = dictionary["Manufacturer"],
                                    Model = dictionary["Model"],
                                    Platform = dictionary["Platform"],
                                    PlatformVersion = dictionary["PlatformVersion"],
                                    RequestDeviceId = new int?()
                                };
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = int.Parse(dictionary["RequestDeviceId"]);
                list.Add(device);
            }
            return list;
        }

        public Device GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            List<Dictionary<string, string>> list = _database.Query("select * from Devices where ClientId=@clientId and DeviceKey = @deviceKey and DeviceSecret = @deviceSecret and IsActived = @isActived", new Dictionary<string, object>()
      {
        {
          "@clientId",
          clientId
        },
        {
          "@deviceKey",
          deviceKey
        },
        {
          "@deviceSecret",
          deviceSecret
        },
        {
          "@isActived",
          true
        }
      });
            Device device = null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                device = new Device
                         {
                             Id = int.Parse(dictionary["Id"]),
                             ClientId = dictionary["ClientId"],
                             IsActived = bool.Parse(dictionary["IsActived"]),
                             DeviceKey = dictionary["DeviceKey"],
                             DeviceSecret = dictionary["DeviceSecret"],
                             DeviceName = dictionary["DeviceName"],
                             DeviceDescription = dictionary["DeviceDescription"],
                             SerialNumber = dictionary["SerialNumber"],
                             IMEI = dictionary["IMEI"],
                             Manufacturer = dictionary["Manufacturer"],
                             Model = dictionary["Model"],
                             Platform = dictionary["Platform"],
                             PlatformVersion = dictionary["PlatformVersion"],
                             RequestDeviceId = new int?()
                         };
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = int.Parse(dictionary["RequestDeviceId"]);
            }
            return device;
        }

        public Device GetDevice(string clientId, string deviceKey)
        {
            List<Dictionary<string, string>> list = this._database.Query("select * from Devices where ClientId=@clientId and  DeviceKey = @deviceKey", new Dictionary<string, object>()
      {{"@clientId",clientId},{"@deviceKey",deviceKey}});
            Device device = null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                device = new Device
                         {
                             Id = int.Parse(dictionary["Id"]),
                             ClientId = dictionary["ClientId"],
                             IsActived = bool.Parse(dictionary["IsActived"]),
                             DeviceKey = dictionary["DeviceKey"],
                             DeviceSecret = dictionary["DeviceSecret"],
                             DeviceName = dictionary["DeviceName"],
                             DeviceDescription = dictionary["DeviceDescription"],
                             SerialNumber = dictionary["SerialNumber"],
                             IMEI = dictionary["IMEI"],
                             Manufacturer = dictionary["Manufacturer"],
                             Model = dictionary["Model"],
                             Platform = dictionary["Platform"],
                             PlatformVersion = dictionary["PlatformVersion"],
                             RequestDeviceId = new int?()
                         };
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
            }
            return device;
        }

        public IEnumerable<Client> GetAllClients()
        {
            IList<Client> list1 = new List<Client>();
            List<Dictionary<string, string>> list2 = _database.Query("Select * from Clients");
            if (list2 != null)
            {
                foreach (var dictionary in list2)
                {
                    var client = new Client
                                 {
                                     Id = dictionary["Id"],
                                     Secret = dictionary["Secret"],
                                     Name = dictionary["Name"]
                                 };
                    if (int.Parse(dictionary["ApplicationType"]) == 1)
                        client.ApplicationType = ApplicationTypes.NativeConfidential;
                    else if (int.Parse(dictionary["ApplicationType"]) == 0)
                        client.ApplicationType = ApplicationTypes.JavaScript;
                    client.Active = int.Parse(dictionary["Active"]) == 1;
                    client.RefreshTokenLifeTime = int.Parse(dictionary["RefreshTokenLifeTime"]);
                    client.AllowedOrigin = dictionary["AllowedOrigin"];
                    list1.Add(client);
                }
            }
            return list1;
        }
    }
}