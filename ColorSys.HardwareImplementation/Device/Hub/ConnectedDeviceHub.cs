using ColorSys.HardwareContract;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device.Hub
{
    public sealed class ConnectedDeviceHub :ObservableObject, IConnectedDeviceHub
    {
        private IDevice? _current;
        public IDevice? Current
        {
            get => _current;
            set => SetProperty(ref _current, value); // 通知 PropertyChanged
        }
    }

    public sealed class GetHubRequest : RequestMessage<IConnectedDeviceHub> { }
}
