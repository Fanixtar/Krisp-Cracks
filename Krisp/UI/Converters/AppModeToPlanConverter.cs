using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class AppModeToPlanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = value.ToString();
			if (text == "minutes")
			{
				return TranslationSourceViewModel.Instance["Free"];
			}
			if (text == "trial")
			{
				return TranslationSourceViewModel.Instance["FreePro"];
			}
			text == "unlimited";
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
