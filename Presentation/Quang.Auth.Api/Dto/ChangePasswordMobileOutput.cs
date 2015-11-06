using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Dto
{
    public class ChangePasswordMobileOutput
    {
        public int Status { get; set; }

        public string ErrorMsg { get; set; }
    }
    public class GetMobileProfileOutput
    {
        public string DisplayName { get; set; }

        public string Cmnd { get; set; }

        public string Email { get; set; }

        public string CongTy { get; set; }

        public string DiaChi { get; set; }

        public string MaSoThue { get; set; }

        public string Avatar { get; set; }
    }
    public class GetOneDeviceByKeyInput
    {
        public string ClientId { get; set; }

        public string DeviceKey { get; set; }
    }
    public class GetOneDeviceOutput
    {
        public Device Device { get; set; }
    }
    public class GetOneGroupInput
    {
        public int Id { get; set; }
    }
    public class GetOneGroupOutput
    {
        public Group Group { get; set; }
    }
    public class GetOneLoginHistoryOutput
    {
        public LoginHistory LoginHistory { get; set; }
    }
    public class GetOnePermissionOutput
    {
        public Permission Permission { get; set; }
    }
    public class GetOneTermInput
    {
        public int Id { get; set; }
    }
    public class GetOneTermOutput
    {
        public Term Term { get; set; }
    }
    public class GetOneUserAppOutput
    {
        public UserApp UserApp { get; set; }
    }
    public class GetOneUserInput
    {
        public string Id { get; set; }
    }
    public class GetOneUserOutput
    {
        public User User { get; set; }
    }
    public class GetPermissionGrantsOutput
    {
        public IEnumerable<PermissionGrant> AllowGrants { get; set; }

        public IEnumerable<PermissionGrant> DenyGrants { get; set; }
    }
   
 
    public class InformNewAppInput
    {
        public string ClientId { get; set; }

        public string DeviceKey { get; set; }

        public string SerialNumber { get; set; }

        public string IMEI { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
    public class InformNewAppOutput
    {
        public int Status { get; set; }

        public string DeviceName { get; set; }
    }
    public class InsertLoginHistoryInput
    {
        public long Id { get; set; }

        public int Type { get; set; }

        public string UserName { get; set; }

        public DateTime LoginTime { get; set; }

        public int LoginStatus { get; set; }

        public string RefreshToken { get; set; }

        public string AppId { get; set; }

        public string ClientUri { get; set; }

        public string ClientIP { get; set; }

        public string ClientUA { get; set; }

        public string ClientDevice { get; set; }

        public string ClientApiKey { get; set; }
    }
    public class IsExistDeviceInput
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public string DeviceKey { get; set; }
    }
    public class IsExistDeviceIOutput
    {
        public int Check { get; set; }
    }
}