using ColorSys.HardwareContract;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class BluetoothComm : ICommunication
    {
        public bool IsConnected { get; private set; }

        public string ConnectionId => throw new NotImplementedException();

        public Task ConnectAsync(CancellationToken token = default) 
        { 
            /* 真实蓝牙 */ 
            IsConnected = true; 
            return Task.CompletedTask; 
        }
        public Task DisconnectAsync(CancellationToken token = default) 
        {
            IsConnected = false; 
            return Task.CompletedTask;
        }
        public Task<byte[]> SendAsync(byte[] data, CancellationToken token = default) => Task.FromResult(data);
        public void Dispose() { }

        public void Initialize()
        {
           
        }
    }
}
