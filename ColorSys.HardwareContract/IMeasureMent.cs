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
        event EventHandler<TestModel> DataReceived;
        Task<TestModel> RunTestAsync(CancellationToken token = default);
    }
}
