using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;

using Quang.Auth.Entities;
using Quang.Common.Auth;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/Permission")]
    public class PermissionController : ApiController
    {
        private IPermissionBll _permissionBll;
        private ITermBll _termBll;

        public PermissionController()
        {
        }

        public PermissionController(IPermissionBll permissionBll, ITermBll termBll)
        {
            this._permissionBll = permissionBll;
            this._termBll = termBll;
        }

        [HttpPost]
        [Route("GetAll")]
       // [AppAuthorize(Roles = "120")]
        public async Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput filter)
        {
            return await this._permissionBll.GetAll(filter);
        }

        [HttpPost]
        //[AppAuthorize(Roles = "120")]
        [Route("GetOnePermission")]
        public async Task<GetOnePermissionOutput> GetOnePermission(GetByIdInput input)
        {
            Permission result = await this._permissionBll.GetOnePermission(input.Id);
            return new GetOnePermissionOutput()
            {
                Permission = result
            };
        }

        [Route("CreatePermission")]
        [HttpPost]
       // [AppAuthorize(Roles = "120")]
        public async Task<ResultUpdateOutput> CreatePermission(CreatePermissionInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._permissionBll.InsertPermission(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
        [Route("UpdatePermission")]
      //  [AppAuthorize(Roles = "120")]
        public async Task<ResultUpdateOutput> UpdatePermission(UpdatePermissionInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._permissionBll.UpdatePermission(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = "120")]
        [Route("DeletePermission")]
        public async Task<ResultUpdateOutput> DeletePermission(DeletePermissionInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._permissionBll.DeletePermission((IEnumerable<int>)input.Ids);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpGet]
        [Route("ListAllPermission")]
        public async Task<DanhSachPermissionOutput> ListAllPermission()
        {
            IEnumerable<Permission> permissions = await this._permissionBll.GetAllPermissions();
            DanhSachPermissionOutput result = new DanhSachPermissionOutput()
            {
                DanhSachPermissions = permissions
            };
            return result;
        }

        [HttpPost]
        //[AppAuthorize(Roles = "120")]
        [Route("GetPermissionGrants")]
        public async Task<GetPermissionGrantsOutput> GetPermissionGrants(GetByIdInput input)
        {
            GetPermissionGrantsOutput result = await this._permissionBll.GetPermissionGrants(input.Id);
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = "120")]
        [Route("UpdatePermissionGrants")]
        public async Task<ResultUpdateOutput> UpdatePermissionGrants(UpdatePermissionGrantsInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await this._permissionBll.UpdatePermissionGrants(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [Route("GetUserPermissions")]
        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        public async Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(GetByIdInput input)
        {
            IEnumerable<PermissionItemGrant> result = await this._permissionBll.GetUserPermissions(input.Id);
            return result;
        }

      //  [AppAuthorize(Roles = "140")]
        [HttpPost]
        [Route("GetGroupPermissions")]
        public async Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(GetByIdInput input)
        {
            IEnumerable<PermissionItemGrant> result = await this._permissionBll.GetGroupPermissions(input.Id);
            return result;
        }

        [Route("UpdateUserPermissions")]
        [HttpPost]
      //  [AppAuthorize(Roles = "140")]
        public async Task<ResultUpdateOutput> UpdateUserPermissions(UpdateUserPermissionInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int status = await this._permissionBll.UpdateUserPermissions(input);
            if (status > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        [Route("UpdateGroupPermissions")]
        public async Task<ResultUpdateOutput> UpdateGroupPermissions(UpdateGroupPermissionInput input)
        {
            ResultUpdateOutput result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int status = await this._permissionBll.UpdateGroupPermissions(input);
            if (status > 0)
                result.Status = 0;
            return result;
        }
    }
}
