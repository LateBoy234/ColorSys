using ColorSys.Domain.Model;

namespace ColorSys.HardwareContract
{
    public interface  ICommunication :IDisposable
    {
        Task ConnectAsync();

         bool IsConnected { get; }

        // ① 主动发指令
        Task SendAsync(byte[] frame);

        // ② 数据接收事件
        event EventHandler<byte[]> DataReceived;

     

        // 统一状态事件（所有实现都必须支持）
        event EventHandler<ConnectionStateChangedEventArgs> StateChanged;

        // 手动重连（适用于 TCP/蓝牙）
        Task<bool> ReconnectAsync();

        // 检查是否支持插拔检测
        bool SupportsPlugDetect { get; }
    }
}
