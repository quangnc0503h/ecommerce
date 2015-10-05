using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class FilterPermissionModel
    {
        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }

    public class PermissionModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class GetOneInputModel
    {
        public long Id { get; set; }
    }

    public class NotificationResultModel
    {
        public int Status { get; set; }
    }

    public class DeleteInputModel
    {
        public List<long> Ids { get; set; }
    }

    public class GetPermissionGrantsOutputModel
    {
        public IEnumerable<PermissionGrant> AllowGrants { get; set; }
        public IEnumerable<PermissionGrant> DenyGrants { get; set; }
    }

    public class PermissionGrantsModel
    {
        public long PermissionId { get; set; }
        public IEnumerable<PermissionGrant> AllowGrants { get; set; }
        public IEnumerable<PermissionGrant> DenyGrants { get; set; }
    }

    public class UserPermissionModel
    {
        public long UserId { get; set; }
        public IEnumerable<long> PermissionIds { get; set; }
    }

    public class GroupPermissionModel
    {
        public long GroupId { get; set; }
        public IEnumerable<long> PermissionIds { get; set; }
    }
}