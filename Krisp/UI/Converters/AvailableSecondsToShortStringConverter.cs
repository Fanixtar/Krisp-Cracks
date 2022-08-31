using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class AvailableSecondsToShortStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int num = (int)((uint)value);
			if (num != 0)
			{
				return (num / 60).ToString();
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
