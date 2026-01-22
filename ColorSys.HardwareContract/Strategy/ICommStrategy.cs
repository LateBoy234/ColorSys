using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.Strategy
{
    // 通讯策略
    public interface ICommStrategy
    {
        string Displayname { get; }
        IConfigViewModel CreateConfigViewModel();
        ICommunication  creatCommunication(ICommParameters commParameters);
    }
}
