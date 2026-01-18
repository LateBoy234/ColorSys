using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.TCP
{
    public  class TcpCommunication : ICommunication
    {
        private readonly TcpParameters _p;
        private TcpClient? _tcp;
        private NetworkStream? _ns;
        private readonly Subject<byte[]> _frameSubject = new();
        public IObservable<byte[]> FrameStream => _frameSubject;
        public TcpCommunication(TcpParameters p) => _p = p;

        public async Task ConnectAsync()
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_p.IP, _p.Port);
            _ns = _tcp.GetStream();
            _ = Task.Run(ReceiveLoop); // 后台死循环收
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[256];
            while (true)
            {
                var n = await _ns!.ReadAsync(buffer, 0, buffer.Length);
                if (n == 0) break; // 远端断开
                var slice = buffer[..n];
                //if (TryExtractFrame(slice, out var frame))
                //    _frameSubject.OnNext(frame);
            }
        }

        public void Dispose()
        {
            _tcp?.Dispose();
            _frameSubject.OnCompleted();
            _frameSubject.Dispose();
        }

        public async Task SendAsync(byte[] frame) => await _ns!.WriteAsync(frame);

        public bool IsConnected => _tcp?.Connected == true;

    }
}
