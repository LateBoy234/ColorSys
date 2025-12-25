using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Permission
{
    public partial class InMemoryAuthService : ObservableObject, IAuthService
    {
        [ObservableProperty]
        private User? _currentUser;

        public int ExpireMinutes { get; set; } = 30;

        private DateTime _loginTime;

        public bool IsExpired => CurrentUser != null &&
                                 DateTime.Now > _loginTime.AddMinutes(ExpireMinutes);

     

        public void Login(string user, string pwd)
        {
            // 仅演示
            CurrentUser = (user, pwd) switch
            {
                ("op", "1") => new User("Operator", UserRole.Operator),
                ("en", "1") => new User("Engineer", UserRole.Engineer),
                ("admin", "1") => new User("Admin", UserRole.Admin),
                _ => null
            };

            if (CurrentUser is null)
                throw new UnauthorizedAccessException("用户名或密码错误");

            _loginTime = DateTime.Now;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
