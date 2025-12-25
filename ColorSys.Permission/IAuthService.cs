using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Permission
{
   /// <summary>
   /// 1.登录
   /// 2，当前已登陆的Token
   /// 3，判断当前用户是否有最小登录权限
   /// 4，注销
   /// </summary>
    public  interface IAuthService : INotifyPropertyChanged
    {
        User? CurrentUser { get; }
        int ExpireMinutes { get; set; }
        bool IsExpired { get; }

        void Login(string user, string pwd);
        void Logout();
    }
}
