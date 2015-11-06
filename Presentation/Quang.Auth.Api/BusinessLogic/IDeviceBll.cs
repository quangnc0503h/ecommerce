using Quang.Auth.Api.Dto;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public interface IDeviceBll
    {
        Task<Device> GetOneDevice(int deviceId);

        Task<Device> GetOneDeviceByKey(string clientId, string deviceKey);

        Task<DanhSachDeviceOutput> GetAll(FilterDeviceInput input);

        Task<DanhSachRequestDeviceOutput> GetAllRequest(FilterRequestDeviceInput input);

        Task<IEnumerable<Device>> GetAllDevices();

        Task<int> DeleteDevice(int deviceId);

        Task<int> DeleteDevice(IEnumerable<int> Ids);

        Task<int> DeleteRequestDevice(int Id);

        Task<int> DeleteRequestDevice(IEnumerable<int> Ids);

        Task<int> InsertDevice(CreateDeviceInput input);

        Task<int> UpdateDevice(UpdateDeviceInput input);

        Task<Device> GetDevice(string clientId, string deviceKey, string deviceSecret);

        Task<InformNewAppOutput> InformNewApp(InformNewAppInput newApp);

        Task<IsExistDeviceIOutput> IsExistDevice(string clientId, string deviceKey, int id);

        Task<CheckDeviceOutput> CheckDevice(CheckDeviceInput input);

        Task<IEnumerable<Client>> GetAllClients();
    }
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