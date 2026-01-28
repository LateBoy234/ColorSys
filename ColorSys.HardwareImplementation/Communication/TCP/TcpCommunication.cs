using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
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

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public bool SupportsPlugDetect => false;  // 不支持硬件插拔

        private DateTime _lastReceiveTime;
        private CancellationTokenSource _cts;
        public bool IsConnected => _tcp?.Connected == true;

        public TcpCommunication(TcpParameters p) => _p = p;

        public async Task ConnectAsync()
        {
            _cts= new CancellationTokenSource();
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_p.IP, _p.Port);
            _ns = _tcp.GetStream();
            // 单循环：接收 + 心跳检测
            _ = RunCommunicationLoop(_cts.Token); 
        }

        private async Task RunCommunicationLoop(CancellationToken ct)
        {
            var buffer = new byte[256];
            _lastReceiveTime = DateTime.Now;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // 非阻塞检测：是否有数据可读
                    if (_ns.DataAvailable)
                    {
                        // 有数据，读取
                        var n = await _ns.ReadAsync(buffer, 0, buffer.Length, ct);
                        if (n == 0) break; // 远端断开

                        _lastReceiveTime = DateTime.Now; // 更新最后接收时间
                        ProcessData(buffer[..n]);
                    }
                    else
                    {
                        // 无数据：检查是否需要发心跳
                        var idleTime = DateTime.Now - _lastReceiveTime;

                        if (idleTime > TimeSpan.FromSeconds(5))
                        {
                            // 空闲超 5 秒，发心跳
                           var alive= await SendHeartbeat();
                            if (!alive)
                            {
                                StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                                {
                                    State = ConnectionState.Lost,
                                    Message = "连接中断（心跳超时）",
                                    CanReconnect = true
                                });

                                // TCP 可以尝试自动重连
                                await AutoReconnect();
                            }
                            _lastReceiveTime = DateTime.Now; // 重置计时
                        }
                        else
                        {
                            // 短暂等待，避免 CPU 空转
                            await Task.Delay(100, ct);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { /* 正常取消 */ }
            catch (Exception) { /* 异常断开 */ }
        }

        /// <summary>
        /// 自动重连
        /// </summary>
        /// <returns></returns>
        private async Task AutoReconnect()
        {
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                State = ConnectionState.Reconnecting,
                Message = "正在尝试重连..."
            });

            for (int i = 0; i < 3; i++)
            {
                if (await SendHeartbeat())
                {
                    StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                    {
                        State = ConnectionState.Connected,
                        Message = "重连成功"
                    });
                    return;
                }
                await Task.Delay(1000);
            }

            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                State = ConnectionState.Disconnected,
                Message = "重连失败，请手动连接",
                CanReconnect = false
            });
        }
        private async Task<bool> SendHeartbeat()
        {
            try
            {
                //
                var heartbeat = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x08 };
                await _ns.WriteAsync(heartbeat);

                // 等待响应（最多等 3 秒）
                var responseBuffer = new byte[256];
                _tcp.ReceiveTimeout = 3000;
                var read = await _ns.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                if (read == 0)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
               // throw new Exception("心跳失败");
            }
        }
        private void ProcessData(byte[] data)
        {
            // 处理收到的数据...
            // _frameSubject.OnNext(data);
        }

      

        public void Dispose()
        {
            _ns?.Close();
            _tcp?.Close();
            _lock.Release();
        }

        private readonly SemaphoreSlim _lock = new(1, 1);
        public async Task<byte[]> SendAndReceiveAsync(byte[] request, int timeoutMs = 5000, CancellationToken token = default)
        {
            await _lock.WaitAsync(token);
            try
            {
                if (!IsConnected) throw new InvalidOperationException("ModbusTCP未连接");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

                var frame = BuildMbapFrame(request);
                await _ns.WriteAsync(frame, 0, frame.Length, cts.Token);
                await _ns.FlushAsync(cts.Token);
                cts.CancelAfter(timeoutMs);
                // 2. 精确读取 MBAP 头部（7字节）- 阻塞直到收到7字节或超时
                var mbap = await ReadExactAsync(7, cts.Token);
                var length = BinaryPrimitives.ReadUInt16BigEndian(mbap.AsSpan(4, 2));

                // 3. 根据长度读取 PDU 数据 - 阻塞直到收完或超时
                var pdu = await ReadExactAsync(length, cts.Token);

                return pdu; // 返回纯 PDU（不含 MBAP 头）
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 精确读取指定字节数 - 核心简化方法（类似事件的阻塞等待）
        /// </summary>
        private async Task<byte[]> ReadExactAsync(int count, CancellationToken token)
        {
            var buffer = new byte[count];
            var read = 0;

            while (read < count)
            {
                // ReadAsync 是异步阻塞的，有数据时立即返回，无数据时等待
                var n = await _ns.ReadAsync(buffer, read, count - read, token);
                if (n == 0) throw new IOException("连接已关闭");
                read += n;
            }

            return buffer;
        }

        private int _transactionId;
        private byte[] BuildMbapFrame(byte[] pdu)
        {
            var tid = (ushort)Interlocked.Increment(ref _transactionId);
           // var tid = Interlocked.Increment(ref _transactionId);
            var frame = new byte[7 + pdu.Length];

            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(0, 2), tid);  // Transaction ID
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(2, 2), 0);     // Protocol ID (0=Modbus)
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(4, 2), (ushort)pdu.Length); // Length
          //  frame[6] = _config.SlaveId;  // Unit ID

            Array.Copy(pdu, 0, frame, 7, pdu.Length);
            return frame;
        }
        public async Task<bool> ReconnectAsync()
        {
           await ConnectAsync();
            return IsConnected;
        }

    }
}
