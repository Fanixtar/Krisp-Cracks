using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class ContentStringToImageSource : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format("/Krisp;component/Resources/Onboarding/{0}.png", (string)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
