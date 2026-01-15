using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.Model
{
    /// <summary>
    /// 串口信息实体
    /// </summary>
    public class SerialPortInfo
    {
        public string PortName { get; set; } = string.Empty;
        public string? Description { get; set; }          // 设备管理器里的“友好名称”
        public override string ToString() => $"{PortName} | {Description}";
    }

}
