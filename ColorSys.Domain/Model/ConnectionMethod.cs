using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.Model
{
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
        StopBit1,
        StopBit2,
    }
}
