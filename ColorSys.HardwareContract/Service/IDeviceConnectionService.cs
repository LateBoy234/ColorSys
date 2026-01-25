using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.Service
{
    public  interface IDeviceConnectionService
    {
        /// <summary>
        /// 弹出连接对话框，返回连接成功的设备
        /// </summary>
        /// <returns>连接成功返回设备，取消或失败返回 null</returns>
        Task<IDevice> ConnectAsync();

        /// <summary>
        /// 当前已连接的设备
        /// </summary>
        IDevice CurrentDevice { get; }

        /// <summary>
        /// 断开当前设备
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 设备连接状态变更事件
        /// </summary>
        event EventHandler<DeviceConnectedEventArgs> DeviceChanged;

        // 新增：连接过程中的状态事件
        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStatusChanged;
    }

    public class DeviceConnectedEventArgs : EventArgs
    {
        public IDevice Device { get; set; }
        public bool IsConnected { get; set; }
    }
}
