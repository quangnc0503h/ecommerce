using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Dto
{
    public class CheckDeviceInput
    {
        public string ClientId { get; set; }

        public string DeviceKey { get; set; }
    }
    public class CheckDeviceOutput
    {
        public int Status { get; set; }

        public string Description { get; set; }
    }
    public class CreateUserInput
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

    public class CreateUserOutput
    {
        public int Status { get; set; }
    }
    public class DanhSachDeviceOutput
    {
        public IEnumerable<Device> DanhSachDevices { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachGroupOutput
    {
        public IEnumerable<Group> DanhSachGroups { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachLoginHistoryOutput
    {
        public IEnumerable<LoginHistory> DanhSachLoginHistories { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachPermissionOutput
    {
        public IEnumerable<Permission> DanhSachPermissions { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachRequestDeviceOutput
    {
        public IEnumerable<RequestDevice> DanhSachRequestDevices { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachTermOutput
    {
        public IEnumerable<Term> DanhSachTerms { get; set; }

        public long TotalCount { get; set; }
    }
    public class DanhSachUserOutput
    {
        public IEnumerable<User> DanhSachUsers { get; set; }

        public long TotalCount { get; set; }
    }

    public class DeleteDeviceInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeleteGroupInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeleteGroupOutput
    {
        public int Status { get; set; }
    }
    public class DeleteLoginHistoryInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeletePermissionInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeleteTermInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeleteTermOutput
    {
        public int Status { get; set; }
    }
    public class DeleteUserInput
    {
        public List<int> Ids { get; set; }
    }
    public class DeleteUserOutput
    {
        public int Status { get; set; }
    }
    public class FilterDeviceInput
    {
        public string ClientId { get; set; }

        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterGroupInput
    {
        public int? ParentId { get; set; }

        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterInput
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterLoginHistoryInput
    {
        public int[] Type { get; set; }

        public string UserName { get; set; }

        public DateTime? LoginTimeFrom { get; set; }

        public DateTime? LoginTimeTo { get; set; }

        public int[] LoginStatus { get; set; }

        public string RefreshToken { get; set; }

        public string[] AppId { get; set; }

        public string ClientUri { get; set; }

        public string ClientIP { get; set; }

        public string ClientUA { get; set; }

        public string ClientDevice { get; set; }

        public string ClientApiKey { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterPermissionInput
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterRequestDeviceInput
    {
        public string ClientId { get; set; }

        public string Keyword { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterTermInput
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class FilterUserInput
    {
        public int? GroupId { get; set; }

        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
    public class GenerateUserAppApiKeyOutput
    {
        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }
    }
    public class GetByIdInput
    {
        public int Id { get; set; }
    }
}