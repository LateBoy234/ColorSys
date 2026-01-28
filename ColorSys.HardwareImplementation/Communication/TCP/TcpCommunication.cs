using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using Polly;
using Polly.Timeout;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly IAsyncPolicy _retryPolicy;
        private readonly IAsyncPolicy _timeoutPolicy;

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public bool SupportsPlugDetect => false;  // 不支持硬件插拔

        private DateTime _lastReceiveTime;
        private CancellationTokenSource _cts;
        private SemaphoreSlim _sendLock = new(1, 1);   // 写锁
        public bool IsConnected => _tcp?.Connected == true;

        public TcpCommunication(TcpParameters p)
        {
            _p = p;
            _retryPolicy = Policy
           .Handle<SocketException>()
           .WaitAndRetryAsync(3,
               retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
               onRetry: (exception, timeSpan, retryCount, context) =>
               {
                  // _logger.LogWarning($"Retry {retryCount} after {timeSpan}");
               });

            _timeoutPolicy = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);
        }

        public async Task ConnectAsync()
        {
            await _retryPolicy.ExecuteAsync(async ct =>
            {
                var tcp = new TcpClient();
                await tcp.ConnectAsync(_p.IP, _p.Port, ct);
                var ns = tcp.GetStream();

                // 旧连接如果有，先干净关掉
                _cts?.Cancel();
                _tcp?.Close();

                _tcp = tcp;
                _ns = ns;
                _cts = new CancellationTokenSource();

                // 启动读写两条任务
                _ = ReadLoop(_cts.Token);
                _ = HeartLoop(_cts.Token);
            }, CancellationToken.None);
        }

        // ① 读循环：阻塞 ReadAsync，按“长度前缀”拆帧
        private async Task ReadLoop(CancellationToken ct)
        {
            var lenBuf = new byte[4];          // 假设 4 字节 Big-Endian 长度
            while (!ct.IsCancellationRequested)
            {
                await ReadExactAsync(lenBuf, ct);                 // 必须读满 4 字节
                var payloadLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
                var payload = new byte[payloadLen];
                await ReadExactAsync(payload, ct);                // 必须读满 payload

                _lastReceiveTime = DateTime.Now;
                ProcessData(payload);                             // 抛给业务
            }
        }

       
        // ② 心跳循环：定时写，带锁
        private async Task HeartLoop(CancellationToken ct)
        {
            var pong = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x08 };
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(5_000, ct);
                var idle = DateTime.Now - _lastReceiveTime;
                if (idle > TimeSpan.FromSeconds(5))
                {
                    await _sendLock.WaitAsync(ct);
                    try { await _ns.WriteAsync(pong, ct); }
                    finally { _sendLock.Release(); }
                }
            }
        }

        // 工具：保证读满指定长度
        private async Task ReadExactAsync(byte[] buf, CancellationToken ct)
        {
            int read = 0;
            while (read < buf.Length)
            {
                int n = await _ns.ReadAsync(buf.AsMemory(read), ct);
                if (n == 0) throw new IOException("远端断开");
                read += n;
            }
        }

        private void ProcessData(byte[] data)
        {
            // 处理收到的数据...
            // _frameSubject.OnNext(data);
        }

      

        public void Dispose()
        {
            _tcp?.Dispose();
        }

        public async Task SendAsync(byte[] frame)
        {
            // protocol.send("MEA", "std", 10000);
            string s =  $"MEA std\n" ;
            // Trace.TraceInformation("SEND:{0}", s);
          
            var bytes = Encoding.UTF8.GetBytes(s);
            var packet = new byte[8 + bytes.Length];

            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(0, 4), 1);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(4, 4), (uint)bytes.Length);
            bytes.CopyTo(packet, 8);
            string ascii = Encoding.ASCII.GetString(packet, 0, packet.Length);
            Trace.TraceInformation("SEND:{0}", ascii);
            _ns.Write(packet, 0, packet.Length);   // 一次发完
          //  await _ns!.WriteAsync(packet, 0, packet.Length);
        }

        public async Task<bool> ReconnectAsync()
        {
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                State = ConnectionState.Reconnecting,
                Message = "正在尝试重连..."
            });

            for (int i = 0; i < 3; i++)
            {
                // 先干净停掉旧管道
                _cts?.Cancel();
                _tcp?.Close();

                // 再递归 ConnectAsync（重试策略会兜底）
                await ConnectAsync();
                if (IsConnected)
                {
                    StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                    {
                        State = ConnectionState.Connected,
                        Message = "重连成功"
                    });
                    return true;
                }
                await Task.Delay(1000);
            }

            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                State = ConnectionState.Disconnected,
                Message = "重连失败，请手动连接",
                CanReconnect = false
            });
            return false;
        }

    }
}
