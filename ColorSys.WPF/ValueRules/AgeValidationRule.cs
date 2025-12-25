using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ColorSys.WPF.ValueRules
{
    public class AgeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value==null||!int.TryParse(value.ToString(),out int age))
            {
                return new ValidationResult(false,"请输入有效年龄");
            }
            if(age<0||age>200)
            {
                return new ValidationResult(false, "请输入0~200的数字");
            }
            return ValidationResult.ValidResult;
        }
    }
}
