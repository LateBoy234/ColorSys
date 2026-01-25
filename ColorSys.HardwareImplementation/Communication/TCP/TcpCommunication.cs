using ColorSys.Domain.Model;
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
            _tcp?.Dispose();
            //_frameSubject.OnCompleted();
            //_frameSubject.Dispose();
        }

        public async Task SendAsync(byte[] frame) => await _ns!.WriteAsync(frame);

        public async Task<bool> ReconnectAsync()
        {
           await ConnectAsync();
            return IsConnected;
        }

    }
}
