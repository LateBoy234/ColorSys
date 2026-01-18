using ColorSys.HardwareContract.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract
{
    public  interface IMeasureMent
    {
       // IObservable<TestModel> TestStream { get; }   // 热数据流
        Task<TestModel> RunTestAsync(CancellationToken token = default);
    }
}
