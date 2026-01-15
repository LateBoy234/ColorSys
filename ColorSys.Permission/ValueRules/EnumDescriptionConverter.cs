using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ColorSys.Permission.ValueRules
{
    /// <summary>
    /// 枚举描述转换器
    /// </summary>
    public  class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return string.Empty;
            }
            var type= value.GetType();
           var name =Enum.GetName(type, value);
            if(name == null)
            {
                return value.ToString()!;
            }
            var field=type.GetField(name);
            //if (field?.GetCustomAttribute<DescriptionAttribute>() is { } desc)
            //    return desc.Description;
            if (field?.GetCustomAttribute<DescriptionAttribute>() is { }desc)
            {
                return desc.Description;
            }
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
