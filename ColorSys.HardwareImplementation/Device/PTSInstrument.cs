using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using ColorSys.HardwareImplementation.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public class PTSInstrument : IDevice
    {
        private readonly ICommunication _comm;

        // 存储解析出的仪器信息
        public string InstrumentName { get; private set; } = "";
        public string Model { get; private set; } = "";
        public string Version { get; private set; } = "";
        public string InternalWhiteboardSN { get; private set; } = "";
        public string ExternalWhiteboardSN { get; private set; } = "";
        public ushort MaxStorage { get; private set; }
        public ushort StdCount { get; private set; }
        public ushort SampleCount { get; private set; }
        public bool IsWhiteCalibrated { get; private set; }
        public bool IsBlackCalibrated { get; private set; }

        public PTSInstrument(ICommunication comm)   // Autofac 自动注入
        {
            _comm = comm;
        }
        public DeviceType DeviceType => DeviceType.PTS;

        public bool IsConnected => _comm.IsConnected;
        public ICommunication Comm => _comm;

        public async Task<bool> ConnectAsync()
        {
            // 1. 建立物理层连接
            await _comm.ConnectAsync();
            if (!_comm.IsConnected)
            {
                return false;
            }

            // 2. 发送 0xA1 指令查询状态并校验协议层连接
            return await VerifyConnectionAsync();
        }

        /// <summary>
        /// 通用的发送请求并等待响应的方法
        /// </summary>
        private async Task<ShannshiResponse?> SendRequestAsync(byte cmdType, byte[]? data = null, int timeoutMs = 3000)
        {
            var tcs = new TaskCompletionSource<ShannshiResponse?>();
            var accumulator = new ShannshiFrameAccumulator();

            EventHandler<byte[]> handler = (s, rawData) =>
            {
                accumulator.Append(rawData);
                while (accumulator.ExtractFrame() is byte[] frame)
                {
                    if (ShannshiProtocol.TryParseResponse(frame, out var response))
                    {
                        // 匹配命令码（请求和响应的命令码应该一致）
                        if (response!.Cmd == cmdType)
                        {
                            tcs.TrySetResult(response);
                        }
                    }
                }
            };

            _comm.DataReceived += handler;
            try
            {
                var request = ShannshiProtocol.CreateRequest(cmdType, data);
                System.Diagnostics.Debug.WriteLine($"发送指令 [0x{cmdType:X2}]: {BitConverter.ToString(request)}");
                await _comm.SendAsync(request);

                using var cts = new CancellationTokenSource(timeoutMs);
                using (cts.Token.Register(() => tcs.TrySetResult(null)))
                {
                    return await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"发送指令 [0x{cmdType:X2}] 异常: {ex.Message}");
                return null;
            }
            finally
            {
                _comm.DataReceived -= handler;
            }
        }

        private async Task<bool> VerifyConnectionAsync()
        {
            System.Diagnostics.Debug.WriteLine("开始发送 0xA1 指令校验协议层连接...");
            
            var response = await SendRequestAsync(0xA1);
            
            if (response != null && response.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine("0xA1 指令响应成功，开始解析仪器信息...");
                if (ParseInstrumentInfo(response.Data))
                {
                    System.Diagnostics.Debug.WriteLine("仪器信息解析成功，协议层连接已确认");
                    return true;
                }
                else
                {
                    _comm.Dispose();
                    System.Diagnostics.Debug.WriteLine("仪器信息解析失败");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(response == null ? "0xA1 指令响应超时" : $"0xA1 指令响应失败: Ack={response.Ack}");
                if (response == null)
                {
                    _comm.Dispose();
                }
            }
            _comm.Dispose();
            return false;
        }

        private bool ParseInstrumentInfo(byte[] data)
        {
            try
            {
                // 根据协议图片解析数据内容
                // 字符串字段格式: [长度1字节] + [内容n字节]
                // 仪器名(1+9) + 机号(1+9) + 版本号(1+14) + 内部白板(1+6) + 外部白板(1+6)
                // 数值字段: 可存储量(2) + 已存标样(2) + 已存试样(2) + 白板校正(1) + 黑板校正(1)
                // 总预计字节: 10 + 10 + 15 + 7 + 7 + 2 + 2 + 2 + 1 + 1 = 57 字节
                if (data == null || data.Length < 50) return false;

                int offset = 0;

                // 1. 仪器名
                byte nameLen = data[offset++];
                InstrumentName = Encoding.ASCII.GetString(data, offset, nameLen).TrimEnd('\0'); 
                offset += nameLen;

                // 2. 机号
                byte modelLen = data[offset++];
                Model = Encoding.ASCII.GetString(data, offset, modelLen).TrimEnd('\0'); 
                offset += modelLen;

                // 3. 版本号
                byte versionLen = data[offset++];
                Version = Encoding.ASCII.GetString(data, offset, versionLen).TrimEnd('\0'); 
                offset += versionLen;

                // 4. 内部白板序列号
                byte inWbLen = data[offset++];
                InternalWhiteboardSN = Encoding.ASCII.GetString(data, offset, inWbLen).TrimEnd('\0'); 
                offset += inWbLen;

                // 5. 外部白板序列号
                byte exWbLen = data[offset++];
                ExternalWhiteboardSN = Encoding.ASCII.GetString(data, offset, exWbLen).TrimEnd('\0'); 
                offset += exWbLen;

                // 6. 数值解析 (Big-Endian)
                MaxStorage = (ushort)((data[offset] << 8) | data[offset + 1]); offset += 2;
                StdCount = (ushort)((data[offset] << 8) | data[offset + 1]); offset += 2;
                SampleCount = (ushort)((data[offset] << 8) | data[offset + 1]); offset += 2;

                // 7. 标志位
                IsWhiteCalibrated = data[offset++] == 1;
                IsBlackCalibrated = data[offset++] == 1;

                return !string.IsNullOrWhiteSpace(InstrumentName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"解析仪器信息异常: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            Comm.Dispose();
        }

        public async Task<TestModel> RunTestAsync(CancellationToken token = default)
        {
            // 示例：执行测量命令 0xA6
            var response = await SendRequestAsync(0xA6);

            if (response != null && response.IsSuccess)
            {
                // 这里根据 response.Data 解析测量数据
                // 例如：解析 L*, a*, b* 等
                return ParseTestData(response.Data);
            }

            return new TestModel(); // 或者抛出异常/返回错误状态
        }

        private TestModel ParseTestData(byte[] data)
        {
            // 根据 0xA6 指令返回的协议格式进行解析
            // 暂返回一个空模型示例
            return new TestModel();
        }
    }
}
