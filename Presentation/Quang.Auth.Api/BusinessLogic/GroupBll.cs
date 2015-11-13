using AspNet.Identity.MySQL;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Practices.Unity;
using Quang.Auth.Api.DataAccess;
using Quang.Auth.Api.Dto;
using Quang.Auth.Api.Models;
using Quang.Auth.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Quang.Auth.Api.BusinessLogic
{
    public class GroupBll : IGroupBll
    {
        private readonly IGroupTable _groupTable;
        private readonly IUserTable _userTable;
        private readonly IPermissionTable _permissionTable;
        private readonly ITermTable _termTable;

        public MySQLDatabase Database { get; private set; }

        public GroupBll()
        {
            Database = HttpContext.Current.Request.GetOwinContext().Get<ApplicationDbContext>();
            _groupTable = new GroupTable(Database);
            _userTable = new UserTable(Database);
            _permissionTable = new PermissionTable(Database);
            _termTable = new TermTable(Database);
        }

        public Task<Group> GetOneGroup(int groupId)
        {
            return Task.FromResult(_groupTable.GetOneGroup(groupId));
        }

        public Task<DanhSachGroupOutput> GetAll(FilterGroupInput input)
        {
            int total = _groupTable.GetTotal(input.ParentId, input.Keyword);
            IEnumerable<Group> paging = _groupTable.GetPaging(input.PageSize, input.PageNumber, input.ParentId, input.Keyword);
            return Task.FromResult(new DanhSachGroupOutput
                                   {
                DanhSachGroups = paging,
                TotalCount = total
            });
        }

        public async Task<DanhSachGroupOutput> GetAllWithTree(FilterGroupInput input)
        {
            IList<Group> groups = await GetListGroupOptions(input.ParentId);
            if (!string.IsNullOrEmpty(input.Keyword))
                groups = groups.Where(m =>
                                      {
                                          if (!string.IsNullOrEmpty(m.Name))
                                              return m.Name.ToLower().Contains(input.Keyword.ToLower());
                                          return false;
                                      }).ToList();
            int totalCount = groups.Count;
            groups = groups.Skip(input.PageNumber * input.PageSize).Take(input.PageSize).ToList();
            var result = new DanhSachGroupOutput()
            {
                DanhSachGroups = groups,
                TotalCount = totalCount
            };
            return result;
        }

        public Task<int> DeleteGroup(IEnumerable<int> Ids)
        {
            int result = _groupTable.Delete(Ids);
            if (result > 0)
            {
                var permissionBll = (UnityConfig.GetConfiguredContainer() as UnityContainer).Resolve<IPermissionBll>();
                foreach (int groupId in Ids)
                {
                    _permissionTable.DeleteGroupPermissions(groupId);
                    foreach (User user in _userTable.GetUsersByGroup(groupId))
                    {
                        _userTable.removeUserFromGroup(groupId, user.Id);
                        permissionBll.GenerateRolesForUser(user.Id);
                    }
                    foreach (Term term in _termTable.GetTermsByGroup(groupId))
                        _termTable.removeTermFromGroup(groupId, term.Id);
                }
            }
            return Task.FromResult(result);
        }

        public Task<int> InsertGroup(CreateGroupInput input)
        {
            Mapper.CreateMap<CreateGroupInput, Group>();
            return Task.FromResult(_groupTable.Insert(Mapper.Map<Group>(input)));
        }

        public Task<int> UpdateGroup(UpdateGroupInput input)
        {
            Mapper.CreateMap<UpdateGroupInput, Group>();
            return Task.FromResult<int>(this._groupTable.Update(Mapper.Map<Group>((object)input)));
        }

        public Task<IList<Group>> GetListGroupOptions()
        {
            return GetListGroupOptions(new int?());
        }

        public Task<IList<Group>> GetListGroupOptions(int? parentId)
        {
            IList<Group> list = new List<Group>();
            IDictionary<int, Group> allGroups = _groupTable.GetAllGroups();
            GetListGroupOptionsInternal(parentId, list, allGroups, "", "---");
            return Task.FromResult(list);
        }

        private void GetListGroupOptionsInternal(int? parentId, IList<Group> items, IDictionary<int, Group> groups, string prefix, string separator)
        {
            foreach (KeyValuePair<int, Group> keyValuePair in groups)
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
                    GetListGroupOptionsInternal(keyValuePair.Value.Id, items, groups, prefix + separator, separator);
                }
            }
        }
    }
}