using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.Model
{

    public enum DeviceType
    {
        PTS,
        CR
    }
    public enum ConnectionMethod
    {
        TCP,
        USB,
        //ByBluettooth,
        //BySpecificalDevice
    }

    public enum BaudRate
    {
        [Description("9600")]
        _9600 = 9600,
        [Description("19200")]
        _19200 = 19200,
        [Description("115200")]
        _115200 = 115200
    }

    public enum DateBit
    {
        [Description("8")]
        _8 = 8,
        [Description("7")]
        _7 = 7,
    }

    public enum ParityBit
    {
        [Description("无校验位")]
        None = 0,
        [Description("奇检验")]
        Odd,
        [Description("偶检验")]
        Even
    }


    public enum StopBit
    {
        None = 0,
        One = 1,
        Two = 2,
        OnePointFive = 3,
    }

    public static class ParityMapper
    {
        public static Parity ToSystemParity(this ParityBit p) => p switch
        {
            ParityBit.None => Parity.None,
            ParityBit.Odd => Parity.Odd,
            ParityBit.Even => Parity.Even,
            _ => throw new ArgumentOutOfRangeException(nameof(p))
        };

        public static StopBits ToSystemStopBits(this StopBit s) => s switch
        {
            StopBit.None => StopBits.One,
            StopBit.One => StopBits.One,
            StopBit.Two => StopBits.Two,
            StopBit.OnePointFive => StopBits.OnePointFive,
            _ => throw new ArgumentOutOfRangeException(nameof(s))
        };
    }
}
