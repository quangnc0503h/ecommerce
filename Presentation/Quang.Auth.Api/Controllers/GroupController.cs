using Quang.Auth.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Quang.Auth.Entities;
using Quang.Auth.BusinessLogic;
using Quang.Common.Auth;

namespace Quang.Auth.Api.Controllers
{
    //[Authorize]
    [RoutePrefix("api/Group")]
    public class GroupController : BaseApiController
    {
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("GetAll")]
        public async Task<GetListGrouputputModel> GetAll(FilterGroupInputModel filter)
        {
            return await GetAllWithTree(filter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AllGroup")]
        public async Task<GetListGrouputputModel> AllGroup()
        {

            var groups = await GroupBll.GetListGroupOptions();
            var result = new GetListGrouputputModel { Groups = groups };
            return result;
        }
        [HttpPost]
        
        [Route("GetOneGroup")]
        public async Task<GetOneGroupOutputModel> GetOneGroup(GetByIdInputModel input)
        {
            var result = await GroupBll.GetOneGroup(input.Id);
            return new GetOneGroupOutputModel { Group = result };
        }
        [HttpPost]        
        [Route("CreateGroup")]
        public async Task<GroupOutputModel> CreateGroup(GroupInputModel input)
        {
            var result = new GroupOutputModel { Status = 1 };
            var group = new Group {
                Name = input.Name,
                Description = input.Description,
                ParentId = input.ParentId,
                Id = input.Id
            };
            var test = await GroupBll.Insert(group);
            if (test > 0)
            {
                result.Status = 0;
            }
            return result;
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("UpdateGroup")]
        public async Task<GroupOutputModel> UpdateGroup(GroupInputModel input)
        {
            var result = new GroupOutputModel { Status = 1 };
            var group = new Group
            {
                Name = input.Name,
                Description = input.Description,
                ParentId = input.ParentId,
                Id = input.Id
            };
            var test = await GroupBll.Update(group);
            if (test > 0)
            {
                result.Status = 0;
            }
            return result;
        }
        [HttpPost]
        [AppAuthorize(Roles = ActionRole.HeThong.Groups)]
        [Route("DeleteGroup")]
        public async Task<GroupOutputModel> DeleteGroup(DeleteGroupInputModel input)
        {
            var result = new GroupOutputModel { Status = 1 };
            var users = await UserBll.GetUsersByGroup(input.Ids.ToArray());
            var test = await GroupBll.Delete(input.Ids);
            if (test > 0)
            {
                foreach (var user in users)
                {
                    await PermissionBll.GenerateRolesForUser(user.Id);
                }
                result.Status = 0;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private  async Task<GetListGrouputputModel> GetAllWithTree(FilterGroupInputModel input)
        {
            IList<Group> groups = await GroupBll.GetListGroupOptions(input.ParentId);
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                groups = groups.Where(m => !string.IsNullOrEmpty(m.Name) && m.Name.ToLower().Contains(input.Keyword.ToLower())).ToList();
            }
            var totalCount = groups.Count;
            groups = groups.Skip(input.PageNumber * input.PageSize).Take(input.PageSize).ToList();
            var result = new GetListGrouputputModel { Groups = groups, TotalCount = totalCount };

            return result;
        }
    }
}
