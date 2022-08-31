using System;
using System.Globalization;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class AvailableSecondsToLongStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			uint num = (uint)value;
			TimeSpan timeSpan = TimeSpan.FromSeconds(num);
			string text;
			if (timeSpan.TotalMinutes < 1.0)
			{
				text = string.Format("00:{0:D2}", timeSpan.Seconds);
			}
			else
			{
				text = string.Format("{0:D2}:{1:D2}", num / 60U, timeSpan.Seconds);
			}
			return text.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
