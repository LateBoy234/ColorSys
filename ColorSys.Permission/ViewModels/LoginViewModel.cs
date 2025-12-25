using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ColorSys.Permission.ViewModels
{
    public partial class LoginViewModel: ObservableObject
    {
        private readonly IAuthService _auth;

        public LoginViewModel(IAuthService auth)
        {
            _auth = auth;
        }

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _expireMinutes = "30";

        [RelayCommand]
        private void Login(Window win)
        {
            try
            {
                if (int.TryParse(ExpireMinutes, out var min))
                    _auth.ExpireMinutes = min;

                _auth.Login(UserName, Password);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "登录失败");
            }
        }
    }
    
}
