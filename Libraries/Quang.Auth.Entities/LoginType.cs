using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public enum LoginType
    {
        LoginForm = 1,
        LoginDevice = 2,
        LoginApiKey = 3,
        RefreshToken = 4,
        ChangePassword = 5,
    }
}
