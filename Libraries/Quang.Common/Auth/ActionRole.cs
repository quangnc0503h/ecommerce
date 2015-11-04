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

            public const string Users = "100";// Quan ly thanh vien
            public const string Groups = "110";//Quan ly group
            public const string Permissions = "120";// Quan ly role
            public const string Terms = "130";// Quan ly quyen
            public const string Grant = "140";// Thiet lap ung dung
            public const string UserApp = "150";// Quan ly ung dung
            public const string Devices = "160";// Quan ly thiet bi

            public class User
            {
                public const string Edit = "101";
                public const string List = "102";
                public const string Add = "103";
                public const string Delete = "104";
            }
        }
        public class HelpView
        {
            public const string HeThong = "001";
            public const string DanhMuc = "002";
            public const string SoatVe = "003";
            public const string PublicApi = "004";
            public const string KeHoach = "007";
            public const string Report = "008";
            public const string HangHoa = "00A";
        }
        public class DanhMuc
        {
            public const string Languages = "200";
            public const string Country = "210";
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
        public class ValidateTicket
        {
            public const string SoatVe = "300";
        }
        public class Report
        {
            public const string BCNhanh = "801";
            public const string BCDoanhThuBanHang = "802";
            public const string BCDoanhThuThucHien = "803";
        }
        private static IDictionary<string, ActionRoleItem> _dictionaryItems;
        public static IDictionary<string, ActionRoleItem> ToListDictionary()
        {
            if (ActionRole._dictionaryItems == null)
            {
                ActionRole._dictionaryItems = (IDictionary<string, ActionRoleItem>)new Dictionary<string, ActionRoleItem>();
                Type[] types = typeof(ActionRole).Assembly.GetTypes();
                foreach (Type type1 in Enumerable.Where<Type>((IEnumerable<Type>)types, (Func<Type, bool>)(type => type.DeclaringType == typeof(ActionRole))))
                {
                    Type item = type1;
                    foreach (FieldInfo fieldInfo in (IEnumerable<FieldInfo>)Enumerable.OrderBy<FieldInfo, object>(Enumerable.Where<FieldInfo>((IEnumerable<FieldInfo>)item.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy), (Func<FieldInfo, bool>)(m =>
                    {
                        if (m.IsLiteral)
                            return !m.IsInitOnly;
                        return false;
                    })), (Func<FieldInfo, object>)(m => m.GetRawConstantValue())))
                    {
                        object rawConstantValue = fieldInfo.GetRawConstantValue();
                        if (rawConstantValue != null)
                        {
                            string str = typeof(ActionRole).Name + "." + item.Name + "." + fieldInfo.Name;
                            ActionRole._dictionaryItems.Add(rawConstantValue.ToString(), new ActionRoleItem()
                            {
                                Group = item.Name,
                                RoleKey = rawConstantValue.ToString(),
                                RoleKeyLabel = str
                            });
                        }
                    }
                    foreach (Type type2 in Enumerable.Where<Type>((IEnumerable<Type>)types, (Func<Type, bool>)(type => type.DeclaringType == item)))
                    {
                        foreach (FieldInfo fieldInfo in (IEnumerable<FieldInfo>)Enumerable.OrderBy<FieldInfo, object>(Enumerable.Where<FieldInfo>((IEnumerable<FieldInfo>)type2.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy), (Func<FieldInfo, bool>)(m =>
                        {
                            if (m.IsLiteral)
                                return !m.IsInitOnly;
                            return false;
                        })), (Func<FieldInfo, object>)(m => m.GetRawConstantValue())))
                        {
                            object rawConstantValue = fieldInfo.GetRawConstantValue();
                            if (rawConstantValue != null)
                            {
                                string str = typeof(ActionRole).Name + "." + item.Name + "." + type2.Name + "." + fieldInfo.Name;
                                ActionRole._dictionaryItems.Add(rawConstantValue.ToString(), new ActionRoleItem()
                                {
                                    Group = item.Name,
                                    RoleKey = rawConstantValue.ToString(),
                                    RoleKeyLabel = str
                                });
                            }
                        }
                    }
                }
            }
            return ActionRole._dictionaryItems;
        }
    }

    public class ActionRoleItem
    {
        public string Group { get; set; }
        public string RoleKey { get; set; }
        public string RoleKeyLabel { get; set; }
    }
}
