using ColorSys.HardwareContract;
using HslCommunication;
using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.PLC
{
    public class Simens
    {

        public Simens(string ip)
        {
            S7 = new SiemensS7Net(SiemensPLCS.S1200,ip);
            ConnectAsync();
            Loop();
        }  

        public string ConnectionId { get; }

        private bool _run = true;
        private ushort _lastHb;
        public SiemensS7Net S7;

        public event Action<string> Alarm;
        public bool IsConnected => throw new NotImplementedException();

        public Task ConnectAsync(CancellationToken token = default)
        {
            try
            {
                OperateResult result = S7.ConnectServer();
                if (result.IsSuccess)
                {
                    return Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
            return Task.CompletedTask;

        }

        private async Task Loop()
        {
            int i = 0;
            while (_run)
            {
                try
                {
                    // 1. 读回心跳字
                    var bytes =await S7.ReadAsync("DB1.DBD1",1);
                    var hb = bytes.Content[0];
                    _lastHb = (ushort)(hb + 1);
                    // 2. 检测 PLC 侧是否变化（上位机→PLC 写失败）
                    if (hb == _lastHb && _lastHb != 0&&i<5)
                    {
                        i++;
                        Alarm?.Invoke("PLC 未收到心跳，可能写失败或断线！");
                    }
                    else if(i>=5)
                    {
                        i = 0;
                        Alarm?.Invoke($"PLC 5次 未收到心跳 尝试断线重连");
                        await TryReconnect();
                    }

                    // 3. 自增并写回
                    
                    if(_lastHb>100)
                    {
                        _lastHb = 0;
                    }
                    await  S7.WriteAsync("DB1.DBD1",  BitConverter.GetBytes(_lastHb));
                    

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Alarm?.Invoke($"心跳异常: {ex.Message}");
                   await TryReconnect();
                }
            }
        }

        private async Task TryReconnect()
        {
            S7.ConnectClose();
            while (_run)
            {
                var isconnected = await Task.Run(() =>  S7.ConnectServer().IsSuccess );
                if (isconnected)
                {
                    Alarm?.Invoke("重连成功");
                    break;
                }
                await Task.Delay(5000);
            }
        }

        public Task DisconnectAsync(CancellationToken token = default)
        {
            _run = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _run = false; ;
        }

        public Task<byte[]> SendAsync(byte[] data, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            //S7 = new SiemensS7Net(SiemensPLCS.S1200,"127.0.0.1");
            //Loop();
        }
    }
}
