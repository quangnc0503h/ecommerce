using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class GroupBll : IGroupBll
    {
        private IGroupTable _groupTable;
        private IUserTable _userTable;
        private IPermissionTable _permissionTable;
        private ITermTable _termTable;

        public MySQLDatabase Database { get; private set; }

        public GroupBll()
        {
            this.Database = (MySQLDatabase)OwinContextExtensions.Get<ApplicationDbContext>(HttpContextExtensions.GetOwinContext(HttpContext.Current.Request));
            this._groupTable = (IGroupTable)new GroupTable(this.Database);
            this._userTable = (IUserTable)new UserTable(this.Database);
            this._permissionTable = (IPermissionTable)new PermissionTable(this.Database);
            this._termTable = (ITermTable)new TermTable(this.Database);
        }

        public Task<Group> GetOneGroup(int groupId)
        {
            return Task.FromResult<Group>(this._groupTable.GetOneGroup(groupId));
        }

        public Task<DanhSachGroupOutput> GetAll(FilterGroupInput input)
        {
            int total = this._groupTable.GetTotal(input.ParentId, input.Keyword);
            IEnumerable<Group> paging = this._groupTable.GetPaging(input.PageSize, input.PageNumber, input.ParentId, input.Keyword);
            return Task.FromResult<DanhSachGroupOutput>(new DanhSachGroupOutput()
            {
                DanhSachGroups = paging,
                TotalCount = (long)total
            });
        }

        public async Task<DanhSachGroupOutput> GetAllWithTree(FilterGroupInput input)
        {
            IList<Group> groups = await this.GetListGroupOptions(input.ParentId);
            if (!string.IsNullOrEmpty(input.Keyword))
                groups = (IList<Group>)Enumerable.ToList<Group>(Enumerable.Where<Group>((IEnumerable<Group>)groups, (Func<Group, bool>)(m =>
                {
                    if (!string.IsNullOrEmpty(m.Name))
                        return m.Name.ToLower().Contains(input.Keyword.ToLower());
                    return false;
                })));
            int totalCount = groups.Count;
            groups = (IList<Group>)Enumerable.ToList<Group>(Enumerable.Take<Group>(Enumerable.Skip<Group>((IEnumerable<Group>)groups, input.PageNumber * input.PageSize), input.PageSize));
            DanhSachGroupOutput result = new DanhSachGroupOutput()
            {
                DanhSachGroups = (IEnumerable<Group>)groups,
                TotalCount = (long)totalCount
            };
            return result;
        }

        public Task<int> DeleteGroup(IEnumerable<int> Ids)
        {
            int result = this._groupTable.Delete(Ids);
            if (result > 0)
            {
                IPermissionBll permissionBll = UnityContainerExtensions.Resolve<IPermissionBll>((IUnityContainer)(UnityConfig.GetConfiguredContainer() as UnityContainer));
                foreach (int groupId in Ids)
                {
                    this._permissionTable.DeleteGroupPermissions(groupId);
                    foreach (User user in this._userTable.GetUsersByGroup(groupId))
                    {
                        this._userTable.removeUserFromGroup(groupId, user.Id);
                        permissionBll.GenerateRolesForUser(user.Id);
                    }
                    foreach (Term term in this._termTable.GetTermsByGroup(groupId))
                        this._termTable.removeTermFromGroup(groupId, term.Id);
                }
            }
            return Task.FromResult<int>(result);
        }

        public Task<int> InsertGroup(CreateGroupInput input)
        {
            Mapper.CreateMap<CreateGroupInput, Group>();
            return Task.FromResult<int>(this._groupTable.Insert(Mapper.Map<Group>((object)input)));
        }

        public Task<int> UpdateGroup(UpdateGroupInput input)
        {
            Mapper.CreateMap<UpdateGroupInput, Group>();
            return Task.FromResult<int>(this._groupTable.Update(Mapper.Map<Group>((object)input)));
        }

        public Task<IList<Group>> GetListGroupOptions()
        {
            return this.GetListGroupOptions(new int?());
        }

        public Task<IList<Group>> GetListGroupOptions(int? parentId)
        {
            IList<Group> list = (IList<Group>)new List<Group>();
            IDictionary<int, Group> allGroups = this._groupTable.GetAllGroups();
            this.GetListGroupOptionsInternal(parentId, list, allGroups, "", "---");
            return Task.FromResult<IList<Group>>(list);
        }

        private void GetListGroupOptionsInternal(int? parentId, IList<Group> items, IDictionary<int, Group> groups, string prefix, string separator)
        {
            foreach (KeyValuePair<int, Group> keyValuePair in (IEnumerable<KeyValuePair<int, Group>>)groups)
            {
                int? nullable = parentId;
                int? parentId1 = keyValuePair.Value.ParentId;
                if ((nullable.GetValueOrDefault() != parentId1.GetValueOrDefault() ? 0 : (nullable.HasValue == parentId1.HasValue ? 1 : 0)) != 0)
                {
                    if (!string.IsNullOrEmpty(prefix))
                        keyValuePair.Value.Name = prefix + " " + keyValuePair.Value.Name;
                    if (parentId.HasValue && groups[parentId.Value] != null)
                        keyValuePair.Value.ParentName = groups[parentId.Value].Name;
                    items.Add(keyValuePair.Value);
                    this.GetListGroupOptionsInternal(new int?(keyValuePair.Value.Id), items, groups, prefix + separator, separator);
                }
            }
        }
    }
}