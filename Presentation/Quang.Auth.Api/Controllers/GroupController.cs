using System;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.BusinessLogic;
using StackExchange.Exceptional;
using Quang.Common.Auth;

namespace Quang.Auth.Api.Controllers
{
    //[Authorize]
    [RoutePrefix("api/Group")]
    public class GroupController : BaseApiController
    {
        private readonly IGroupBll _groupBll;
        private readonly IUserBll _userBll;
        private readonly IPermissionBll _permissionBll;

        public GroupController()
        {
        }

        public GroupController(IGroupBll groupBll, IUserBll userBll, IPermissionBll permissionBll)
        {
            _groupBll = groupBll;
            _userBll = userBll;
            _permissionBll = permissionBll;
        }

        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("GetAll")]
        [HttpPost]
        public async Task<DanhSachGroupOutput> GetAll(FilterGroupInput filter)
        {
            try
            {
                return await _groupBll.GetAllWithTree(filter);
            }
               catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DanhSachGroupOutput();
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("GetOneGroup")]
        public async Task<GetOneGroupOutput> GetOneGroup(GetOneGroupInput input)
        {
            try
            {
                var result = await _groupBll.GetOneGroup(input.Id);
                return new GetOneGroupOutput
                {
                    Group = result
                };
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new GetOneGroupOutput();
        }

        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("CreateGroup")]
        [HttpPost]
        public async Task<CreateGroupOutput> CreateGroup(CreateGroupInput input)
        {
            try
            {
                var result = new CreateGroupOutput()
                {
                    Status = 1
                };
                int test = await _groupBll.InsertGroup(input);
                if (test > 0)
                    result.Status = 0;
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new CreateGroupOutput();
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("UpdateGroup")]
        public async Task<UpdateGroupOutput> UpdateGroup(UpdateGroupInput input)
        {
            try
            {
                var result = new UpdateGroupOutput()
                {
                    Status = 1
                };
                int test = await _groupBll.UpdateGroup(input);
                if (test > 0)
                    result.Status = 0;
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new UpdateGroupOutput();
        }

        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("DeleteGroup")]
        public async Task<DeleteGroupOutput> DeleteGroup(DeleteGroupInput input)
        {
            try
            {
                var result = new DeleteGroupOutput()
                {
                    Status = 1
                };
                var users = await _userBll.GetUsersByGroup(input.Ids.ToArray());
                int test = await _groupBll.DeleteGroup(input.Ids);
                if (test > 0)
                {
                    foreach (var user in users)
                    {
                        await _permissionBll.GenerateRolesForUser(user.Id);
                    }
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DeleteGroupOutput();
        }

        [HttpGet]
        [AppAuthorize]
        [Route("ListAllGroup")]
        public async Task<DanhSachGroupOutput> ListAllGroup()
        {
            try
            {
                var groups = await _groupBll.GetListGroupOptions();
                var result = new DanhSachGroupOutput
                {
                    DanhSachGroups = groups
                };
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DanhSachGroupOutput();
        }
    }
}
