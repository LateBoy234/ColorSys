using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using ColorSys.HardwareImplementation.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public class CRInstrument : IDevice, IMeasureMent
    {
        private readonly ICommunication _comm;
        
        public event EventHandler<StandarModel> DataReceived;
        
        public CRInstrument(ICommunication communication)
        {
            _comm = communication;
        }
        public DeviceType DeviceType => DeviceType.CR;

        public bool IsConnected => Comm.IsConnected;

        public ICommunication Comm => _comm;

       

        public async Task<bool> ConnectAsync()
        {
            await _comm.ConnectAsync();
            if (!_comm.IsConnected)
            {
                return false;
            }
            bool connected = await VerifyConnectionAsync();

            if (connected)
            {
                // 启动数据监听
                StartDataListener();
            }

            return connected;
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


        /// <summary>
        /// 通用的发送请求并等待响应的方法
        /// </summary>
        private async Task<ShannshiResponse?> SendRequestAsync(byte cmdType, byte[]? data = null, int timeoutMs = 3000)
        {
            var tcs = new TaskCompletionSource<ShannshiResponse?>();
            var tempAccumulator = new ShannshiFrameAccumulator();

            EventHandler<byte[]> handler = (s, rawData) =>
            {
                tempAccumulator.Append(rawData);
                while (tempAccumulator.ExtractFrame() is byte[] frame)
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

        public void Dispose()
        {
            Comm.Dispose(); ;
        }

        public async Task<StandarModel> RunTestAsync(CancellationToken token = default)
        {
            //var response = await SendRequestAsync(0xA6);

            //if (response != null && response.IsSuccess)
            //{
            //    // 这里根据 response.Data 解析测量数据
            //    // 例如：解析 L*, a*, b* 等
            //    return ParseTestData(response.Data);
            //}
            await Task.Delay(100); // 模拟异步操作
            return new StandarModel(); // 或者抛出异常/返回错误状态
        }
    }
}
