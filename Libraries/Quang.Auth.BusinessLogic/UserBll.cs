using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quang.Auth.BusinessLogic
{
    public static class UserBll
    {
        public async static Task<IEnumerable<User>> GetAllUsers()
        {
            return await UserDal.GetAllUsers();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetUsersByGroup(long groupId)
        {
            return await UserDal.GetUsersByGroup(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetUsersByRole(long roleId)
        {
            return await UserDal.GetUsersByRole(roleId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<User> GetOneUser(long userId)
        {
            return await UserDal.GetOneUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(long? groupId, string keyword)
        {
            return await UserDal.GetTotal(groupId, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<User>> GetPaging(int pageSize, int pageNumber, long? groupId, string keyword)
        {
            return await UserDal.GetPaging(pageSize, pageNumber, groupId, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Group>> GetGroupsByUser(long userId)
        {
            return await UserDal.GetGroupsByUser(userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<long> AddUserToGroup(long groupId, long userId)
        {
            return await UserDal.AddUserToGroup(groupId, userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async static Task<long> RemoveUserFromGroup(long groupId, long userId)
        {
            return await UserDal.RemoveUserFromGroup(groupId, userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public async static Task<UserApp> GetUserApp(long userId, AppApiType appType)
        {
            return await UserDal.GetUserApp(userId, appType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApiKey"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public async static Task<UserApp> GetUserApp(string userApiKey, bool? isActive)
        {
            return await UserDal.GetUserApp(userApiKey, isActive);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApp"></param>
        /// <returns></returns>
        public async static Task<long> InsertUserApp(UserApp userApp)
        {
            return await UserDal.InsertUserApp(userApp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userApp"></param>
        /// <returns></returns>
        public async static Task<long> UpdateUserApp(UserApp userApp)
        {
            return await UserDal.UpdateUserApp(userApp);
        }

        public static async Task<IEnumerable<User>> GetUsersByGroup(params long[] groupIds)
        {
            IList<User> users = new List<User>();
            foreach (var groupId in groupIds)
            {
                var tmp = await GetUsersByGroup(groupId);
                foreach (var user in tmp)
                {
                    if (!users.Any(m => m.Id == user.Id))
                    {
                        users.Add(user);
                    }
                }
            }
            return (users);
        }
    }
}
