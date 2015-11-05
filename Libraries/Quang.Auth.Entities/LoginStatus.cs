using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quang.Auth.Entities
{
    public enum LoginStatus
    {
        BadRequest,
        Success,
        InvalidUserNameOrPassword,
        InvalidDeviceKey,
        InvalidApiClientToken,
        InvalidApiHeaderFormat,
        InvalidApiInfo,
        InvalidRefreshToken,
        ErrorAddRefreshToken,
        InvalidOldPassword,
        InvalidCientId,
        InvalidCientSecret,
        InvalidCientActive,
    }
}
