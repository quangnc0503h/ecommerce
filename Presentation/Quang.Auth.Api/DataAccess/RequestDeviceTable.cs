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
        private readonly MySQLDatabase _database;

        public RequestDeviceTable(MySQLDatabase database)
        {
            _database = database;
        }

        public int Delete(int id)
        {
            return _database.Execute("Delete from RequestDevices where Id = @id", new Dictionary<string, object>()
      {
        {
          "@id",
          id
        }
      });
        }

        public int Delete(IEnumerable<int> ids)
        {
            return _database.Execute("Delete from RequestDevices where Id in (" + string.Join(",", ids.ToArray()) + ")", new Dictionary<string, object>());
        }

        public int GetTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            string commandText = "select count(*) from RequestDevices where IsApproved = false AND (Id = @id OR DeviceKey LIKE @param)";
            var parameters = new Dictionary<string, object>
                             {
                                 {
                                     "@param","%" + Utils.EncodeForLike(keyword) + "%"
                                 }
                             };
            int num = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                var match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@id", num);
            if (!string.IsNullOrEmpty(clientId))
            {
                commandText += " and ClientId = @clientId";
                parameters.Add("@clientId", clientId);
            }
            if (dateFrom.HasValue)
            {
                commandText += " and Created >= @dateFrom";
                parameters.Add("@dateFrom", dateFrom.Value.ToString("yyyy-MM-dd"));
            }
            if (dateTo.HasValue)
            {
                commandText += " and Created >= @dateTo";
                parameters.Add("@dateTo", dateTo.Value.ToString("yyyy-MM-dd"));
            }
            return int.Parse(_database.QueryValue(commandText, parameters).ToString());
        }

        public IEnumerable<RequestDevice> GetPaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo)
        {
            var parameters = new Dictionary<string, object>();
            string str = "select *, EXISTS(SELECT * FROM TrustDevices WHERE TrustDevices.DeviceSerial = RequestDevices.SerialNumber) as IsTrust\n                        from RequestDevices \n                        where IsApproved = false and (Id = @id or DeviceKey LIKE @param)";
            int num1 = 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                Match match = Regex.Match(keyword, "^R([\\d]{4,})$");
                if (match.Success)
                    num1 = int.Parse(match.Groups[1].Value);
            }
            parameters.Add("@id", num1);
            parameters.Add("@param", "%" + Utils.EncodeForLike(keyword) + "%");
            if (!string.IsNullOrEmpty(clientId))
            {
                str += " and ClientId = @clientId";
                parameters.Add("@clientId", clientId);
            }
            if (dateFrom.HasValue)
            {
                str += " and Created >= @dateFrom";
                parameters.Add("@dateFrom", dateFrom.Value.ToString("yyyy-MM-dd 00:00:00"));
            }
            if (dateTo.HasValue)
            {
                str += " and Created <= @dateTo";
                parameters.Add("@dateTo", dateTo.Value.ToString("yyyy-MM-dd 23:59:59"));
            }
            string commandText = str + " order by Created limit @rowNumber, @pageSize";
            parameters.Add("@rowNumber", pageSize * pageNumber);
            parameters.Add("@pageSize", pageSize);
            var list = new List<RequestDevice>();
            foreach (Dictionary<string, string> dictionary in _database.Query(commandText, parameters))
            {
                var requestDevice = new RequestDevice
                                    {
                                        Id = int.Parse(dictionary["Id"]),
                                        ClientId = dictionary["ClientId"],
                                        IsApproved = bool.Parse(dictionary["IsApproved"]),
                                        DeviceKey = dictionary["DeviceKey"],
                                        SerialNumber = dictionary["SerialNumber"],
                                        IMEI = dictionary["IMEI"],
                                        Manufacturer = dictionary["Manufacturer"],
                                        Model = dictionary["Model"],
                                        Platform = dictionary["Platform"],
                                        PlatformVersion = dictionary["PlatformVersion"],
                                        Created = DateTime.Parse(dictionary["Created"])
                                    };
                if (!string.IsNullOrEmpty(dictionary["Updated"]))
                    requestDevice.Updated = DateTime.Parse(dictionary["Updated"]);
                if (!string.IsNullOrEmpty(dictionary["IsTrust"]))
                {
                    int num2 = int.Parse(dictionary["IsTrust"]);
                    requestDevice.IsTrust = num2 > 0;
                }
                list.Add(requestDevice);
            }
            return list;
        }

        public int CreateRequestDevice(InformNewAppInput newApp)
        {
            return _database.Execute("Insert into RequestDevices (Id, ClientId, DeviceKey, SerialNumber, IMEI, Manufacturer, Model, Platform, PlatformVersion, IsApproved, Created, Updated) " + "values (@id, @clientId, @deviceKey, @serialNumber, @iMEI, @manufacturer, @model, @platform, @platformVersion, @isApproved, @created, @updated)", new Dictionary<string, object>()
      {
        {
          "@id",
          null
        },
        {
          "@clientId",
          newApp.ClientId
        },
        {
          "@deviceKey",
          newApp.DeviceKey
        },
        {
          "@serialNumber",
          newApp.SerialNumber
        },
        {
          "@iMEI",
          newApp.IMEI
        },
        {
          "@manufacturer",
          newApp.Manufacturer
        },
        {
          "@model",
          newApp.Model
        },
        {
          "@platform",
          newApp.Platform
        },
        {
          "@platformVersion",
          newApp.PlatformVersion
        },
        {
          "@isApproved",
          false
        },
        {
          "@created",
          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        },
        {
          "@updated",
          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public int UpdateRequestDevice(InformNewAppInput newApp)
        {
            return UpdateRequestDevice(newApp, false);
        }

        public int UpdateRequestDevice(InformNewAppInput newApp, bool isApproved)
        {
            if (isApproved)
                return _database.Execute("Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, IsApproved = @isApproved, Updated = @updated Where DeviceKey = @deviceKey", new Dictionary<string, object>()
        {
          {
            "@clientId",
            newApp.ClientId
          },
          {
            "@deviceKey",
            newApp.DeviceKey
          },
          {
            "@serialNumber",
            newApp.SerialNumber
          },
          {
            "@iMEI",
            newApp.IMEI
          },
          {
            "@manufacturer",
            newApp.Manufacturer
          },
          {
            "@model",
            newApp.Model
          },
          {
            "@platform",
            newApp.Platform
          },
          {
            "@platformVersion",
            newApp.PlatformVersion
          },
          {
            "@isApproved",
            isApproved ? 1 : 0
          },
          {
            "@updated",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });
            return _database.Execute("Update RequestDevices Set ClientId=@clientId, SerialNumber=@serialNumber, IMEI=@iMEI, Manufacturer=@manufacturer, Model=@model, Platform=@platform, PlatformVersion=@platformVersion, Created = @created Where DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>()
      {
        {
          "@clientId",
          newApp.ClientId
        },
        {
          "@deviceKey",
          newApp.DeviceKey
        },
        {
          "@serialNumber",
          newApp.SerialNumber
        },
        {
          "@iMEI",
          newApp.IMEI
        },
        {
          "@manufacturer",
          newApp.Manufacturer
        },
        {
          "@model",
          newApp.Model
        },
        {
          "@platform",
          newApp.Platform
        },
        {
          "@platformVersion",
          newApp.PlatformVersion
        },
        {
          "@isApproved",
          isApproved ? 1 : 0
        },
        {
          "@created",
          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public int UpdateRequestDeviceStatus(string clientId, string deviceKey, bool isApproved)
        {
            if (isApproved)
                return _database.Execute("Update RequestDevices Set IsApproved = @isApproved, Updated = @updated Where ClientId=@clientId and DeviceKey = @deviceKey", new Dictionary<string, object>()
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
            "@isApproved",
            isApproved ? 1 : 0
          },
          {
            "@updated",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
          }
        });
            return _database.Execute("Update RequestDevices Set Created = @created Where ClientId=@clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>
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
          "@isApproved",
          0
        },
        {
          "@created",
          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
      });
        }

        public bool IsExistRequestDevice(string clientId, string deviceKey)
        {
            return int.Parse(_database.QueryValue("select count(*) from RequestDevices where ClientId = @clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>
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
          "@isApproved",
          false
        }
      }).ToString()) > 0;
        }

        public RequestDevice GetRequestDevice(string clientId, string deviceKey)
        {
            List<Dictionary<string, string>> list = _database.Query("select * from RequestDevices where ClientId=@clientId and DeviceKey = @deviceKey and IsApproved = @isApproved", new Dictionary<string, object>
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
          "@isApproved",
          false
        }
      });
            var requestDevice = (RequestDevice)null;
            if (list != null && list.Count == 1)
            {
                Dictionary<string, string> dictionary = list[0];
                requestDevice = new RequestDevice
                                {
                                    Id = int.Parse(dictionary["Id"]),
                                    ClientId = dictionary["ClientId"],
                                    IsApproved = bool.Parse(dictionary["IsApproved"]),
                                    DeviceKey = dictionary["DeviceKey"],
                                    SerialNumber = dictionary["SerialNumber"],
                                    IMEI = dictionary["IMEI"],
                                    Manufacturer = dictionary["Manufacturer"],
                                    Model = dictionary["Model"],
                                    Platform = dictionary["Platform"],
                                    PlatformVersion = dictionary["PlatformVersion"],
                                    Created = DateTime.Parse(dictionary["Created"]),
                                    Updated = DateTime.Parse(dictionary["Updated"])
                                };
            }
            return requestDevice;
        }
    }
}