using Autofac;
using Autofac.Core;
using ColorSys.Domain.Model;
using ColorSys.Domain.StaticService;
using ColorSys.HardwareContract;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using ColorSys.HardwareImplementation.Device;
using ColorSys.HardwareImplementation.Device.Hub;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColorSys.WPF.ViewModels
{
    public partial class ConnectViewModel : ObservableObject
    {

        //public IDevice? ColorDevice { get; set; }
        //private ICommunication? Comm;

        private readonly Func<string, IDevice> _deviceFactory; // Autofac 自动生成
        public ConnectViewModel(Func<string, IDevice> deviceFactory)
        {
            _deviceFactory = deviceFactory;
        }

        [ObservableProperty]
        private ConnectionMethod _selectCommunicationType;


        private ConnectOptionViewModel? _serialOptions;
        public ConnectOptionViewModel SerialOptions =>
            _serialOptions ??= new ConnectOptionViewModel();
        public Array CommunicationTypeList => Enum.GetValues<ConnectionMethod>();
        public Array DeviceTypeList => Enum.GetValues<DeviceType>();

        [ObservableProperty]
        private DeviceType _selectDeviceType;

        [RelayCommand]
        partial void OnSelectCommunicationTypeChanged(ConnectionMethod value)
        {
            if (SerialOptions != null)
                SerialOptions.ParentCommType = value;
        }

        [RelayCommand]
        public async Task Connect(Window win)
        {
            var para = WeakReferenceMessenger.Default.Send(new GetSerialParaRequest()).Response;
            para.PortName = SerialOptions.SelectedSerialPort;
            para.DataBits = (int)SerialOptions.SelectedDateBit;
            para.BaudRate =(int) SerialOptions.SelectedBaudRate;
            para.Parity = SerialOptions.SelectedParityBit;
            para.StopBits = SerialOptions.SelectedStopBit;



            var key = $"{SelectDeviceType}-{SelectCommunicationType}"; // "PTS-rtu"
            var device = _deviceFactory(key);             // 直接拿到实例
            await device.ConnectAsync();
            if (device.IsConnected)
            {
                var hub = WeakReferenceMessenger.Default.Send(new GetHubRequest());
                var instance = hub.Response;
                instance.Current=device;
                win.DialogResult = true;
                win.Close();
            }
        }

        [RelayCommand]
        private void Cancel(Window win)
        {
            win.DialogResult = true;
            win.Close();
        }
    }

    public partial class ConnectOptionViewModel : ObservableObject
    {

        public ConnectOptionViewModel()
        {
            IP = "127.0.0.1";
            Port = 8080;
            var ports = SerialPortService.GetAllPorts();
            foreach (var port in ports)
            {
                _serialPortList.Add(port.PortName);
            }
            _selectedSerialPort = _serialPortList?.FirstOrDefault() ?? "无设备";

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
