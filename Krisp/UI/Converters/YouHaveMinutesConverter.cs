using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class YouHaveMinutesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format(TranslationSourceViewModel.Instance["YouHaveMinutes"], ((int)((uint)value / 60U)).ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
