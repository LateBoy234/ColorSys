using ColorSys.Domain.Model;
using ColorSys.Domain.StaticService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.WPF.ViewModels
{
    public  partial class ConnectViewModel:ObservableObject
    {

      
      
        [ObservableProperty]
        private ConnectionMethod _selectCommunicationType;
       
        
        private ConnectOptionViewModel? _serialOptions;
        public ConnectOptionViewModel SerialOptions =>
            _serialOptions ??= new ConnectOptionViewModel();
        public Array CommunicationTypeList => Enum.GetValues<ConnectionMethod>();

        [RelayCommand]
        partial void OnSelectCommunicationTypeChanged(ConnectionMethod value)
        {
            if (SerialOptions != null)
                SerialOptions.ParentCommType = value;
        }


    }

    public partial class ConnectOptionViewModel : ObservableObject
    {

        public ConnectOptionViewModel()
        {
            IP = "127.0.0.1";
            Port = 8080;
           var ports= SerialPortService.GetAllPorts();
            foreach (var port in ports)
            {
                _serialPortList.Add(port.PortName);
            }
            _selectedSerialPort = _serialPortList?.FirstOrDefault()??"无设备";

            SelectedBaudRate = BaudRate._9600;   // 对应 9600
            SelectedDateBit = DateBit._8;       // 同理，给其余属性也写上
        }
        [ObservableProperty]
        private string _iP;
        [ObservableProperty]
        private int _port;
        [ObservableProperty]
        private string _selectedSerialPort;
        [ObservableProperty]
        private BaudRate _selectedBaudRate;
        [ObservableProperty]
        private DateBit _selectedDateBit;
        [ObservableProperty]
        private ParityBit _selectedParityBit;
        [ObservableProperty]
        private StopBit _selectedStopBit;

        [ObservableProperty]
        private ConnectionMethod _parentCommType;

        public Array BaudRateList => Enum.GetValues<BaudRate>();
        public Array DateBitList => Enum.GetValues<DateBit>();
        public Array ParityBitList => Enum.GetValues<ParityBit>();
        public Array StopBitList => Enum.GetValues<StopBit>();

        [ObservableProperty]
        private ObservableCollection<string> _serialPortList = new ObservableCollection<string>();

    }
}
