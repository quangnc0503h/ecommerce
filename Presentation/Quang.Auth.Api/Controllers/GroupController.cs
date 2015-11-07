using Quang.Auth.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Auth.Entities;

using Quang.Common.Auth;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.BusinessLogic;

namespace Quang.Auth.Api.Controllers
{
    //[Authorize]
    [RoutePrefix("api/Group")]
    public class GroupController : ApiController
    {
        private IGroupBll _groupBll;
        private IUserBll _userBll;
        private ITermBll _termBll;
        private IPermissionBll _permissionBll;

        public GroupController()
        {
        }

        public GroupController(IGroupBll groupBll, ITermBll termBll, IUserBll userBll, IPermissionBll permissionBll)
        {
            this._groupBll = groupBll;
            this._userBll = userBll;
            this._termBll = termBll;
            this._permissionBll = permissionBll;
        }

       // [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("GetAll")]
        [HttpPost]
        public async Task<DanhSachGroupOutput> GetAll(FilterGroupInput filter)
        {
            return await this._groupBll.GetAllWithTree(filter);
        }

        [HttpPost]
       // [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("GetOneGroup")]
        public async Task<GetOneGroupOutput> GetOneGroup(GetOneGroupInput input)
        {
            Group result = await this._groupBll.GetOneGroup(input.Id);
            return new GetOneGroupOutput()
            {
                Group = result
            };
        }

        //[AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("CreateGroup")]
        [HttpPost]
        public async Task<CreateGroupOutput> CreateGroup(CreateGroupInput input)
        {
            CreateGroupOutput result = new CreateGroupOutput()
            {
                Status = 1
            };
            int test = await this._groupBll.InsertGroup(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("UpdateGroup")]
        public async Task<UpdateGroupOutput> UpdateGroup(UpdateGroupInput input)
        {
            UpdateGroupOutput result = new UpdateGroupOutput()
            {
                Status = 1
            };
            int test = await this._groupBll.UpdateGroup(input);
            if (test > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("DeleteGroup")]
        public async Task<DeleteGroupOutput> DeleteGroup(DeleteGroupInput input)
        {
            DeleteGroupOutput result = new DeleteGroupOutput()
            {
                Status = 1
            };
            IEnumerable<Quang.Auth.Entities.User> users = await this._userBll.GetUsersByGroup(input.Ids.ToArray());
            int test = await this._groupBll.DeleteGroup((IEnumerable<int>)input.Ids);
            if (test > 0)
            {
                foreach (Quang.Auth.Entities.User user in users)
                {
                    int num = await this._permissionBll.GenerateRolesForUser(user.Id);
                }
                result.Status = 0;
            }
            return result;
        }

        [HttpGet]
      //  [AppAuthorize]
        [Route("ListAllGroup")]
        public async Task<DanhSachGroupOutput> ListAllGroup()
        {
            IList<Group> groups = await this._groupBll.GetListGroupOptions();
            DanhSachGroupOutput result = new DanhSachGroupOutput()
            {
                DanhSachGroups = (IEnumerable<Group>)groups
            };
            return result;
        }
    }
}
