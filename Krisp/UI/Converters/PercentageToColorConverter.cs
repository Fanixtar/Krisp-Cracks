using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class PercentageToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double num = (double)value;
			if (num > 35.0)
			{
				return "#6371DE";
			}
			if (num > 15.0)
			{
				return "#FF7E61";
			}
			return "#FF4646";
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
