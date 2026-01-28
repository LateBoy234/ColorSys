using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class ModbusRtuSerial : ICommunication
    {
        private readonly SerialParameters _p;

        private readonly List<byte> _receiveBuffer = new();
        private TaskCompletionSource<byte[]> _receiveTcs;
        private readonly object _dataLock = new();
        private CancellationTokenSource _receiveCts;

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public bool SupportsPlugDetect => true;  // 支持 WMI 检测

        private ManagementEventWatcher _plugWatcher;
        private ManagementEventWatcher _unplugWatcher;
        public ModbusRtuSerial(SerialParameters p)
        {
            _p = p;
            // 定时检测端口是否存在

        }

        private SerialPort? _port;

        private readonly Subject<byte[]> _frameSubject = new();

        public async Task ConnectAsync()
        {
            try
            {
                StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                {
                    State = ConnectionState.Connecting,
                    Message = "正在打开串口..."
                });
                _port = new SerialPort(_p.PortName, _p.BaudRate, _p.Parity.ToSystemParity(), _p.DataBits, _p.StopBits.ToSystemStopBits());

                await Task.Run(() => _port.Open());
                _port.DataReceived += OnData;

                // 2. 启动 WMI 监听（关键！）
                StartDeviceWatcher(_p.PortName);

                StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                {
                    State = ConnectionState.Connected,
                    Message = $"串口 {_p.PortName} 已连接"
                });
            }
            catch (Exception ex)
            {
              //  _logger.LogError(ex, "Unexpected heartbeat failure");
                throw;
            }

        }


        // ========== WMI 监听 USB 插拔 ==========

        private void StartDeviceWatcher(string targetPort)
        {
            // WMI 查询要用 DeviceID，格式是 "COM3" 不是 "COM3:"
            var query = new WqlEventQuery(
                "SELECT * FROM __InstanceDeletionEvent " +
                "WITHIN 1 " +
                "WHERE TargetInstance ISA 'Win32_SerialPort' " +
                $"AND TargetInstance.DeviceID = '{targetPort}'");  // 精确匹配

            _unplugWatcher = new ManagementEventWatcher(query);
            _unplugWatcher.EventArrived += (s, e) =>
            {
                var port = GetPortNameFromWmi(e.NewEvent);
                if (port == targetPort)
                {
                    // 触发状态变更
                    StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
                    {
                        State = ConnectionState.Lost,
                        Message = "串口设备已拔出",
                        CanReconnect = true  // 可以自动重连
                    });

                    CleanupPort();
                    StartReconnectWatcher();
                }
            };
            _unplugWatcher.Start();
        }


        private readonly SemaphoreSlim _reconnectLock = new(1, 1);
        /// <summary>
        /// 启动自动重连监听
        /// </summary>
        private void StartReconnectWatcher()
        {
            Debug.WriteLine("[WMI] 开始监听设备插入...");

            try
            {
                var plugQuery = new WqlEventQuery(
                    "SELECT * FROM __InstanceCreationEvent " +
                    "WITHIN 1 " +
                    "WHERE TargetInstance ISA 'Win32_SerialPort'");

                _plugWatcher = new ManagementEventWatcher(plugQuery);
                _plugWatcher.EventArrived += (s, e) =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _reconnectLock.WaitAsync();
                            var port = GetPortNameFromWmi(e.NewEvent);
                            if (port == _p.PortName)
                            {
                                await ReconnectAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                           // _logger.LogError(ex, "Reconnect failed");
                        }
                        finally
                        {
                            _reconnectLock.Release();
                        }
                    });
                };
                _plugWatcher.Start();

                Debug.WriteLine("[WMI] 插入监听已启动");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WMI] 启动重连监听失败: {ex.Message}");
            }
        }

        // 线程安全的状态通知
        private void InvokeStateChanged(ConnectionState state, string message, bool canReconnect = false)
        {
            var args = new ConnectionStateChangedEventArgs
            {
                State = state,
                Message = message,
                CanReconnect = canReconnect
            };
            StateChanged?.Invoke(this, args);
        }
        public async Task<bool> ReconnectAsync()
        {
            await ConnectAsync();
            return IsConnected;
        }
        /// <summary>
        /// 从 WMI 获取端口名
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        private string GetPortNameFromWmi(ManagementBaseObject newEvent)
        {
            try
            {
                var targetInstance = newEvent["TargetInstance"] as ManagementBaseObject;
                return targetInstance?["DeviceID"]?.ToString();  // 如 "COM3"
            }
            catch
            {
                return null;
            }
        }


        object _lock = new();

        /// <summary>
        /// 清理端口
        /// </summary>
        private void CleanupPort()
        {
            lock (_lock)
            {
                if (_port != null)
                {
                    _port.DataReceived -= OnData;
                    try { _port.Close(); } catch { }
                    _port.Dispose();
                    _port = null;
                }
            }
        }
        /// <summary>
        /// 数据接收处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnData(object sender, SerialDataReceivedEventArgs e)
        {
           lock(_dataLock)
            {
                if(_receiveTcs == null)
                {
                    //没有等待中的任务，忽略
                    return;
                }
                try
                {
                    var bytesToReads = _port.BytesToRead;
                    var buffs= new byte[bytesToReads];
                    _port.Read(buffs, 0, bytesToReads);
                    _receiveBuffer.AddRange(buffs);
                    //检查是否接收完成（根据实际协议调整判断逻辑）
                    if (IsFrameComplete(_receiveBuffer))
                    {
                        _receiveTcs.TrySetResult(_receiveBuffer.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    _receiveTcs.TrySetException(ex);
                }

            }
        }

        /// <summary>
        /// 判断帧是否完整 
        /// </summary>
        private bool IsFrameComplete(List<byte> buffer)
        {
            if (buffer.Count < 5) return false; // 最小帧长度

            // 示例1：固定长度帧（如你的协议 0x55 0xAA ...）
            // 假设第4字节是长度字段
            int expectedLen = buffer[3] + 5; // 头(2) + 命令(1) + 长度(1) + 数据 + 校验(1)
            return buffer.Count >= expectedLen;

            // 示例2：以特定字节结尾（如 \r\n）
            // return buffer.Count >= 2 && 
            //        buffer[^2] == 0x0D && 
            //        buffer[^1] == 0x0A;

            // 示例3：超时判断（配合定时器）
            // return false; // 由超时定时器触发完成
        }
        private static readonly RingBuffer s_buffer = new(512);
        /// <summary>
        /// 尝试提取帧
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static bool TryExtractFrame(ReadOnlySpan<byte> raw, out byte[] frame)
        {
            frame = Array.Empty<byte>();

            // 1. 缓冲区里先把新数据追加进来（线程安全用 ConcurrentQueue 或 RingBuffer）
            s_buffer.Append(raw);   // 见下方 RingBuffer 实现

            // 2. 循环找帧
            while (s_buffer.Length > 4)   // 最小长度：地址+功能码+长度+CRC=5
            {
                int head = 0;
                ReadOnlySpan<byte> span = s_buffer.UnreadSpan;

                // 2.1 先找头（Modbus-RTU 任意字节都可能像头，只能靠长度+CRC 验证）
                byte addr = span[head];
                byte func = span[head + 1];
                byte len = span[head + 2];        // 仅对“读保持寄存器”响应有效

                int frameLen = 3 + len + 2;         // 地址+功能码+数据+CRC16
                if (s_buffer.Length < frameLen) return false; // 还不够，继续等

                ReadOnlySpan<byte> candidate = span.Slice(head, frameLen);
            }
            return false;
        }

        private sealed class RingBuffer
        {

            private readonly object _bufferLock = new();

            private readonly byte[] _buf;
            private int _write;
            private int _read;
            public int Length => _write - _read;
            public ReadOnlySpan<byte> UnreadSpan => _buf.AsSpan(_read, Length);
            public RingBuffer(int size) => _buf = new byte[size];
            public void Append(ReadOnlySpan<byte> data)
            {
                lock (_bufferLock)
                {
                    data.CopyTo(_buf.AsSpan(_write));
                    _write += data.Length;
                }
              //  data.CopyTo(_buf.AsSpan(_write));
                //_write += data.Length;
            }
            public void Skip(int cnt) => _read += cnt;
        }


        public void Dispose()
        {
            if (_port?.IsOpen == true)
            {
                _port.DataReceived -= OnData;
                _port.Close();
                _port.Dispose();
                _port = null;
            }
        }

        public async Task<byte[]> SendAndReceiveAsync( byte[] request, int timeoutMs = 5000, CancellationToken token = default)
        {
            lock(_dataLock)
            {
                if(_receiveTcs!=null&&!_receiveTcs.Task.IsCompleted)
                {
                    throw new InvalidOperationException("已有等待中的接收操作");
                }
                _receiveBuffer.Clear();
                _receiveTcs = new TaskCompletionSource<byte[]>();
                _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                _receiveCts.CancelAfter(timeoutMs);
            }
            //注册取消回调
            using (_receiveCts.Token.Register(()=>_receiveTcs.TrySetCanceled()))
            {
              await  _port.BaseStream.WriteAsync(request, 0, request.Length,token);
               await _port.BaseStream.FlushAsync(token);
                // 等待接收完成（由事件触发）
                return await _receiveTcs.Task;
            }
               
        }

        public bool IsConnected => _port?.IsOpen == true;
    }


}
