using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFRtspServerAsync
{
    class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string l_result = "";

            try
            {

                var l_gameData = (uint)value;

                l_gameData /= 1024;

                l_result = l_gameData.ToString() + " kbit";

            }
            catch (Exception)
            {
                
            }

            return l_result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
