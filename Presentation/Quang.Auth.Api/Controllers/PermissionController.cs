using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
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
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<DanhSachPermissionOutput> GetAll(FilterPermissionInput filter)
        {
            return await _permissionBll.GetAll(filter);
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetOnePermission")]
        public async Task<GetOnePermissionOutput> GetOnePermission(GetByIdInput input)
        {
            Permission result = await _permissionBll.GetOnePermission(input.Id);
            return new GetOnePermissionOutput
                   {
                Permission = result
            };
        }

        [Route("CreatePermission")]
        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
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
      //  [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        public async Task<ResultUpdateOutput> UpdatePermission(UpdatePermissionInput input)
        {
            var result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await _permissionBll.UpdatePermission(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("DeletePermission")]
        public async Task<ResultUpdateOutput> DeletePermission(DeletePermissionInput input)
        {
            var result = new ResultUpdateOutput()
            {
                Status = 1
            };
            int test = await _permissionBll.DeletePermission((IEnumerable<int>)input.Ids);
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
        //[AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("GetPermissionGrants")]
        public async Task<GetPermissionGrantsOutput> GetPermissionGrants(GetByIdInput input)
        {
            GetPermissionGrantsOutput result = await _permissionBll.GetPermissionGrants(input.Id);
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Permissions)]
        [Route("UpdatePermissionGrants")]
        public async Task<ResultUpdateOutput> UpdatePermissionGrants(UpdatePermissionGrantsInput input)
        {
            var result = new ResultUpdateOutput()
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
       // [AppAuthorize(Roles = "140")]
        public async Task<IEnumerable<PermissionItemGrant>> GetUserPermissions(GetByIdInput input)
        {
            IEnumerable<PermissionItemGrant> result = await _permissionBll.GetUserPermissions(input.Id);
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
