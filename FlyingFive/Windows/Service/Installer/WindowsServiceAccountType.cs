using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Windows.Service.Installer
{

    /// <summary>
    /// win svc服务账户类型枚举
    /// </summary>
    public enum WindowsServiceAccountType
    {
        LocalService,
        NetworkService,
        LocalSystem,
        User
    }
}
