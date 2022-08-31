using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class DaysToGiftMessageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int num = (int)((uint)value / 60U / 60U / 24U + 1U);
			return string.Format(TranslationSourceViewModel.Instance["GiftMessage"], num.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
