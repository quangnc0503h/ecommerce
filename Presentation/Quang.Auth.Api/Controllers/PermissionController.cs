using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/Permission")]
    public class PermissionController : BaseApiController
    {
        private readonly IPermissionBll _permissionBll;

        public PermissionController()
        {
        }

        public PermissionController(IPermissionBll permissionBll)
        {
            _permissionBll = permissionBll;
        }

        [HttpPost]
        [Route("GetAll")]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput filter)
        {
            return await _permissionBll.GetAll(filter);
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetOnePermission")]
        public async Task<GetOnePermissionOutput> GetOnePermission(GetByIdInput input)
        {
            var result = await _permissionBll.GetOnePermission(input.Id);
            return new GetOnePermissionOutput
                   {
                Permission = result
            };
        }

        [Route("CreatePermission")]
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<ResultUpdateOutput> CreatePermission(CreatePermissionInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int test = await _permissionBll.InsertPermission(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
        [Route("UpdatePermission")]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<ResultUpdateOutput> UpdatePermission(UpdatePermissionInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int test = await _permissionBll.UpdatePermission(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("DeletePermission")]
        public async Task<ResultUpdateOutput> DeletePermission(DeletePermissionInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int test = await _permissionBll.DeletePermission(input.Ids);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpGet]
        [Route("ListAllPermission")]
        public async Task<DanhSachPermissionOutput> ListAllPermission()
        {
            IEnumerable<Permission> permissions = await _permissionBll.GetAllPermissions();
            var result = new DanhSachPermissionOutput
                         {
                DanhSachPermissions = permissions
            };
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetPermissionGrants")]
        public async Task<GetPermissionGrantsOutput> GetPermissionGrants(GetByIdInput input)
        {
            GetPermissionGrantsOutput result = await _permissionBll.GetPermissionGrants(input.Id);
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("UpdatePermissionGrants")]
        public async Task<ResultUpdateOutput> UpdatePermissionGrants(UpdatePermissionGrantsInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int test = await _permissionBll.UpdatePermissionGrants(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [Route("GetUserPermissions")]
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        public async Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(GetByIdInput input)
        {
            IEnumerable<PermissionItemGrant> result = await _permissionBll.GetUserPermissions(input.Id);
            return result;
        }

        [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [HttpPost]
        [Route("GetGroupPermissions")]
        public async Task<IEnumerable<PermissionItemGrant>> GetGroupPermissions(GetByIdInput input)
        {
            IEnumerable<PermissionItemGrant> result = await _permissionBll.GetGroupPermissions(input.Id);
            return result;
        }

        [Route("UpdateUserPermissions")]
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        public async Task<ResultUpdateOutput> UpdateUserPermissions(UpdateUserPermissionInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int status = await _permissionBll.UpdateUserPermissions(input);
            if (status > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Grant)]
        [Route("UpdateGroupPermissions")]
        public async Task<ResultUpdateOutput> UpdateGroupPermissions(UpdateGroupPermissionInput input)
        {
            var result = new ResultUpdateOutput
                         {
                Status = 1
            };
            int status = await _permissionBll.UpdateGroupPermissions(input);
            if (status > 0)
                result.Status = 0;
            return result;
        }
    }
}
