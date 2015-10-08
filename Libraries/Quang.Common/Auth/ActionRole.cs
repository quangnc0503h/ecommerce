using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Common.Auth
{
    public class ActionRole
    {
        public class HeThong
        {

            public const string Users = "100";
            public const string Groups = "110";
            public const string Permissions = "120";
            public const string Terms = "130";
            public const string Grant = "140";
            public const string UserApp = "150";

            public class User
            {
                public const string Edit = "101";
                public const string List = "102";
                public const string Add = "103";
                public const string Delete = "104";
            }
        }
        public class DanhMuc
        {
            public const string DoiTuongKH = "200";
            public const string PhuongThucTT = "210";
            public const string LyDoKhoaCho = "220";
            public const string Tuyen = "230";
            public const string Ga = "240";
            public const string NhomCho = "250";
            public const string LoaiCho = "260";
            public const string MacTau = "270";
            public const string ToaXe = "280";
        }

        public class KeHoach
        {

        }

        private static IDictionary<string, ActionRoleItem> _dictionaryItems;
        public static IDictionary<string, ActionRoleItem> ToListDictionary()
        {
            if (_dictionaryItems == null)
            {
                _dictionaryItems = new Dictionary<string, ActionRoleItem>();
                var assemblyTypes = typeof(ActionRole).Assembly.GetTypes();
                var items = assemblyTypes.Where(type => type.DeclaringType == typeof(ActionRole));
                foreach (var item in items)
                {
                    var properties = item.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(m => m.IsLiteral && !m.IsInitOnly).OrderBy(m => m.GetRawConstantValue());
                    foreach (var property in properties)
                    {
                        var roleKey = property.GetRawConstantValue();
                        if (roleKey != null)
                        {
                            var name = typeof(ActionRole).Name + "." + item.Name + "." + property.Name;
                            _dictionaryItems.Add(roleKey.ToString(), new ActionRoleItem { Group = item.Name, RoleKey = roleKey.ToString(), RoleKeyLabel = name });
                        }
                    }
                    var subItems = assemblyTypes.Where(type => type.DeclaringType == item);
                    foreach (var subItem in subItems)
                    {
                        var subProperties = subItem.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(m => m.IsLiteral && !m.IsInitOnly).OrderBy(m => m.GetRawConstantValue());
                        foreach (var subProperty in subProperties)
                        {
                            var roleKey = subProperty.GetRawConstantValue();
                            if (roleKey != null)
                            {
                                var name = typeof(ActionRole).Name + "." + item.Name + "." + subItem.Name + "." + subProperty.Name;
                                _dictionaryItems.Add(roleKey.ToString(), new ActionRoleItem { Group = item.Name, RoleKey = roleKey.ToString(), RoleKeyLabel = name });
                            }
                        }
                    }
                }
            }
            return _dictionaryItems;
        }
    }

    public class ActionRoleItem
    {
        public string Group { get; set; }
        public string RoleKey { get; set; }
        public string RoleKeyLabel { get; set; }
    }
}
