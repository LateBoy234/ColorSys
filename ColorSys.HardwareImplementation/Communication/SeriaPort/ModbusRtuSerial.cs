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

        public event EventHandler<ConnectionStateChangedEventArgs> StateChanged;
        public event EventHandler<byte[]> DataReceived;
        public bool SupportsPlugDetect => true;  // 支持 WMI 检测

        private ManagementEventWatcher _plugWatcher;
        private ManagementEventWatcher _unplugWatcher;
        public ModbusRtuSerial(SerialParameters p)
        {
            _p = p;
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

                // 2. 启动 WMI 监听
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
            // 简单示例：按 Modbus 长度拼帧
            var sp = (SerialPort)sender!;
            var len = sp.BytesToRead;
            if (len > 0)
            {
                var buf = new byte[len];
                sp.Read(buf, 0, len);
                DataReceived?.Invoke(this, buf);
            }
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

        public Task SendAsync(byte[] frame)
        {
            try
            {
                _port.Write(frame, 0, frame.Length);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                //  _logger.LogError(ex, "Unexpected heartbeat failure");
                throw;
            }
        }

        public bool IsConnected => _port?.IsOpen == true;
    }


}
