using CColorSys.WPF.Interface;
using ColorSys.HardwareImplementation.Communication.PLC;
using ColorSys.HardwareImplementation.SystemConfig;
using ColorSys.Permission;
using ColorSys.Domain.Config;
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
using ColorSys.Domain.Model;

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

        [ObservableProperty]
        private string _alarmMsg;
        #endregion

        [ObservableProperty]
        private string _title;

        [RelayCommand(CanExecute=nameof(CanLogin))]
        private void Login()
        {
            _nav.ShowLoginDialog();
            LoginCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
            LangUSCommand.NotifyCanExecuteChanged();    
            langCNCommand.NotifyCanExecuteChanged();
        }
        private bool CanLogin()
        {
            return _auth.CurrentUser is null || _auth.IsExpired;
        }
        [RelayCommand(CanExecute=nameof(CanLoginOut))]
        private void Logout()
        {
            _auth.Logout();
            LoginCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
        }

        private bool CanLoginOut()
        {
            return _auth.CurrentUser is not null && !_auth.IsExpired;
        }

        [RelayCommand]
        private void LangCN()
        {
             App.ChangeLanguage("zh-CN");
        }

        [RelayCommand]
        private void LangUS()
        {
            App.ChangeLanguage("en-US");
        }

        [RelayCommand]
        private void SysConfigSetting()
        {
            ConfigManager.Instance.SetEnum("ConnectionMethod",
                               ConnectionMethod.ByUSB);
        }

        [RelayCommand]
        private void A()
        {
            Simens s = new Simens("127.0.0.1");
            s.Alarm += S_Alarm;
        }

        [RelayCommand]
        private void B()
        {
            Simens s = new Simens("127.0.0.1");
            s.Alarm += S_Alarm;
        }

        private void S_Alarm(string obj)
        {
            AlarmMsg=obj;
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
