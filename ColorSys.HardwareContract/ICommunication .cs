namespace ColorSys.HardwareContract
{
    public interface  ICommunication :IDisposable
    {
         Task ConnectAsync();

         bool IsConnected { get; }

        // ① 主动发指令
        Task SendAsync(byte[] frame);

        // ② 实时侦听原始帧（热流，永不 Complete）
        IObservable<byte[]> FrameStream { get; }
    }
}
