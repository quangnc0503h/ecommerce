using Quang.Auth.Entities;
using Quang.Common.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Dto
{
    public class CheckUserExistInput
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }
    public class CheckUserExistOutput
    {
        public int Check { get; set; }
    }

    public class CreateDeviceInput
    {
        public string ClientId { get; set; }

        public int? RequestDeviceId { get; set; }

        public bool IsActived { get; set; }

        public string DeviceKey { get; set; }

        public string DeviceSecret { get; set; }

        public string DeviceName { get; set; }

        public string DeviceDescription { get; set; }

        public string SerialNumber { get; set; }

        public string IMEI { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
    public class CreateGroupInput
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    public class CreateGroupOutput
    {
        public int Status { get; set; }
    }
    public class CreateMobileUserInput
    {
        public string Mobile { get; set; }
    }
    public class CreateMobileUserOutput
    {
        public int Status { get; set; }

        public string MobileMsg { get; set; }
    }

    public class CreatePermissionInput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class CreateTermInput
    {
        public int Id { get; set; }

        public string RoleKey { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class CreateTermOutput
    {
        public int Status { get; set; }
    }
    public class ListRoleOptionsOutput
    {
        public IEnumerable<ActionRoleItem> Options { get; set; }
    }
    public class LogoutMobileOutput
    {
        public int Status { get; set; }

        public string ErrorMsg { get; set; }
    }
   
    public class ResultUpdateOutput
    {
        public int Status { get; set; }
    }
    public class SetMobilePasswordInput
    {
        public string Mobile { get; set; }
    }
    public class SetMobilePasswordOutput
    {
        public int Status { get; set; }

        public string MobileMsg { get; set; }
    }
    public class UpdateDeviceInput
    {
        public int Id { get; set; }

        public string ClientId { get; set; }

        public int? RequestDeviceId { get; set; }

        public bool IsActived { get; set; }

        public string DeviceKey { get; set; }

        public string DeviceSecret { get; set; }

        public string DeviceName { get; set; }

        public string DeviceDescription { get; set; }

        public string SerialNumber { get; set; }

        public string IMEI { get; set; }

        public string Manufacturer { get; set; }

        public string Model { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
    public class UpdateGroupGrantInput
    {
        public int GroupId { get; set; }

        public IEnumerable<GrantGroupTerm> GroupGrants { get; set; }
    }
    public class UpdateGroupInput
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    public class UpdateGroupOutput
    {
        public int Status { get; set; }
    }
    public class UpdateGroupPermissionInput
    {
        public int GroupId { get; set; }

        public IEnumerable<int> PermissionIds { get; set; }
    }
    public class UpdateMobileProfileInput
    {
        public string DisplayName { get; set; }

        public string Cmnd { get; set; }

        public string Email { get; set; }

        public string CongTy { get; set; }

        public string DiaChi { get; set; }

        public string MaSoThue { get; set; }

        public string Avatar { get; set; }
    }
    public class UpdateMobileProfileOutput
    {
        public int Status { get; set; }
    }
    public class UpdatePermissionGrantsInput
    {
        public int PermissionId { get; set; }

        public IEnumerable<PermissionGrant> AllowGrants { get; set; }

        public IEnumerable<PermissionGrant> DenyGrants { get; set; }
    }
    public class UpdatePermissionInput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    public class UpdateTermInput
    {
        public int Id { get; set; }

        public string RoleKey { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
    public class UpdateTermOutput
    {
        public int Status { get; set; }
    }
    public class UpdateUserAppInput
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string ApiName { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public string AppHosts { get; set; }

        public string AppIps { get; set; }
    }
    public class UpdateUserGrantInput
    {
        public int UserId { get; set; }

        public IEnumerable<GrantUserTerm> UserGrants { get; set; }
    }
    public class UpdateUserInput
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public IEnumerable<Group> UserGroups { get; set; }
    }
    public class UpdateUserOutput
    {
        public int Status { get; set; }
    }
    public class UpdateUserPermissionInput
    {
        public int UserId { get; set; }

        public IEnumerable<int> PermissionIds { get; set; }
    }
}