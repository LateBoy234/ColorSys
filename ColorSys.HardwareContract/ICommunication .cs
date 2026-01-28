using ColorSys.Domain.Model;

namespace ColorSys.HardwareContract
{
    public interface  ICommunication :IDisposable
    {
         Task ConnectAsync();

         bool IsConnected { get; }

        /// <summary>
        /// 发送数据并等待响应
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <param name="token">取消令牌</param>
        /// <returns>响应数据</returns>
        Task<byte[]> SendAndReceiveAsync(byte[] request, int timeoutMs = 5000, CancellationToken token = default);

        // 统一状态事件（所有实现都必须支持）
        event EventHandler<ConnectionStateChangedEventArgs> StateChanged;

        // 手动重连（适用于 TCP/蓝牙）
        Task<bool> ReconnectAsync();

        // 检查是否支持插拔检测
        bool SupportsPlugDetect { get; }
    }
}
