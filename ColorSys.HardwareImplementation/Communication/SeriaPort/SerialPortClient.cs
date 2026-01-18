using ColorSys.HardwareImplementation.Communication.CommParameter;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    internal class SerialPortClient : IDisposable
    {
        #region 对外事件
        /// <summary>收到数据（已转十六进制字符串）</summary>
        public event Action<string> DataReceivedHex;
        /// <summary>收到数据（原始字节）</summary>
        public event Action<byte[]> DataReceivedRaw;
        /// <summary>日志/调试信息</summary>
        public event Action<string> Log;
        /// <summary>异常发生</summary>
        public event Action<Exception> Error;
        /// <summary>连接状态变化</summary>
        public event Action<bool> ConnectStateChanged;
        #endregion

        #region 对外属性
        public bool IsOpen => _serialPort?.IsOpen == true;
        public string PortName => _serialPort?.PortName!;
        #endregion


        #region 私有字段
        private SerialPort _serialPort;
        private readonly object _lock = new object();
        private CancellationTokenSource _cts;
        private Task _monitorTask;
        private readonly StringBuilder _builder = new StringBuilder();
        #endregion

        #region 构造/析构

        private readonly SerialParameters _p;
        public SerialPortClient(SerialParameters serial) => _p = serial;
        public void Dispose() => Close();
        #endregion

        #region 打开/关闭
     
        /// </summary>
        /// 异步打开，带重试
        /// </summary>
        /// <param name="portName">COM1 / COM2 / ...</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="autoReconnect">是否启用掉线重连</param>
        /// <param name="maxRetries">最大重连次数</param>
        /// <param name="baseInterval">重连等待延时时长</param>
        /// <param name="token">取消令牌</param>
        /// <returns></returns>
        public Task OpenAsync(
                            bool autoReconnect = true,
                            int maxRetries = 3,
                            int baseInterval = 300,
                            CancellationToken token = default)
        {
            return OpenInternal(_p, autoReconnect, maxRetries, baseInterval, async: true, token);
        }

        private async Task OpenInternal(SerialParameters portModel,
                                       bool autoReconnect,
                                       int maxRetries, int baseInterval,
                                       bool async, CancellationToken token)
        {
            lock (_lock)
            {
                if (_serialPort != null)
                    throw new InvalidOperationException("串口已打开，请先 Close()");
            }


            Exception lastEx = null;
            var rnd = new Random();

            for (int i = 0; i <= maxRetries; i++)
            {
                if (i > 0)          // 第一次不延迟
                {
                   await Task.Delay(500, token);
                }

                try
                {
                    //var sp = new SerialPort(portModel.PortName, (int)portModel.BaudRate, portModel.Parity, (int)portModel.DataBits, portModel.StopBits)
                    //{
                    //    Encoding = Encoding.UTF8,
                    //    NewLine = "\r\n",
                    //    ReadTimeout = 500,
                    //    WriteTimeout = 500
                    //};

                    //if (async)
                    //    await Task.Run(() => sp.Open(), token);
                    //else
                    //    sp.Open();

                    // 成功：赋值 + 注册事件 + 启动监控任务
                    //lock (_lock)
                    //{
                    //    _serialPort = sp;
                    //    _serialPort.DataReceived += OnDataReceived;
                    //}

                    Log?.Invoke($"串口 {portModel.PortName} 打开成功（第 {i + 1} 次）");
                    ConnectStateChanged?.Invoke(true);

                    if (autoReconnect)
                    {
                        _cts = new CancellationTokenSource();
                        _monitorTask = Task.Run(MonitorLoop, _cts.Token);
                    }
                    return; // ⚡️ 成功就立即返回
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    Log?.Invoke($"第 {i + 1} 次打开失败：{ex.Message}");
                }
            }

            // 全部失败
            throw new Exception(
                $"串口 {portModel.PortName} 在 {maxRetries + 1} 次尝试后仍无法打开，最后一次异常：{lastEx?.Message}",
                lastEx);
        }

        /// <summary>
        /// 关闭串口（线程安全）
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                _cts?.Cancel();
                try { _monitorTask?.Wait(); } catch { /* ignored */ }

                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen) _serialPort.Close();
                    }
                    catch { /* ignored */ }
                    _serialPort.Dispose();
                    _serialPort = null;
                    Log?.Invoke("串口已关闭");
                    ConnectStateChanged?.Invoke(false);
                }
            }
        }
        #endregion

        #region 发送
        /// <summary>同步发送字节</summary>
        public void Send(byte[] buffer)
        {
            lock (_lock)
            {
                if (!IsOpen) throw new InvalidOperationException("串口未打开");
                _serialPort.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>同步发送字符串（按 Encoding.UTF8）</summary>
        public void Send(string text) => Send(Encoding.UTF8.GetBytes(text));

        /// <summary>同步发送十六进制字符串（例如 "01 03 00 00 00 0A C5 CD"）</summary>
        public void SendHex(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");
            if (hex.Length % 2 != 0) throw new ArgumentException("十六进制字符串长度应为偶数");
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            Send(bytes);
        }

        /// <summary>异步发送（内部仍锁串口，但方法返回 Task）</summary>
        public async Task SendAsync(byte[] buffer)
            => await Task.Run(() => Send(buffer));
        #endregion

        #region CRC16-ModBus 校验（静态方法，可直接用）
        public static byte[] Crc16ModBus(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return new[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }
        #endregion

        #region 内部数据接收
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = _serialPort.BytesToRead;
                byte[] buffer = new byte[len];
                _serialPort.Read(buffer, 0, len);

                // 原始字节事件
                Task.Run(() => DataReceivedRaw?.Invoke(buffer));

                // 十六进制字符串事件
                _builder.Clear();
                foreach (var b in buffer) _builder.AppendFormat("{0:X2} ");
                Task.Run(() => DataReceivedHex?.Invoke(_builder.ToString().Trim()));
            }
            catch (Exception ex)
            {
                Task.Run(() => Error?.Invoke(ex));
            }
        }
        #endregion

        #region 掉线重连监控
        private void MonitorLoop()
        {
            var token = _cts.Token;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_serialPort == null) break;
                    if (!_serialPort.IsOpen)
                    {
                        Log?.Invoke("检测到串口掉线，尝试重连...");
                        ConnectStateChanged?.Invoke(false);
                        try
                        {
                            _serialPort.Open();
                            Log?.Invoke("重连成功");
                            ConnectStateChanged?.Invoke(true);
                        }
                        catch (Exception ex)
                        {
                            Log?.Invoke($"重连失败：{ex.Message}，5 秒后再次尝试");
                        }
                    }
                }
                catch { /* ignored */ }
                Thread.Sleep(5000);
            }
        }
        #endregion
    }
}
