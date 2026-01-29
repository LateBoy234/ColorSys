using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorSys.HardwareImplementation.Communication
{
    public class ShannshiResponse
    {
        public byte Cmd { get; set; }
        public byte Ack { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public bool IsSuccess => Ack == 0;

        public override string ToString()
        {
            return $"Cmd: 0x{Cmd:X2}, Ack: {Ack}, DataLen: {Data.Length}";
        }
    }

    public class ShannshiProtocol
    {
        public const ushort START_FLAG = 0x55AA;

        public static byte[] CreateRequest(byte cmdType, byte[]? data = null)
        {
            int dataLen = data?.Length ?? 0;
            // 报文结构: Header(2) + Cmd(1) + Len(4) + Data(n) + Checksum(2)
            // 注意：请求报文没有 Ack 位，或者结构稍有不同？
            // 根据 A1 指令: 55 AA A1 00 00 00 02 00 02
            // 这里 00 00 00 02 看起来是 Len，包含了最后的 2 字节 Checksum。
            
            int totalLen = 7 + dataLen + 2; 
            byte[] frame = new byte[totalLen];
            frame[0] = 0x55;
            frame[1] = 0xAA;
            frame[2] = cmdType;
            
            // Len (4字节) - 包含数据和校验和的长度
            uint lenField = (uint)(dataLen + 2);
            frame[3] = (byte)((lenField >> 24) & 0xFF);
            frame[4] = (byte)((lenField >> 16) & 0xFF);
            frame[5] = (byte)((lenField >> 8) & 0xFF);
            frame[6] = (byte)(lenField & 0xFF);

            if (data != null && dataLen > 0)
            {
                Array.Copy(data, 0, frame, 7, dataLen);
            }

            // Checksum = Len 和 Data 逐字节相加
            ushort checksum = 0;
            checksum += frame[3];
            checksum += frame[4];
            checksum += frame[5];
            checksum += frame[6];
            for (int i = 0; i < dataLen; i++)
            {
                checksum += data![i];
            }

            frame[totalLen - 2] = (byte)((checksum >> 8) & 0xFF);
            frame[totalLen - 1] = (byte)(checksum & 0xFF);

            return frame;
        }

        public static bool TryParseResponse(byte[] buffer, out ShannshiResponse? response)
        {
            response = null;

            if (buffer == null || buffer.Length < 8) return false;
            if (buffer[0] != 0x55 || buffer[1] != 0xAA) return false;

            byte cmdType = buffer[2];
            byte ack = buffer[3];
            
            // Len (4字节)
            uint dataLenField = (uint)((buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7]);
            
            // 总长度 = 8 (头) + dataLenField
            if (buffer.Length < 8 + dataLenField) return false;

            // 校验和是最后两个字节
            int checksumIndex = 8 + (int)dataLenField - 2;
            ushort receivedChecksum = (ushort)((buffer[checksumIndex] << 8) | buffer[checksumIndex + 1]);
            
            // 计算校验和 (Len字段4字节 + Data部分)
            ushort calculatedChecksum = 0;
            calculatedChecksum += buffer[4];
            calculatedChecksum += buffer[5];
            calculatedChecksum += buffer[6];
            calculatedChecksum += buffer[7];

            int payloadLen = (int)dataLenField - 2;
            byte[] payload = Array.Empty<byte>();
            
            if (payloadLen > 0)
            {
                payload = new byte[payloadLen];
                Array.Copy(buffer, 8, payload, 0, payloadLen);
                for (int i = 0; i < payloadLen; i++)
                {
                    calculatedChecksum += payload[i];
                }
            }

            // 暂时根据用户要求，校验和失败也返回（或者您可以取消下面的注释）
            // if (receivedChecksum != calculatedChecksum) return false;

            response = new ShannshiResponse
            {
                Cmd = cmdType,
                Ack = ack,
                Data = payload
            };

            return true;
        }
    }

    /// <summary>
    /// 用于累积字节并提取完整的 3nh 协议帧
    /// </summary>
    public class ShannshiFrameAccumulator
    {
        private readonly List<byte> _buffer = new List<byte>();

        public void Append(byte[] data)
        {
            if (data != null)
                _buffer.AddRange(data);
        }

        public byte[] ExtractFrame()
        {
            while (_buffer.Count >= 8) // 至少要有头(2) + Cmd(1) + Ack(1) + Len(4)
            {
                // 查找头 55 AA
                int index = -1;
                for (int i = 0; i < _buffer.Count - 1; i++)
                {
                    if (_buffer[i] == 0x55 && _buffer[i + 1] == 0xAA)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    // 没找到头，清空只剩最后一个字节（万一它是 0x55）
                    byte last = _buffer.Count > 0 ? _buffer[_buffer.Count - 1] : (byte)0;
                    _buffer.Clear();
                    if (last == 0x55) _buffer.Add(last);
                    return null;
                }

                if (index > 0)
                {
                    _buffer.RemoveRange(0, index);
                }

                if (_buffer.Count < 8) return null;

                // 获取数据长度
                uint dataLen = (uint)((_buffer[4] << 24) | (_buffer[5] << 16) | (_buffer[6] << 8) | _buffer[7]);
                int totalLen =8+ (int)dataLen; // 加上固定字节 (Start2+Cmd1+Ack1+Len4+Checksum2 = 10)

                if (_buffer.Count >= totalLen)
                {
                    byte[] frame = new byte[totalLen];
                    _buffer.CopyTo(0, frame, 0, totalLen);
                    _buffer.RemoveRange(0, totalLen);
                    return frame;
                }
                else
                {
                    // 数据不够一帧
                    return null;
                }
            }
            return null;
        }

        public void Clear()
        {
            _buffer.Clear();
        }
    }
}
