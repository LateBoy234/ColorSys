using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.Model
{
    // 状态变更事件参数
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionState State { get; set; }
        public string Message { get; set; }
        public bool CanReconnect { get; set; }  // 是否支持自动重连
    }

    // 连接状态枚举
    public enum ConnectionState
    {
        Disconnected,   // 未连接
        Connecting,     // 连接中
        Connected,      // 已连接
        Lost,           // 连接丢失（需要重连）
        Reconnecting    // 自动重连中
    }
}
