namespace ColorSys.HardwareContract
{
    public interface  ICommunication : IDisposable
    {
        string ConnectionId { get; }          // 端口/地址/序列号
        bool IsConnected { get; }
        Task ConnectAsync(CancellationToken token = default);
        Task DisconnectAsync(CancellationToken token = default);
        Task<byte[]> SendAsync(byte[] data, CancellationToken token = default);

        void Initialize();
    }
}
