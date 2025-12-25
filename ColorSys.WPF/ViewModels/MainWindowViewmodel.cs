using CColorSys.WPF.Interface;
using ColorSys.Permission;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ColorSys.WPF.ViewModels
{
    public partial  class MainWindowViewmodel:ObservableObject
    {
        private readonly IAuthService _auth;
        private readonly INavigationService _nav;

        public MainWindowViewmodel(IAuthService auth, INavigationService nav)
        {
            _auth = auth;
            _nav = nav;
            Title = "ColorSystem";
            // 监听 CurrentUser/IsExpired 变化
            _auth.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(IAuthService.CurrentUser)
                                    or nameof(IAuthService.IsExpired))
                    RefreshPermissions();
            };
            RefreshPermissions();
        }

        #region 权限属性（源生成）
        [ObservableProperty]
        private bool _canA;

        [ObservableProperty]
        private bool _canB;

        [ObservableProperty]
        private bool _canC;
        #endregion

        [ObservableProperty]
        private string _title;

        [RelayCommand]
        private void Login()
        {
            _nav.ShowLoginDialog();
        }

        [RelayCommand]
        private void Logout()
        {
            _auth.Logout();
        }

        private void RefreshPermissions()
        {
            var user = _auth.CurrentUser;
            var ok = user is not null && !_auth.IsExpired;

            CanA = ok;                           // 所有人都有 A
            CanB = ok && user!.Role >= UserRole.Engineer;
            CanC = ok && user!.Role == UserRole.Admin;
        }
    }
}
