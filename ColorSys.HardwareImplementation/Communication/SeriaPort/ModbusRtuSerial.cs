using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.SeriaPort
{
    public class ModbusRtuSerial : ICommunication
    {
        private readonly SerialParameters _p;
        
        public ModbusRtuSerial(SerialParameters p)
        {
            _p = p;
        }

        private SerialPort? _port;

        private readonly Subject<byte[]> _frameSubject = new();

        public IObservable<byte[]> FrameStream => _frameSubject;
        public async Task ConnectAsync()
        {
            try
            {
                _port = new SerialPort(_p.PortName, _p.BaudRate, _p.Parity.ToSystemParity(), _p.DataBits, _p.StopBits.ToSystemStopBits());
                await Task.Run(() => _port.Open());
                _port.DataReceived += OnData;
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        private void OnData(object sender, SerialDataReceivedEventArgs e)
        {
            // 简单示例：按 Modbus 长度拼帧
            var sp = (SerialPort)sender!;
            var len = sp.BytesToRead;
            var buf = new byte[len];
            sp.Read(buf, 0, len);

            if (TryExtractFrame(buf, out var frame))
                _frameSubject.OnNext(frame);
        }
        private static readonly RingBuffer s_buffer = new(512);
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
                //if (Crc16Modbus(candidate[..^2]) == BitConverter.ToUInt16(candidate[^2..]))
                //{
                //    frame = candidate.ToArray();
                //    s_buffer.Skip(frameLen);        // 把这帧从缓冲区扔掉
                //    return true;
                //}
                //else
                //{
                //    // CRC 错，滑动 1 字节继续找
                //    s_buffer.Skip(1);
                //}
            }
            return false;
        }

        private sealed class RingBuffer
        {
            private readonly byte[] _buf;
            private int _write;
            private int _read;
            public int Length => _write - _read;
            public ReadOnlySpan<byte> UnreadSpan => _buf.AsSpan(_read, Length);
            public RingBuffer(int size) => _buf = new byte[size];
            public void Append(ReadOnlySpan<byte> data)
            {
                data.CopyTo(_buf.AsSpan(_write));
                _write += data.Length;
            }
            public void Skip(int cnt) => _read += cnt;
        }

      
        public void Dispose()
        {
            _port?.Dispose();   
            _port = null;
        }

        public Task SendAsync(byte[] frame)
        {
            try
            {
                _port.Write(frame, 0, frame.Length);
                return Task.CompletedTask;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool IsConnected => _port?.IsOpen == true;

      
    }

   
}
