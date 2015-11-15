using System;
using System.Collections.Generic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.DataAccess
{
    public interface IRequestDeviceTable
    {
        int Delete(int id);

        int Delete(IEnumerable<int> ids);

        int GetTotal(string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo);

        IEnumerable<RequestDevice> GetPaging(int pageSize, int pageNumber, string clientId, string keyword, DateTime? dateFrom, DateTime? dateTo);

        int CreateRequestDevice(InformNewAppInput newApp);

        int UpdateRequestDevice(InformNewAppInput newApp);

        int UpdateRequestDevice(InformNewAppInput newApp, bool isApproved);

        int UpdateRequestDeviceStatus(string clientId, string deviceKey, bool isApproved);

        bool IsExistRequestDevice(string clientId, string deviceKey);

        RequestDevice GetRequestDevice(string clientId, string deviceKey);
    }
}