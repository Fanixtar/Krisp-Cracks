using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class MintoMinsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((uint)value / 60U < 2U)
			{
				return TranslationSourceViewModel.Instance["MinuteRemaining"];
			}
			return TranslationSourceViewModel.Instance["MinutesRemaining"];
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
