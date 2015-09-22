using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quang.Auth.DataAccess;
using Quang.Auth.Entities;
namespace Quang.Auth.BusinessLogic
{
    public static class GroupBll
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<long> Delete(long groupId)
        {
            return await GroupDal.Delete(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public async static Task<long> Delete(IEnumerable<long> Ids)
        {
            return await GroupDal.Delete(Ids);
        }
        public async static Task<long> DeleteGroup(IEnumerable<long> Ids)
        {
            var result =  await Delete(Ids);
            if (result > 0)
            {
                

                foreach (var groupId in Ids)
                {
                    // Remove all permission from group
                    await PermissionBll.DeleteGroupPermissions(groupId);

                    // Remove all user from group
                    var users = await UserBll.GetUsersByGroup(groupId);
                    foreach (var user in users)
                    {
                        await UserBll.RemoveUserFromGroup(groupId, user.Id);
                        await PermissionBll.GenerateRolesForUser(user.Id);
                    }

                    // Remove all terms from group
                    var terms = await TermBll.GetTermsByGroup(groupId);
                    foreach (var term in terms)
                    {
                        await TermBll.RemoveTermFromGroup(groupId, term.Id);
                    }
                }
            }
            return (result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async static Task<long> Insert(Group group)
        {
            return await GroupDal.Insert(group);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async static Task<long> Update(Group group)
        {
            return await GroupDal.Update(group);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<IDictionary<long, Group>> GetAllGroups()
        {
            return await GroupDal.GetAllGroups();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async static Task<Group> GetOneGroup(long groupId)
        {
            return await GroupDal.GetOneGroup(groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<long> GetTotal(long? parentId, string keyword)
        {
            return await GroupDal.GetTotal(parentId, keyword);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="parentId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async static Task<IEnumerable<Group>> GetPaging(int pageSize, int pageNumber, long? parentId, string keyword)
        {
            return await GroupDal.GetPaging(pageSize, pageNumber, parentId, keyword);
        }
        public static  Task<IList<Group>> GetListGroupOptions()
        {
            return GetListGroupOptions(null);
        }

        public static async Task<IList<Group>> GetListGroupOptions(long? parentId)
        {
            IList<Group> items = new List<Group>();
            var groups =  await GroupDal.GetAllGroups();
            GetListGroupOptionsInternal(parentId, items, groups, "", "---");

            return (items);
        }
        private static void GetListGroupOptionsInternal(long? parentId, IList<Group> items, IDictionary<long, Group> groups, string prefix, string separator)
        {
            foreach (var group in groups)
            {
                if (parentId == group.Value.ParentId)
                {
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        group.Value.Name = prefix + " " + group.Value.Name;
                    }
                    if (parentId.HasValue && groups[parentId.Value] != null)
                    {
                        group.Value.ParentName = groups[parentId.Value].Name;
                    }
                    items.Add(group.Value);
                    GetListGroupOptionsInternal(group.Value.Id, items, groups, prefix + separator, separator);
                }
            }
        }
    }
}
