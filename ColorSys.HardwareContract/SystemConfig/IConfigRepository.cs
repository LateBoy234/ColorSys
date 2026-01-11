using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareContract.SystemConfig
{
    public  interface IConfigRepository
    {
        Task<Dictionary<string, string>> LoadAsync();
        Task SaveAsync(IReadOnlyDictionary<string, string> dict);
    }
}
