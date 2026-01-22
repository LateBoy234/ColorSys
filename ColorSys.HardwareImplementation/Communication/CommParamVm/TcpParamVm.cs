using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ColorSys.HardwareImplementation.Communication.CommParamVm
{
    public class TcpParamVm : ObservableObject, IConfigViewModel
    {
        private string _ip = "192.168.1.100";
        private int _port = 502;

        [RegularExpression(@"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$")]   // 自定义验证特性
        public string IP
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        [Range(1, 65535)]
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public  ICommParameters GetConfig()
        {
            // 把界面当前值打成 DTO
            return new TcpParameters { IP = this.IP, Port = this.Port };
        }
    }
}
