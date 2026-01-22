using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Communication.CommParamVm
{
    public partial  class SerialParamVm : ObservableObject, IConfigViewModel
    {
        #region ==== 真实绑定值 ====
        [Required(ErrorMessage = "串口号不能为空")]
        private string _portName = string.Empty;

      
        private int _baudRate = 9600;

        [Range(5, 9, ErrorMessage = "数据位只能是 5-9")]
        private int _dataBits = 8;

        [EnumDataType(typeof(ParityBit), ErrorMessage = "请选择有效校验位")]
        private ParityBit _parity = ParityBit.None;

        [EnumDataType(typeof(StopBit), ErrorMessage = "请选择有效停止位")]
        private StopBit _stopBits = StopBit.One;
        #endregion

        #region ==== 属性（带验证）====
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        public int DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        public ParityBit Parity
        {
            get => _parity;
            set => SetProperty(ref _parity, value);
        }

        public StopBit StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }
        #endregion


        #region ==== 转 DTO ====
        public  ICommParameters GetConfig() => new SerialParameters
        {
            PortName = this.PortName,
            BaudRate = this.BaudRate,
            DataBits = this.DataBits,
            Parity = this.Parity,
            StopBits = this.StopBits
        };
        #endregion
    }
}
