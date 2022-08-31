using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class YouHaveMinutesVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			int num = (int)((uint)values[0]);
			int num2 = (int)((uint)values[1]);
			bool flag = (bool)values[2];
			return (num == num2 && num > 0 && !flag) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
