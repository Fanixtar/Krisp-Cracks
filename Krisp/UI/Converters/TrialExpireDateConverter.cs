using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class TrialExpireDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DateTime.Now.AddSeconds((uint)value).ToString("dd MMM yyyy");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
