using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ColorSys.WPF.Converters
{
    public class LValueToPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double lValue)
            {
                // L值范围是0-100，对应亮度区域的高度200px
                // L=100 should be at top (Y=0), L=0 should be at bottom (Y=200)
                // So we need to invert the value: higher L value -> lower Y position
                double position = (100 - lValue) * 2; // 200px / 100 units = 2px per unit
                
                // Adjust for margin of the brightness bar (border starts at Y=5 due to Margin="0,5")
                // The TranslateTransform positions the top-left corner of the circle
                // Since circle is 16px tall, and we want to position the center of the circle
                // on the bar, we subtract half the height (8) to center it
                position = position + 5 - 8; // margin offset - half circle height
                
                // Make sure the position stays within bounds
                position = Math.Max(-8, Math.Min(200, position));
                return position;
            }
            return 100; // Default to middle position if value is null
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}