using ColorSys.Permission.EventModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace ColorSys.Permission
{
    public partial class InMemoryAuthService : ObservableObject, IAuthService,IDisposable
    {

        private readonly Timer _timer;
        [ObservableProperty]
        private User? _currentUser;


        public InMemoryAuthService()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (_, _) => CheckExpiry();
            _timer.Start();
        }

        private void CheckExpiry()
        {
            if (CurrentUser is null) return;
            if (DateTime.Now > _loginTime.AddSeconds(ExpireMinutes))
            {
                Logout();
                MessengerEvent.SendSessionExpired();   // ← 抛事件
            }
        }
        public int ExpireMinutes { get; set; } = 30;

        private DateTime _loginTime;

        public bool IsExpired { get; set; }

     

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
            IsExpired = CurrentUser != null &&
                                 DateTime.Now > _loginTime.AddSeconds(ExpireMinutes);
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
