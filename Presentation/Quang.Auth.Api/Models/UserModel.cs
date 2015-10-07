using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quang.Auth.Api.Models
{
    public class FilterUserModel
    {
        public long? GroupId { get; set; }

        public string Keyword { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        //public string OrderBy { get; set; }

    }

    public class UserModel
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string PasswordHash { get; set; }

        public IEnumerable<Group> UserGroups { get; set; }

        public bool UpdateGroups { get; set; }
    }

    public class CheckUserExistInput
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
    public class CheckUserExistOutput
    {
        public int Check { get; set; }
    }
    public class UserAppModel
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public bool IsActive { get; set; }

        public string ApiName { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        public string AppHosts { get; set; }

        public string AppIps { get; set; }
    }
    public class GenerateUserAppApiKeyModel
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}