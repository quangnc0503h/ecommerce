using System.Collections.Generic;
using System.Threading.Tasks;
using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface IUserBll
    {
        Task<DanhSachUserOutput> GetAll(FilterUserInput input);

        Task<GetOneUserOutput> GetOneUser(string userId);

        Task<GetOneUserOutput> GetOneUser(string userId, bool getGroups);

        Task<CreateUserOutput> CreateUser(CreateUserInput input);

        Task<CreateMobileUserOutput> CreateMobileUser(CreateMobileUserInput input);

        Task<GetMobileProfileOutput> GetMobileProfile(int userId);

        Task<UpdateMobileProfileOutput> UpdateMobileProfile(int userId, UpdateMobileProfileInput input);

        Task<SetMobilePasswordOutput> SetMobilePassword(SetMobilePasswordInput input);

        Task<UpdateUserOutput> UpdateUser(UpdateUserInput input);

        Task<UpdateUserOutput> UpdateUser(UpdateUserInput input, bool updateGroups);

        Task<DeleteUserOutput> DeleteUser(DeleteUserInput input);

        Task<CheckUserExistOutput> CheckExistUserName(string userName, int id);

        Task<CheckUserExistOutput> CheckExistEmail(string email, int id);

        Task<UserApp> GetUserApp(int userId, AppApiType appType);

        Task<ResultUpdateOutput> UpdateUserApp(UpdateUserAppInput input, AppApiType appType);

        Task<UserApp> GetUserApp(string userApiKey);

        Task<UserApp> GetUserApp(string userApiKey, bool? isActive);

        Task<IEnumerable<User>> GetUsersByGroup(params int[] groupIds);
    }
}