using ColorSys.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Domain.Config
{
    public  class SystemConfig
    {
        public string Key  { get; set; }
        public  string Value { get; set; }
        public ConnectionMethod  ConnectionMethod { get; set; }
    }
}
