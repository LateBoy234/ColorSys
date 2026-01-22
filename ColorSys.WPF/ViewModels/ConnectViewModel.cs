using Autofac;
using Autofac.Core;
using ColorSys.Domain.Model;
using ColorSys.Domain.StaticService;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using ColorSys.HardwareImplementation.Device;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColorSys.WPF.ViewModels
{
    public partial class ConnectViewModel : ObservableObject
    {

        private readonly ICommStrategy[] _commStrategies;
        private readonly IDeviceStrategy[] _deviceStrategies;

        // 构造函数注入（Autofac 自动装配）
        public ConnectViewModel(
            IEnumerable<ICommStrategy> commStrategies,
            IEnumerable<IDeviceStrategy> deviceStrategies)
        {
            _commStrategies = commStrategies.ToArray();
            _deviceStrategies = deviceStrategies.ToArray();
        }

        public IReadOnlyList<ICommStrategy> CommStrategies => _commStrategies;
        public IReadOnlyList<IDeviceStrategy> DeviceStrategies => _deviceStrategies;

        // ========== 选中项（关键：选中策略时自动创建配置VM）==========

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentConfigViewModel))]  // 联动通知
        private ICommStrategy _selectedCommStrategy;

        partial void OnSelectedCommStrategyChanged(ICommStrategy value)
        {
            // 自动创建对应的配置VM
            CurrentConfigViewModel = value?.CreateConfigViewModel();
        }

        [ObservableProperty]
        private IDeviceStrategy _selectedDeviceStrategy;

        // ========== 动态配置VM（XAML ContentControl 绑这个）==========

        [ObservableProperty]
        private IConfigViewModel _currentConfigViewModel;

      

        // 用于传回主窗体的结果
        public IDevice DialogResult { get; private set; }

        [RelayCommand]
       
        public async Task ConnectAsync(Window win)
        {
            if (SelectedCommStrategy == null || SelectedDeviceStrategy == null || CurrentConfigViewModel == null)
            {
                MessageBox.Show("请选择仪器类型和连接方式");
                return;
            }

            try
            {
                // 1. 获取配置
                var config = CurrentConfigViewModel.GetConfig();

                // 2. 创建通讯
                var comm = SelectedCommStrategy.creatCommunication(config);

                // 3. 创建设备
                var device = SelectedDeviceStrategy.GetDevice(comm);

                // 4. 连接
                if (await device.ConnectAsync())
                {
                    // 成功，关闭窗口并返回设备
                    DialogResult = device;
                    win?.Close();
                }
                else
                {
                    MessageBox.Show("连接失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接错误：{ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel(Window win)
        {
            win.DialogResult = true;
            win.Close();
        }
    }

   
}
