using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.Model
{
    public interface IDeviceInfo
    {
        /// <summary>
        /// 仪器型号
        /// </summary>
        string Model { get; }

        /// <summary>
        /// 仪器序列号
        /// </summary>
        string SN { get; }

        /// <summary>
        /// 设备硬件版本号
        /// </summary>
        string HardwareRevision { get; }

        /// <summary>
        /// 设备固件版本号
        /// </summary>
        string FirmwareRevision { get; }

        /// <summary>
        /// 设备软件版本号
        /// </summary>
        string SoftwareRevision { get; }

        /// <summary>
        /// 设备制造商
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        /// 设备光学结构
        /// </summary>
        string OpticalStructure { get; }

        /// <summary>
        /// 配套白板编号
        /// </summary>
        string WhiteBoardNumber { get; }

        /// <summary>
        /// 设备类型
        /// </summary>
        DeviceType DeviceType { get; }

        /// <summary>
        /// 测量波长范围
        /// </summary>
       // WavelengthRange WavelengthRange { get; }
    }
}
