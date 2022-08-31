using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class RangeEndDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			DateTime dateTime = DateTime.Now.AddSeconds((uint)value + 10U);
			return string.Format(TranslationSourceViewModel.Instance["NextOn"], dateTime.ToString("MMM dd"), dateTime.ToString("HH:mm"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
