using Quang.Auth.Api.BusinessLogic;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using Quang.Common.Auth;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quang.Auth.Api.Controllers
{
    [RoutePrefix("api/Term")]
    public class TermController : BaseApiController
    {
        private readonly ITermBll _termBll;

        public TermController()
        {
        }

        public TermController(ITermBll termBll)
        {
            _termBll = termBll;
        }

        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetAll")]
        [HttpPost]
        public async Task<DanhSachTermOutput> GetAll(FilterTermInput filter)
        {
            try
            {
                return await _termBll.GetAll(filter);
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DanhSachTermOutput();
        }

       // [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetGrantedUsersByTerm")]
        [HttpPost]
        public async Task<IEnumerable<User>> GetGrantedUsersByTerm(GetOneTermInput input)
        {
            try
            {
                return await _termBll.GetGrantedUsersByTerm(input.Id);
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return null;
        }

       // [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [HttpGet]
        [Route("GetMissingTerms")]
        public async Task<IEnumerable<ActionRoleItem>> GetMissingTerms()
        {
            try
            {
                return await _termBll.GetMissingTerms();
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return null;
        }

        [HttpPost]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [Route("GetOneTerm")]
        public async Task<GetOneTermOutput> GetOneTerm(GetOneTermInput input)
        {
            try
            {
                Term result = await _termBll.GetOneTerm(input.Id);
                return new GetOneTermOutput()
                {
                    Term = result
                };
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new GetOneTermOutput();
        }

       // [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        [HttpPost]
        [Route("CreateTerm")]
        public async Task<CreateTermOutput> CreateTerm(CreateTermInput input)
        {
            try
            {
                var result = new CreateTermOutput()
                {
                    Status = 1
                };
                int test = await _termBll.InsertTerm(input);
                if (test > 0)
                {
                    await _termBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new CreateTermOutput();
        }

        [Route("UpdateTerm")]
        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        public async Task<UpdateTermOutput> UpdateTerm(UpdateTermInput input)
        {
            try
            {
                var result = new UpdateTermOutput()
                {
                    Status = 1
                };
                int test = await _termBll.UpdateTerm(input);
                if (test > 0)
                {
                    await _termBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new UpdateTermOutput();
        }

        [Route("DeleteTerm")]
        [HttpPost]
      //  [AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        public async Task<DeleteTermOutput> DeleteTerm(DeleteTermInput input)
        {
            try
            {
                var result = new DeleteTermOutput
                {
                    Status = 1
                };
                int test = await _termBll.DeleteTerm(input.Ids);
                if (test > 0)
                {
                    await this._termBll.SynchTermsToRoles();
                    result.Status = 0;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new DeleteTermOutput();
        }

        [Route("ListRoleOptions")]
        [HttpGet]
        //[AppAuthorize(Roles = ActionRole.HeThong.Terms)]
        public ListRoleOptionsOutput ListRoleOptions()
        {
            try
            {
                IEnumerable<ActionRoleItem> listRoleOptions = this._termBll.GetListRoleOptions();
                return new ListRoleOptionsOutput()
                {
                    Options = listRoleOptions
                };
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new ListRoleOptionsOutput();
            
        }

        [Route("GetGrantTermsUser")]
        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        public async Task<IEnumerable<GrantUserTerm>> GetGrantTermsUser(GetOneUserInput input)
        {
            try
            {
                IEnumerable<GrantUserTerm> grantTermsUser = await _termBll.GetGrantTermsUser(input.Id);
                return grantTermsUser;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return null;
        }

       // [AppAuthorize(Roles = "140")]
        [Route("UpdateUserGrant")]
        [HttpPost]
        public async Task<UpdateTermOutput> UpdateUserGrant(UpdateUserGrantInput input)
        {
            try
            {
                int success = await _termBll.UpdateUserGrant(input);
                var result = new UpdateTermOutput
                {
                    Status = 1
                };
                if (success > 0)
                    result.Status = 0;
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new UpdateTermOutput();
        }

        [HttpPost]
       // [AppAuthorize(Roles = "140")]
        [Route("GetGrantTermsGroup")]
        public async Task<IEnumerable<GrantGroupTerm>> GetGrantTermsGroup(GetOneGroupInput input)
        {
            try
            {
                IEnumerable<GrantGroupTerm> grantTermsGroup = await _termBll.GetGrantTermsGroup(input.Id);
                return grantTermsGroup;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return null;
        }

        //[AppAuthorize(Roles = "140")]
        [Route("UpdateGroupGrant")]
        [HttpPost]
        public async Task<UpdateTermOutput> UpdateGroupGrant(UpdateGroupGrantInput input)
        {
            try
            {
                int success = await _termBll.UpdateGroupGrant(input);
                var result = new UpdateTermOutput()
                {
                    Status = 1
                };
                if (success > 0)
                    result.Status = 0;
                return result;
            }
            catch (Exception ex)
            {
                ErrorStore.LogExceptionWithoutContext(ex);
            }
            return new UpdateTermOutput();
        }
    }
}
