using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Strategy;
using ColorSys.HardwareImplementation.Communication.CommParameter;
using ColorSys.HardwareImplementation.Communication.CommParamVm;
using ColorSys.HardwareImplementation.Communication.SeriaPort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.StrategyImp.Communication
{
    public  class SerialCommStrategy : ICommStrategy
    {
        public string Displayname => "串口 (RS232/485)";


        public ICommunication creatCommunication(ICommParameters commParameters)
        {
            return new ModbusRtuSerial((SerialParameters)commParameters);
        }

        IConfigViewModel ICommStrategy.CreateConfigViewModel()
        {
           return new SerialParamVm();
        }
    }
}
