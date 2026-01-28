using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.WPF.Resources
{
    public static  class LanguageSwith
    {
        public static string GetString(string key)
        {
            if (App.Current.Resources.Contains(key))
            {
                return App.Current.Resources[key].ToString() ?? key;
            }
            return key;
        }
    }
}
