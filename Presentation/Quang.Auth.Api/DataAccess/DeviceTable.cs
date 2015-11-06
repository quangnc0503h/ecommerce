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
        private MySQLDatabase _database;

        public DeviceTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public int Delete(int deviceId)
        {
            return this._database.Execute("Delete from Devices where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) deviceId
        }
      });
        }

        public int Delete(IEnumerable<int> Ids)
        {
            string commandText = "Delete from Devices where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(Ids)) + ")";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int num = this._database.Execute(commandText, parameters);
            this._database.Execute(commandText, new Dictionary<string, object>());
            return num;
        }

        public int Insert(Device device)
        {
            string commandText = "Insert into Devices (Id, ClientId, RequestDeviceId, IsActived, DeviceKey, DeviceSecret, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, DeviceName, DeviceDescription) " + "values (@id, @clientId, @requestDeviceId, @isActived, @deviceKey, @deviceSecret, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @deviceName, @deviceDescription)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (device.Id > 0)
                parameters.Add("@id", (object)device.Id);
            else
                parameters.Add("@id", (object)null);
            parameters.Add("@clientId", (object)device.ClientId);
            parameters.Add("@requestDeviceId", device.RequestDeviceId);
            parameters.Add("@isActived", (device.IsActived ? 1 : 0));
            parameters.Add("@deviceKey", (object)device.DeviceKey);
            parameters.Add("@deviceSecret", (object)device.DeviceSecret);
            parameters.Add("@serialNumber", (object)device.SerialNumber);
            parameters.Add("@iMEI", (object)device.IMEI);
            parameters.Add("@manufacturer", (object)device.Manufacturer);
            parameters.Add("@model", (object)device.Model);
            parameters.Add("@platform", (object)device.Platform);
            parameters.Add("@platformVersion", (object)device.PlatformVersion);
            parameters.Add("@deviceName", (object)device.DeviceName);
            parameters.Add("@deviceDescription", (object)device.DeviceDescription);
            return this._database.Execute(commandText, parameters);
        }

        public int Update(Device device)
        {
            return this._database.Execute("Update Devices Set " + "ClientId = @clientId, " + "IsActived = @isActived, " + "DeviceKey = @deviceKey, " + "DeviceSecret = @deviceSecret, " + "SerialNumber = @serialNumber, " + "IMEI = @iMEI, " + "Manufacturer = @manufacturer, " + "Model = @model, " + "Platform = @platform, " + "PlatformVersion = @platformVersion, " + "DeviceName = @deviceName, " + "DeviceDescription = @deviceDescription " + "where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) device.Id
        },
        {
          "@clientId",
          (object) device.ClientId
        },
        {
          "@isActived",
          (object) (device.IsActived ? 1 : 0)
        },
        {
          "@deviceKey",
          (object) device.DeviceKey
        },
        {
          "@deviceSecret",
          (object) device.DeviceSecret
        },
        {
          "@serialNumber",
          (object) device.SerialNumber
        },
        {
          "@iMEI",
          (object) device.IMEI
        },
        {
          "@manufacturer",
          (object) device.Manufacturer
        },
        {
          "@model",
          (object) device.Model
        },
        {
          "@platform",
          (object) device.Platform
        },
        {
          "@platformVersion",
          (object) device.PlatformVersion
        },
        {
          "@deviceName",
          (object) device.DeviceName
        },
        {
          "@deviceDescription",
          (object) device.DeviceDescription
        }
      });
        }

        public IEnumerable<Device> GetAllDevices()
        {
            IList<Device> list = (IList<Device>)new List<Device>();
            foreach (Dictionary<string, string> dictionary in this._database.Query("Select * from Devices order by Id"))
            {
                Device device = new Device();
                device.Id = int.Parse(dictionary["Id"]);
                device.ClientId = dictionary["ClientId"];
                device.IsActived = bool.Parse(dictionary["IsActived"]);
                device.DeviceKey = dictionary["DeviceKey"];
                device.DeviceSecret = dictionary["DeviceSecret"];
                device.DeviceName = dictionary["DeviceName"];
                device.DeviceDescription = dictionary["DeviceDescription"];
                device.SerialNumber = dictionary["SerialNumber"];
                device.IMEI = dictionary["IMEI"];
                device.Manufacturer = dictionary["Manufacturer"];
                device.Model = dictionary["Model"];
                device.Platform = dictionary["Platform"];
                device.PlatformVersion = dictionary["PlatformVersion"];
                device.RequestDeviceId = new int?();
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
                list.Add(device);
            }
            return (IEnumerable<Device>)list;
        }

        public Device GetOneDevice(int deviceId)
        {
            Device device = (Device)null;
            List<Dictionary<string, string>> list = this._database.Query("Select * from Devices where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) deviceId
        }
      });
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                device = new Device();
                device.Id = int.Parse(dictionary["Id"]);
                device.ClientId = dictionary["ClientId"];
                device.IsActived = bool.Parse(dictionary["IsActived"]);
                device.DeviceKey = dictionary["DeviceKey"];
                device.DeviceSecret = dictionary["DeviceSecret"];
                device.DeviceName = dictionary["DeviceName"];
                device.DeviceDescription = dictionary["DeviceDescription"];
                device.SerialNumber = dictionary["SerialNumber"];
                device.IMEI = dictionary["IMEI"];
                device.Manufacturer = dictionary["Manufacturer"];
                device.Model = dictionary["Model"];
                device.Platform = dictionary["Platform"];
                device.PlatformVersion = dictionary["PlatformVersion"];
                device.RequestDeviceId = new int?();
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
            }
            return device;
        }

        public int GetTotal(string clientId, string keyword)
        {
            string commandText = "select count(*) from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (!string.IsNullOrEmpty(clientId))
            {
                commandText += " AND ClientId=@clientId";
                parameters.Add("@clientId", (object)clientId);
            }
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@rid", (object)num);
            return int.Parse(this._database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<Device> GetPaging(int pageSize, int pageNumber, string clientId, string keyword)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string str = "select * from Devices where (DeviceName LIKE @param OR DeviceKey LIKE @param OR RequestDeviceId=@rid)";
            parameters.Add("@param", (object)("%" + Utils.EncodeForLike(keyword) + "%"));
            if (!string.IsNullOrEmpty(clientId))
            {
                str += " AND ClientId=@clientId";
                parameters.Add("@clientId", (object)clientId);
            }
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@rid", (object)num);
            string commandText = str + " order by Id limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", (object)(pageSize * pageNumber));
            parameters.Add("@pageSize", (object)pageSize);
            List<Device> list = new List<Device>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                Device device = new Device();
                device.Id = int.Parse(dictionary["Id"]);
                device.ClientId = dictionary["ClientId"];
                device.IsActived = bool.Parse(dictionary["IsActived"]);
                device.DeviceKey = dictionary["DeviceKey"];
                device.DeviceSecret = dictionary["DeviceSecret"];
                device.DeviceName = dictionary["DeviceName"];
                device.DeviceDescription = dictionary["DeviceDescription"];
                device.SerialNumber = dictionary["SerialNumber"];
                device.IMEI = dictionary["IMEI"];
                device.Manufacturer = dictionary["Manufacturer"];
                device.Model = dictionary["Model"];
                device.Platform = dictionary["Platform"];
                device.PlatformVersion = dictionary["PlatformVersion"];
                device.RequestDeviceId = new int?();
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
                list.Add(device);
            }
            return (IEnumerable<Device>)list;
        }

        public Device GetDevice(string clientId, string deviceKey, string deviceSecret)
        {
            List<Dictionary<string, string>> list = this._database.Query("select * from Devices where ClientId=@clientId and DeviceKey = @deviceKey and DeviceSecret = @deviceSecret and IsActived = @isActived", new Dictionary<string, object>()
      {
        {
          "@clientId",
          (object) clientId
        },
        {
          "@deviceKey",
          (object) deviceKey
        },
        {
          "@deviceSecret",
          (object) deviceSecret
        },
        {
          "@isActived",
          (object) true
        }
      });
            Device device = (Device)null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                device = new Device();
                device.Id = int.Parse(dictionary["Id"]);
                device.ClientId = dictionary["ClientId"];
                device.IsActived = bool.Parse(dictionary["IsActived"]);
                device.DeviceKey = dictionary["DeviceKey"];
                device.DeviceSecret = dictionary["DeviceSecret"];
                device.DeviceName = dictionary["DeviceName"];
                device.DeviceDescription = dictionary["DeviceDescription"];
                device.SerialNumber = dictionary["SerialNumber"];
                device.IMEI = dictionary["IMEI"];
                device.Manufacturer = dictionary["Manufacturer"];
                device.Model = dictionary["Model"];
                device.Platform = dictionary["Platform"];
                device.PlatformVersion = dictionary["PlatformVersion"];
                device.RequestDeviceId = new int?();
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
            }
            return device;
        }

        public Device GetDevice(string clientId, string deviceKey)
        {
            List<Dictionary<string, string>> list = this._database.Query("select * from Devices where ClientId=@clientId and  DeviceKey = @deviceKey", new Dictionary<string, object>()
      {
        {
          "@clientId",
          (object) clientId
        },
        {
          "@deviceKey",
          (object) deviceKey
        }
      });
            Device device = (Device)null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                device = new Device();
                device.Id = int.Parse(dictionary["Id"]);
                device.ClientId = dictionary["ClientId"];
                device.IsActived = bool.Parse(dictionary["IsActived"]);
                device.DeviceKey = dictionary["DeviceKey"];
                device.DeviceSecret = dictionary["DeviceSecret"];
                device.DeviceName = dictionary["DeviceName"];
                device.DeviceDescription = dictionary["DeviceDescription"];
                device.SerialNumber = dictionary["SerialNumber"];
                device.IMEI = dictionary["IMEI"];
                device.Manufacturer = dictionary["Manufacturer"];
                device.Model = dictionary["Model"];
                device.Platform = dictionary["Platform"];
                device.PlatformVersion = dictionary["PlatformVersion"];
                device.RequestDeviceId = new int?();
                if (!string.IsNullOrEmpty(dictionary["RequestDeviceId"]))
                    device.RequestDeviceId = new int?(int.Parse(dictionary["RequestDeviceId"]));
            }
            return device;
        }

        public IEnumerable<Client> GetAllClients()
        {
            IList<Client> list1 = (IList<Client>)new List<Client>();
            List<Dictionary<string, string>> list2 = this._database.Query("Select * from Clients");
            if (list2 != null)
            {
                foreach (Dictionary<string, string> dictionary in list2)
                {
                    Client client = new Client();
                    client.Id = dictionary["Id"];
                    client.Secret = dictionary["Secret"];
                    client.Name = dictionary["Name"];
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
            return (IEnumerable<Client>)list1;
        }
    }
}