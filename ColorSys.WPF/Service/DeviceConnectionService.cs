using ColorSys.Domain.Model;
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
        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStatusChanged;

        private readonly SynchronizationContext _syncContext;

        private readonly IEnumerable<ICommStrategy> _commStrategies;
        private readonly IEnumerable<IDeviceStrategy> _deviceStrategies;

        public DeviceConnectionService(
            IEnumerable<ICommStrategy> commStrategies,
            IEnumerable<IDeviceStrategy> deviceStrategies)
        {
            _commStrategies = commStrategies;
            _deviceStrategies = deviceStrategies;
            _syncContext = SynchronizationContext.Current; // 主线程创建
        }

        public async Task<IDevice> ConnectAsync()
        {
            // 创建连接窗口（UI 操作在这里，ViewModel 看不到）
            var vm = new ConnectViewModel(_commStrategies, _deviceStrategies);
            var window = new ConnectView { DataContext = vm };

           
            vm.StateChanged += (s, e) =>
            {
                _syncContext.Post(_ =>
                {
                    ConnectionStatusChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                    {
                        State = e.State,
                        Message = e.Message,
                        CanReconnect = e.CanReconnect
                    });
                }, null);
            };
            // 等待窗口关闭
            var result = window.ShowDialog();

            if (result == true && vm.DialogResult != null)
            {
                // 断开旧设备
               await DisconnectAsync();

                // 设置新设备
                CurrentDevice = vm.DialogResult;
                DeviceChanged?.Invoke(this, new DeviceConnectedEventArgs
                {
                    Device = CurrentDevice,
                    IsConnected = true,
                    ConnectionType = vm.SelectedConnectionType // 传递连接方式类型
                });

                return CurrentDevice;
            }

            return null;
        }

        public async Task DisconnectAsync()
        {
            if (CurrentDevice != null)
            {
                try
                {
                    if (CurrentDevice is IAsyncDisposable asyncDisposable)
                        await asyncDisposable.DisposeAsync();
                    else
                        CurrentDevice.Dispose();
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "Error disposing device");
                }
                finally
                {
                    var device = CurrentDevice;
                    CurrentDevice = null;

                    DeviceChanged?.Invoke(this, new DeviceConnectedEventArgs
                    {
                        Device = device,
                        IsConnected = false
                    });
                }
            }
        }

    }
}
