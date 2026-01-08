using ColorSys.HardwareContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract
{
    public  interface IMeasureMent
    {
        IObservable<TestModel> TestStream { get; }   // 热数据流
        Task RunTestAsync(CancellationToken token = default);
    }
}
