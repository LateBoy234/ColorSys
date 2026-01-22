using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.CommParamVm;
using ColorSys.HardwareImplementation.Communication.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.StrategyImp.Communication
{
    public  class TcpCommStrategy : ICommStrategy
    {
        public string Displayname => "TCP/IP";


        public ICommunication creatCommunication(ICommParameters commParameters)
        {
           return new TcpCommunication((TcpParameters)commParameters);
        }

        public IConfigViewModel CreateConfigViewModel()
        {
            return new TcpParamVm();
        }
    }
}
