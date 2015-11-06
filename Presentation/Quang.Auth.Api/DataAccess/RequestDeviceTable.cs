using AspNet.Identity.MySQL;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Quang.Auth.Api.DataAccess
{
    public class RequestDeviceTable : IRequestDeviceTable
    {
        private MySQLDatabase _database;

        public RequestDeviceTable(MySQLDatabase database)
        {
            this._database = database;
        }

        public int Delete(int id)
        {
            return this._database.Execute("Delete from RequestDevices where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) id
        }
      });
        }

        public int Delete(IEnumerable<int> ids)
        {
            return this._database.Execute("Delete from RequestDevices where Id in (" + string.Join<int>(",", (IEnumerable<int>)Enumerable.ToArray<int>(ids)) + ")", new Dictionary<string, object>());
        }

        public int GetTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            string commandText = "select count(*) from RequestDevices where IsApproved = false AND (Id = @id OR DeviceKey LIKE @param)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
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
            return int.Parse(this._database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<RequestDevice> GetPaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string str = "select *, EXISTS(SELECT * FROM TrustDevices WHERE TrustDevices.DeviceSerial = RequestDevices.SerialNumber) as IsTrust\n                        from RequestDevices \n                        where IsApproved = false and (Id = @id or DeviceKey LIKE @param)";
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
            List<RequestDevice> list = new List<RequestDevice>();
            foreach (Dictionary<string, string> dictionary in this._database.Query(commandText, parameters))
            {
                RequestDevice requestDevice = new RequestDevice();
                requestDevice.Id = int.Parse(dictionary["Id"]);
                requestDevice.ClientId = dictionary["ClientId"];
                requestDevice.IsApproved = bool.Parse(dictionary["IsApproved"]);
                requestDevice.DeviceKey = dictionary["DeviceKey"];
                requestDevice.SerialNumber = dictionary["SerialNumber"];
                requestDevice.IMEI = dictionary["IMEI"];
                requestDevice.Manufacturer = dictionary["Manufacturer"];
                requestDevice.Model = dictionary["Model"];
                requestDevice.Platform = dictionary["Platform"];
                requestDevice.PlatformVersion = dictionary["PlatformVersion"];
                requestDevice.Created = DateTime.Parse(dictionary["Created"]);
                if (!string.IsNullOrEmpty(dictionary["Updated"]))
                    requestDevice.Updated = new DateTime?(DateTime.Parse(dictionary["Updated"]));
                if (!string.IsNullOrEmpty(dictionary["IsTrust"]))
                {
                    int num2 = int.Parse(dictionary["IsTrust"]);
                    requestDevice.IsTrust = num2 > 0;
                }
                list.Add(requestDevice);
            }
            return (IEnumerable<RequestDevice>)list;
        }

        public int CreateRequestDevice(InformNewAppInput newApp)
        {
            return this._database.Execute("Insert into RequestDevices (Id, ClientId, DeviceKey, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, IsApproved, Created, Updated) " + "values (@id, @clientId, @deviceKey, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @isApproved, @created, @updated)", new Dictionary<string, object>()
      {
        {
          "@id",
          (object) null
        },
        {
          "@clientId",
          (object) newApp.ClientId
        },
        {
          "@deviceKey",
          (object) newApp.DeviceKey
        },
        {
          "@serialNumber",
          (object) newApp.SerialNumber
        },
        {
          "@iMEI",
          (object) newApp.IMEI
        },
        {
          "@manufacturer",
          (object) newApp.Manufacturer
        },
        {
          "@model",
          (object) newApp.Model
        },
        {
          "@platform",
          (object) newApp.Platform
        },
        {
          "@platformVersion",
          (object) newApp.PlatformVersion
        },
        {
          "@isApproved",
          (object) false
        },
        {
          "@created",
          (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        },
        {
          "@updated",
          (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public int UpdateRequestDevice(InformNewAppInput newApp)
        {
            return this.UpdateRequestDevice(newApp, false);
        }

        public int UpdateRequestDevice(InformNewAppInput newApp, bool isApproved)
        {
            if (isApproved)
                return this._database.Execute("Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, IsApproved = @isApproved, Updated = @updated Where DeviceKey = @deviceKey", new Dictionary<string, object>()
        {
          {
            "@clientId",
            (object) newApp.ClientId
          },
          {
            "@deviceKey",
            (object) newApp.DeviceKey
          },
          {
            "@serialNumber",
            (object) newApp.SerialNumber
          },
          {
            "@iMEI",
            (object) newApp.IMEI
          },
          {
            "@manufacturer",
            (object) newApp.Manufacturer
          },
          {
            "@model",
            (object) newApp.Model
          },
          {
            "@platform",
            (object) newApp.Platform
          },
          {
            "@platformVersion",
            (object) newApp.PlatformVersion
          },
          {
            "@isApproved",
            (object)  (isApproved ? 1 : 0)
          },
          {
            "@updated",
            (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });
            return this._database.Execute("Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, Created = @created Where DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>()
      {
        {
          "@clientId",
          (object) newApp.ClientId
        },
        {
          "@deviceKey",
          (object) newApp.DeviceKey
        },
        {
          "@serialNumber",
          (object) newApp.SerialNumber
        },
        {
          "@iMEI",
          (object) newApp.IMEI
        },
        {
          "@manufacturer",
          (object) newApp.Manufacturer
        },
        {
          "@model",
          (object) newApp.Model
        },
        {
          "@platform",
          (object) newApp.Platform
        },
        {
          "@platformVersion",
          (object) newApp.PlatformVersion
        },
        {
          "@isApproved",
          (object) (isApproved ? 1 : 0)
        },
        {
          "@created",
          (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public int UpdateRequestDeviceStatus(string clientId, string deviceKey, bool isApproved)
        {
            if (isApproved)
                return this._database.Execute("Update RequestDevices Set IsApproved = @isApproved, Updated = @updated Where ClientId=@clientId and DeviceKey = @deviceKey", new Dictionary<string, object>()
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
            "@isApproved",
            (object) (isApproved ? 1 : 0)
          },
          {
            "@updated",
            (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });
            return this._database.Execute("Update RequestDevices Set Created = @created Where ClientId=@clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>()
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
          "@isApproved",
          (object)  (isApproved ? 1 : 0)
        },
        {
          "@created",
          (object) DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public bool IsExistRequestDevice(string clientId, string deviceKey)
        {
            return int.Parse(this._database.QueryValue("select count(*) from RequestDevices where ClientId = @clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>()
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
          "@isApproved",
          (object) false
        }
      }).ToString()) > 0;
        }

        public RequestDevice GetRequestDevice(string clientId, string deviceKey)
        {
            List<Dictionary<string, string>> list = this._database.Query("select * from RequestDevices where ClientId=@clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>()
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
          "@isApproved",
          (object) false
        }
      });
            RequestDevice requestDevice = (RequestDevice)null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                requestDevice = new RequestDevice();
                requestDevice.Id = int.Parse(dictionary["Id"]);
                requestDevice.ClientId = dictionary["ClientId"];
                requestDevice.IsApproved = bool.Parse(dictionary["IsApproved"]);
                requestDevice.DeviceKey = dictionary["DeviceKey"];
                requestDevice.SerialNumber = dictionary["SerialNumber"];
                requestDevice.IMEI = dictionary["IMEI"];
                requestDevice.Manufacturer = dictionary["Manufacturer"];
                requestDevice.Model = dictionary["Model"];
                requestDevice.Platform = dictionary["Platform"];
                requestDevice.PlatformVersion = dictionary["PlatformVersion"];
                requestDevice.Created = DateTime.Parse(dictionary["Created"]);
                requestDevice.Updated = new DateTime?(DateTime.Parse(dictionary["Updated"]));
            }
            return requestDevice;
        }
    }
}