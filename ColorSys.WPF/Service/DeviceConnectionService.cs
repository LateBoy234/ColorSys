using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Service;
using ColorSys.HardwareContract.Strategy;
using ColorSys.WPF.ViewModels;
using ColorSys.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.WPF.Service
{
    public class DeviceConnectionService : IDeviceConnectionService
    {
       
        public IDevice CurrentDevice { get; private set; }

        public event EventHandler<DeviceConnectedEventArgs> DeviceChanged;

        private readonly IEnumerable<ICommStrategy> _commStrategies;
        private readonly IEnumerable<IDeviceStrategy> _deviceStrategies;

        public DeviceConnectionService(
            IEnumerable<ICommStrategy> commStrategies,
            IEnumerable<IDeviceStrategy> deviceStrategies)
        {
            _commStrategies = commStrategies;
            _deviceStrategies = deviceStrategies;
        }

        public async Task<IDevice> ConnectAsync()
        {
            // 创建连接窗口（UI 操作在这里，ViewModel 看不到）
            var vm = new ConnectViewModel(_commStrategies, _deviceStrategies);
            var window = new ConnectView { DataContext = vm };

            // 等待窗口关闭
            var result = window.ShowDialog();

            if (result == true && vm.DialogResult != null)
            {
                // 断开旧设备
                Disconnect();

                // 设置新设备
                CurrentDevice = vm.DialogResult;
                DeviceChanged?.Invoke(this, new DeviceConnectedEventArgs
                {
                    Device = CurrentDevice,
                    IsConnected = true
                });

                return CurrentDevice;
            }

            return null;
        }

        public void Disconnect()
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.Dispose();
                DeviceChanged?.Invoke(this, new DeviceConnectedEventArgs
                {
                    Device = CurrentDevice,
                    IsConnected = false
                });
                CurrentDevice = null;
            }
        }
    }
}
