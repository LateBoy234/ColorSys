using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class BluetoothComm : ICommunication
    {
        private readonly BlutoothParameter _p;
       
        public BluetoothComm(BlutoothParameter p) => _p = p;


        private readonly SemaphoreSlim _lock = new(1, 1);

       
        public bool IsConnected => throw new NotImplementedException();

        public bool SupportsPlugDetect => throw new NotImplementedException();

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public event EventHandler<byte[]> DataReceived;

        //public async Task ConnectAsync()
        //{
        //    _tcp = new TcpClient();
        //    await _tcp.ConnectAsync(_p.IP, _p.Port);
        //}
        //public bool IsConnected => _tcp?.Connected == true;
        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReconnectAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> SendAndReceiveAsync(byte[] request, int timeoutMs = 5000, CancellationToken token = default)
        {
            await _lock.WaitAsync(token);
            try
            {
                if (!IsConnected) throw new InvalidOperationException("蓝牙未连接");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(timeoutMs);

                throw new InvalidOperationException("功能未实现");
                // 发送
                //_writer.WriteBytes(request);
                //await _writer.StoreAsync().AsTask(cts.Token);

                //// 接收
                //var response = new List<byte>();
                //uint loaded;

                //do
                //{
                //    loaded = await _reader.LoadAsync(64).AsTask(cts.Token);
                //    if (loaded > 0)
                //    {
                //        var buffer = new byte[loaded];
                //        _reader.ReadBytes(buffer);
                //        response.AddRange(buffer);
                //    }

                //    // 简单判断：如果100ms内没有新数据，认为接收完成
                //    await Task.Delay(100, cts.Token);
                //} while (_reader.UnconsumedBufferLength > 0);

                //return response.ToArray();
            }
            finally
            {
                _lock.Release();
            }
        }

    }
}
