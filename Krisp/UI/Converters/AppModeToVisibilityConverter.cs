using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class AppModeToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = value.ToString();
			if (!(text == "minutes") && !(text == "trial"))
			{
				return "Collapsed";
			}
			return "Visible";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
