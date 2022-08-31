using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public Visibility TrueValue { get; set; }

		public Visibility FalseValue { get; set; } = Visibility.Collapsed;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool flag = false;
			if (value is bool)
			{
				flag = (bool)value;
			}
			return flag ? this.TrueValue : this.FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Visibility? visibility = value as Visibility?;
			Visibility trueValue = this.TrueValue;
			return (visibility.GetValueOrDefault() == trueValue) & (visibility != null);
		}
	}
}
