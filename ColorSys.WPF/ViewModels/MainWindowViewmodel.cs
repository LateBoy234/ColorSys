using CColorSys.WPF.Interface;
using ColorSys.Domain.Config;
using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using ColorSys.HardwareContract.Service;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.PLC;
using ColorSys.HardwareImplementation.SystemConfig;
using ColorSys.Permission;
using ColorSys.WPF.Resources;
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
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ColorSys.WPF.ViewModels
{
    public partial  class MainWindowViewmodel:ObservableObject,IDisposable
    {
        private readonly IAuthService _auth;
        private readonly INavigationService _nav;

      
       
        private readonly IDeviceConnectionService _connectionService;
        private readonly IEnumerable<ICommStrategy> _commStrategies;
        private readonly IEnumerable<IDeviceStrategy> _deviceStrategies;

        public MainWindowViewmodel(
            IAuthService auth, 
            INavigationService nav, 
            IDeviceConnectionService connectionService,
            IEnumerable<ICommStrategy> commStrategies,
            IEnumerable<IDeviceStrategy> deviceStrategies)
        {
            // 关键：监听 Hub 的变化
            _connectionService = connectionService;
            _commStrategies = commStrategies;
            _deviceStrategies = deviceStrategies;

            _auth = auth;
            _nav = nav;

            Title = "ColorSystem";
            ConnectStatus= LanguageSwith.GetString("S_ConnectOn");
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
            InitiaColourDiagramContent();

            // 异步初始化配置并尝试自动连接
            _ = InitializeAsync();
        }

        private void InitiaColourDiagramContent()
        {
            ColourDiagramContent = new ColourDiagramView();
        }
        [ObservableProperty]
        private UserControl _colourDiagramContent;
        /// <summary>
        /// 初始化配置管理器并尝试自动连接
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                // 初始化配置管理器
                await ConfigManager.Instance.InitializeAsync();
                
                // 尝试自动连接
                await TryAutoConnectAsync();
            }
            catch (Exception ex)
            {
                // 记录错误但不影响程序启动
                System.Diagnostics.Debug.WriteLine($"自动连接失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 尝试使用上次保存的配置自动连接
        /// </summary>
        private async Task TryAutoConnectAsync()
        {
            var config = ConfigManager.Instance;
            
            // 获取上次的连接配置
            var lastDeviceType = config.Get<string>("LastDeviceType");
            var lastCommType = config.Get<string>("LastCommType");
            
            if (string.IsNullOrEmpty(lastDeviceType) || string.IsNullOrEmpty(lastCommType))
            {
                System.Diagnostics.Debug.WriteLine("没有找到上次的连接配置");
                return;
            }

            try
            {
                // 查找对应的策略
                var commStrategy = _commStrategies.FirstOrDefault(s => s.DisplayName == lastCommType);
                var deviceStrategy = _deviceStrategies.FirstOrDefault(s => s.DeviceType.ToString() == lastDeviceType);
                
                if (commStrategy == null || deviceStrategy == null)
                {
                    System.Diagnostics.Debug.WriteLine("找不到匹配的通讯或设备策略");
                    return;
                }

                // 根据通讯类型加载参数
                ICommParameters parameters = null;
                
                if (lastCommType.Contains("Serial") || lastCommType.Contains("串口"))
                {
                    parameters = new SerialParameters
                    {
                        PortName = config.Get<string>("Serial_PortName", ""),
                        BaudRate = config.Get<int>("Serial_BaudRate", 9600),
                        DataBits = config.Get<int>("Serial_DataBits", 8),
                        Parity = config.GetEnum<ParityBit>("Serial_Parity", ParityBit.None),
                        StopBits = config.GetEnum<StopBit>("Serial_StopBits", StopBit.One)
                    };
                }
                else if (lastCommType.Contains("TCP") || lastCommType.Contains("网络"))
                {
                    parameters = new TcpParameters
                    {
                        IP = config.Get<string>("Tcp_IP", "192.168.1.100"),
                        Port = config.Get<int>("Tcp_Port", 502)
                    };
                }
                
                if (parameters == null)
                {
                    System.Diagnostics.Debug.WriteLine("无法创建连接参数");
                    return;
                }

                // 创建通讯和设备
                var comm = commStrategy.CreatCommunication(parameters);
                var device = deviceStrategy.GetDevice(comm);
                
                // 尝试连接
                System.Diagnostics.Debug.WriteLine($"尝试自动连接到 {lastDeviceType} 通过 {lastCommType}...");
                
                if (await device.ConnectAsync())
                {
                    CurrentDevice = device;
                    ConnectionType = lastCommType; // 设置连接方式
                    ConnectStatus = LanguageSwith.GetString("S_ConnectOff");
                    System.Diagnostics.Debug.WriteLine("自动连接成功！");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("自动连接失败");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"自动连接异常: {ex.Message}");
            }
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

        /// <summary>
        /// 是否有设置权限（需要登录）
        /// </summary>
        [ObservableProperty]
        private bool _canAccessSettings;

        /// <summary>
        /// 是否有导入导出权限（需要登录）
        /// </summary>
        [ObservableProperty]
        private bool _canImportExport;
        #endregion

        #region 测试界面属性
        [ObservableProperty]
        private string _title;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private void Login()
        {
            _nav.ShowLoginDialog();
            LoginCommand.NotifyCanExecuteChanged();
            LogOutCommand.NotifyCanExecuteChanged();
            LangUSCommand.NotifyCanExecuteChanged();
            langCNCommand.NotifyCanExecuteChanged();
            RefreshPermissions(); // 刷新权限相关属性
        }
        private bool CanLogin()
        {
            return _auth.CurrentUser is null || _auth.IsExpired;
        }
        [RelayCommand(CanExecute = nameof(CanLoginOut))]
        private void LogOut()
        {
            _auth.Logout();
            LoginCommand.NotifyCanExecuteChanged();
            LogOutCommand.NotifyCanExecuteChanged();
            RefreshPermissions(); // 刷新权限相关属性
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

        [RelayCommand(CanExecute = nameof(IsDeviceConnected))]
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

            // 设置和导入导出权限 - 需要登录
            CanAccessSettings = ok;
            CanImportExport = ok;
        }
        #endregion



        #region 设备通信区
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDeviceConnected))]
        [NotifyPropertyChangedFor(nameof(ConnectionStatus))]
        [NotifyPropertyChangedFor(nameof(DeviceTypeDisplay))]
        [NotifyPropertyChangedFor(nameof(ConnectionTypy))]
        [NotifyPropertyChangedFor(nameof(ConnectionTypyIconKind))]
        private IDevice? _currentDevice;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectIconKind))]
        private string _connectStatus;

        /// <summary>
        /// 当前连接方式类型（Serial/TCP等）
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectionTypy))]
        [NotifyPropertyChangedFor(nameof(ConnectionTypyIconKind))]
        private string _connectionType = "";

        /// <summary>
        /// Returns the MaterialDesign icon kind based on connection status
        /// </summary>
        public string ConnectIconKind => _connectStatus == LanguageSwith.GetString("S_ConnectOff") ? "LinkVariant" : "LinkVariantOff";

        /// <summary>
        /// 根据连接方式返回对应的图标
        /// </summary>
        public string ConnectionTypyIconKind
        {
            get
            {
                if (!IsDeviceConnected) return "CloseCircleOutline";
                
                if (ConnectionType.Contains("Serial") || ConnectionType.Contains("串口"))
                    return "Usb";
                else if (ConnectionType.Contains("TCP") || ConnectionType.Contains("网络"))
                    return "Ethernet";
                else if (ConnectionType.Contains("Bluetooth") || ConnectionType.Contains("蓝牙"))
                    return "Bluetooth";
                else
                    return "Connection";
            }
        }

        /// <summary>
        /// 根据连接方式返回对应的文本
        /// </summary>
        public string ConnectionTypy
        {
            get
            {
                if (!IsDeviceConnected) return "未连接";
                
                if (ConnectionType.Contains("Serial") || ConnectionType.Contains("串口"))
                    return "串口";
                else if (ConnectionType.Contains("TCP") || ConnectionType.Contains("网络"))
                    return "网络";
                else if (ConnectionType.Contains("Bluetooth") || ConnectionType.Contains("蓝牙"))
                    return "蓝牙";
                else
                    return ConnectionType;
            }
        }

        public bool IsDeviceConnected
        {
            get => CurrentDevice != null&&CurrentDevice.IsConnected;
        }

        public string ConnectionStatus => IsDeviceConnected
            ? $"已连接: {DeviceTypeDisplay}"
            : "未连接";

        public string DeviceTypeDisplay => CurrentDevice?.DeviceType.ToString() ?? "无";

        #region Measurement Data Properties
        [ObservableProperty]
        private string _measurementResult = "Waiting for data...";
        
        [ObservableProperty]
        private string _lValue = "--";
        
        [ObservableProperty]
        private string _aValue = "--";
        
        [ObservableProperty]
        private string _bValue = "--";
        
        [ObservableProperty]
        private DateTime _lastMeasurementTime = DateTime.MinValue;
        #endregion


        [RelayCommand(CanExecute = nameof(IsDeviceConnected))]
        private async Task MeasureAsync()
        {
            if (CurrentDevice is IMeasureMent measurement)
            {
                var result = await measurement.RunTestAsync();
                
                // Also update the UI with the manual measurement result
                if (result.DataValues != null && result.DataValues.Length >= 3)
                {
                    LValue = result.DataValues[0].ToString("F2");
                    AValue = result.DataValues[1].ToString("F2");
                    BValue = result.DataValues[2].ToString("F2");
                }
                
                LastMeasurementTime = result.DateTime;
                MeasurementResult = $"Manual measurement at {result.DateTime:HH:mm:ss}";
            }
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            try
            {
                if (_currentDevice != null)
                {
                    await _connectionService.DisconnectAsync();
                    CurrentDevice = _connectionService.CurrentDevice;
                    // 显示连接成功消息
                    ConnectStatus = LanguageSwith.GetString("S_ConnectOn");
                }
                else
                {
                    var device = await _connectionService.ConnectAsync();
                    if (device!=null&&device.IsConnected)
                    {
                        CurrentDevice = device;
                        // 显示连接成功消息
                        ConnectStatus = LanguageSwith.GetString("S_ConnectOff");
                    }
                    else
                    {
                        // 显示连接失败消息
                        ConnectStatus = LanguageSwith.GetString("S_ConnectOn");
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
            // 先取消之前的订阅
            if (CurrentDevice is IMeasureMent prevMeasurementDevice)
            {
                prevMeasurementDevice.DataReceived -= OnDataReceivedFromDevice;
            }
            
            if (e.IsConnected)
            {
                CurrentDevice = e.Device;
                ConnectionType = e.ConnectionType; // 设置连接方式
                ConnectStatus = LanguageSwith.GetString("S_ConnectOff");
                
                // 订阅新的设备数据接收事件
                if (CurrentDevice is IMeasureMent newMeasurementDevice)
                {
                    newMeasurementDevice.DataReceived += OnDataReceivedFromDevice;
                }
            }
            else
            {
                CurrentDevice = null;
                ConnectionType = ""; // 清空连接方式
                ConnectStatus = LanguageSwith.GetString("S_ConnectOn");
            }
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            // 刷新测量相关命令（依赖设备连接状态）
            MeasureCommand.NotifyCanExecuteChanged();
            MassureCommand.NotifyCanExecuteChanged();
        }
        #endregion

        public void Dispose()
        {
            // Unsubscribe from device data received event
            if (CurrentDevice is IMeasureMent measurementDevice)
            {
                measurementDevice.DataReceived -= OnDataReceivedFromDevice;
            }
            
            _connectionService.DeviceChanged -= OnDeviceChanged;
            // Fire and forget for disposal - consider implementing IAsyncDisposable if needed
            _ = _connectionService.DisconnectAsync();
        }

        // 处理从设备接收到的数据
        private void OnDataReceivedFromDevice(object sender, TestModel testModel)
        {
            // 更新UI上的测量数据显示
            if (testModel.DataValues != null && testModel.DataValues.Length >= 3)
            {
                LValue = testModel.DataValues[0].ToString("F2");
                AValue = testModel.DataValues[1].ToString("F2");
                BValue = testModel.DataValues[2].ToString("F2");
            }
                    
            LastMeasurementTime = testModel.DateTime;
            MeasurementResult = $"Measurement received at {testModel.DateTime:HH:mm:ss}";
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
                    ConnectStatus = LanguageSwith.GetString("S_ConnectOn");
                    CurrentDevice = null;
                   // UpdateCommandStates();
                            
                    // 取消订阅数据接收事件
                    if (CurrentDevice is IMeasureMent measurementDevice)
                    {
                        measurementDevice.DataReceived -= OnDataReceivedFromDevice;
                    }
                            
                    _ = _connectionService.DisconnectAsync();
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
