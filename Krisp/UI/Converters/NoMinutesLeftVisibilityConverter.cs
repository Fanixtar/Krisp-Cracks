using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class NoMinutesLeftVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((uint)value > 0U) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
