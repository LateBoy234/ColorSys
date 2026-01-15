using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.StaticService
{
    public static class SerialPortService
    {
        /// <summary>
        /// 获取本机全部串口（含描述）
        /// </summary>
        public static List<SerialPortInfo> GetAllPorts()
        {
            // 1. WMI 取描述
            var descMap = new Dictionary<string, string>();
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'");
                foreach (var mo in searcher.Get())
                {
                    string? caption = mo["Caption"]?.ToString();
                    if (string.IsNullOrWhiteSpace(caption)) continue;
                    // 解析出 COM 号
                    int s = caption.IndexOf('('), e = caption.LastIndexOf(')');
                    if (s < 0 || e < 0) continue;
                    string com = caption.Substring(s + 1, e - s - 1).Trim(); // "COM3"
                    descMap[com.ToUpperInvariant()] = caption;
                }
            }
            catch { /* 旧系统或权限不足就放弃描述 */ }

            // 2. 合并
            return SerialPort.GetPortNames()
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .Select(n => new SerialPortInfo
                             {
                                 PortName = n,
                                 Description = descMap.TryGetValue(n.ToUpperInvariant(), out var d) ? d : "未知设备"
                             })
                             .OrderBy(p => p.PortName)
                             .ToList();
        }

        /// <summary>
        /// 按关键字（端口名或描述）过滤
        /// </summary>
        /// <param name="keyword">关键字；空/空白 返回全部</param>
        public static List<SerialPortInfo> SearchPorts(string? keyword)
        {
            var ports = GetAllPorts();
            if (string.IsNullOrWhiteSpace(keyword)) return ports;

            return ports
                .Where(p => p.PortName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            (p.Description != null &&
                             p.Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }

    }
}
