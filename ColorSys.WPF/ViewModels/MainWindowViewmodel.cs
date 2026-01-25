using CColorSys.WPF.Interface;
using ColorSys.Domain.Config;
using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Service;
using ColorSys.HardwareImplementation.Communication.PLC;
using ColorSys.HardwareImplementation.SystemConfig;
using ColorSys.Permission;
using ColorSys.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ColorSys.WPF.ViewModels
{
    public partial  class MainWindowViewmodel:ObservableObject,IDisposable
    {
        private readonly IAuthService _auth;
        private readonly INavigationService _nav;

      
       
        private readonly IDeviceConnectionService _connectionService;

        public MainWindowViewmodel(IAuthService auth, INavigationService nav, IDeviceConnectionService connectionService)
        {
            // 关键：监听 Hub 的变化
            _connectionService = connectionService;
               _auth = auth;
            _nav = nav;
            //ColorDevice = device;
            Title = "ColorSystem";
            ConnectStatus= "连接";
            // 订阅设备变更事件
            _connectionService.DeviceChanged += OnDeviceChanged;

            // 订阅连接状态变化（连接过程中、自动断开/重连）
            _connectionService.ConnectionStatusChanged += OnConnectionStatusChanged;

            // 初始化命令状态
            UpdateCommandStates();
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

        #region 测试界面属性
        [ObservableProperty]
        private string _title;

        [RelayCommand(CanExecute = nameof(CanLogin))]
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
        [RelayCommand(CanExecute = nameof(CanLoginOut))]
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
        private void Massure()
        {
           // ColorDevice.RunTestAsync();
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
            AlarmMsg = obj;
        }

        private void RefreshPermissions()
        {
            var user = _auth.CurrentUser;
            var ok = user is not null && !_auth.IsExpired;

            CanA = ok;                           // 所有人都有 A
            CanB = ok && user!.Role >= UserRole.Engineer;
            CanC = ok && user!.Role == UserRole.Admin;
        }
        #endregion



        #region 设备通信区
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDeviceConnected))]
        [NotifyPropertyChangedFor(nameof(ConnectionStatus))]
        [NotifyPropertyChangedFor(nameof(DeviceTypeDisplay))]
        private IDevice _currentDevice;

        [ObservableProperty]
        private string _connectStatus;
        public bool IsDeviceConnected
        {
            get => CurrentDevice != null&&CurrentDevice.IsConnected;
        }

        public string ConnectionStatus => IsDeviceConnected
            ? $"已连接: {DeviceTypeDisplay}"
            : "未连接";

        public string DeviceTypeDisplay => CurrentDevice?.DeviceType.ToString() ?? "无";

        //[RelayCommand(CanExecute = nameof(IsDeviceConnected))]
        //private void Disconnect()
        //{
        //    _connectionService.Disconnect();
        //}

        [RelayCommand(CanExecute = nameof(IsDeviceConnected))]
        private async Task MeasureAsync()
        {
            if (CurrentDevice is IMeasureMent measurement)
            {
                var result = await measurement.RunTestAsync();
                // 处理测量结果...
            }
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                if (_currentDevice != null)
                {
                    _connectionService.Disconnect();
                    CurrentDevice = _connectionService.CurrentDevice;
                    // 显示连接成功消息
                    ConnectStatus = "连接";
                }
                else
                {
                    var device = await _connectionService.ConnectAsync();
                    if (device!=null&&device.IsConnected)
                    {
                        CurrentDevice = device;
                        // 显示连接成功消息
                        ConnectStatus = "断开";
                    }
                    else
                    {
                        // 显示连接失败消息
                        ConnectStatus = "连接";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void OnDeviceChanged(object sender, DeviceConnectedEventArgs e)
        {
            if (e.IsConnected)
            {
                CurrentDevice = e.Device;
                ConnectStatus = "断开";
            }
            else
            {
                CurrentDevice = null;
                ConnectStatus = "连接";
            }
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
           MeasureCommand.NotifyCanExecuteChanged();
        }
        #endregion

        public void Dispose()
        {
            _connectionService.DeviceChanged -= OnDeviceChanged;
            _connectionService.Disconnect();
        }

        // 连接过程中的状态（打开对话框时、自动断开/重连时）
        private void OnConnectionStatusChanged(object sender, ConnectionStateChangedEventArgs e)
        {
           // ConnectionState = e.State;

            // 可以在这里做更多 UI 反馈
            switch (e.State)
            {
                case ConnectionState.Connecting:
                    // 显示进度
                    break;
                case ConnectionState.Lost:
                    ConnectStatus = "连接";
                    CurrentDevice = null;
                   // UpdateCommandStates();
                   
                    _connectionService.Disconnect();
                    _connectionService.DeviceChanged -= OnDeviceChanged;
                    // 显示警告
                    break;
                case ConnectionState.Reconnecting:
                    // 显示重连中
                    break;
            }
        }


    }
}
