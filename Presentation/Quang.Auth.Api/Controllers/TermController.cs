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
    [RoutePrefix("api/Term")]
    public class TermController : ApiController
    {
        private ITermBll _termBll;

        public TermController()
        {
        }

        public TermController(ITermBll termBll)
        {
            this._termBll = termBll;
        }

      //  [AppAuthorize(Roles = "130")]
        [Route("GetAll")]
        [HttpPost]
        public async Task<DanhSachTermOutput> GetAll(FilterTermInput filter)
        {
            return await this._termBll.GetAll(filter);
        }

       // [AppAuthorize(Roles = "130")]
        [Route("GetGrantedUsersByTerm")]
        [HttpPost]
        public async Task<IEnumerable<Quang.Auth.Entities.User>> GetGrantedUsersByTerm(GetOneTermInput input)
        {
            return await this._termBll.GetGrantedUsersByTerm(input.Id);
        }

       // [AppAuthorize(Roles = "130")]
        [HttpGet]
        [Route("GetMissingTerms")]
        public async Task<IEnumerable<ActionRoleItem>> GetMissingTerms()
        {
            return await this._termBll.GetMissingTerms();
        }

        [HttpPost]
        //[AppAuthorize(Roles = "130")]
        [Route("GetOneTerm")]
        public async Task<GetOneTermOutput> GetOneTerm(GetOneTermInput input)
        {
            Term result = await this._termBll.GetOneTerm(input.Id);
            return new GetOneTermOutput()
            {
                Term = result
            };
        }

       // [AppAuthorize(Roles = "130")]
        [HttpPost]
        [Route("CreateTerm")]
        public async Task<CreateTermOutput> CreateTerm(CreateTermInput input)
        {
            CreateTermOutput result = new CreateTermOutput()
            {
                Status = 1
            };
            int test = await this._termBll.InsertTerm(input);
            if (test > 0)
            {
                await this._termBll.SynchTermsToRoles();
                result.Status = 0;
            }
            return result;
        }

        [Route("UpdateTerm")]
        [HttpPost]
      //  [AppAuthorize(Roles = "130")]
        public async Task<UpdateTermOutput> UpdateTerm(UpdateTermInput input)
        {
            UpdateTermOutput result = new UpdateTermOutput()
            {
                Status = 1
            };
            int test = await this._termBll.UpdateTerm(input);
            if (test > 0)
            {
                await this._termBll.SynchTermsToRoles();
                result.Status = 0;
            }
            return result;
        }

        [Route("DeleteTerm")]
        [HttpPost]
      //  [AppAuthorize(Roles = "130")]
        public async Task<DeleteTermOutput> DeleteTerm(DeleteTermInput input)
        {
            DeleteTermOutput result = new DeleteTermOutput()
            {
                Status = 1
            };
            int test = await this._termBll.DeleteTerm((IEnumerable<int>)input.Ids);
            if (test > 0)
            {
                await this._termBll.SynchTermsToRoles();
                result.Status = 0;
            }
            return result;
        }

        [Route("ListRoleOptions")]
        [HttpGet]
        //[AppAuthorize(Roles = "130")]
        public ListRoleOptionsOutput ListRoleOptions()
        {
            IEnumerable<ActionRoleItem> listRoleOptions = this._termBll.GetListRoleOptions();
            return new ListRoleOptionsOutput()
            {
                Options = listRoleOptions
            };
        }

        [Route("GetGrantTermsUser")]
        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        public async Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(GetOneUserInput input)
        {
            IEnumerable<GrantUserTerm> grantTermsUser = await this._termBll.GetGrantTermsUser(input.Id);
            return grantTermsUser;
        }

       // [AppAuthorize(Roles = "140")]
        [Route("UpdateUserGrant")]
        [HttpPost]
        public async Task<UpdateTermOutput> UpdateUserGrant(UpdateUserGrantInput input)
        {
            int success = await this._termBll.UpdateUserGrant(input);
            UpdateTermOutput result = new UpdateTermOutput()
            {
                Status = 1
            };
            if (success > 0)
                result.Status = 0;
            return result;
        }

        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        [Route("GetGrantTermsGroup")]
        public async Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(GetOneGroupInput input)
        {
            IEnumerable<GrantGroupTerm> grantTermsGroup = await this._termBll.GetGrantTermsGroup(input.Id);
            return grantTermsGroup;
        }

        //[AppAuthorize(Roles = "140")]
        [Route("UpdateGroupGrant")]
        [HttpPost]
        public async Task<UpdateTermOutput> UpdateGroupGrant(UpdateGroupGrantInput input)
        {
            int success = await this._termBll.UpdateGroupGrant(input);
            UpdateTermOutput result = new UpdateTermOutput()
            {
                Status = 1
            };
            if (success > 0)
                result.Status = 0;
            return result;
        }
    }
}
