using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class DeviceUsageToTextConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Any((object e) => e == DependencyProperty.UnsetValue))
			{
				return DependencyProperty.UnsetValue;
			}
			bool flag = (bool)values[0];
			IList<string> list = values[1] as IList<string>;
			if (flag)
			{
				if (list != null)
				{
					int num = list.Distinct<string>().Count<string>();
					if (num == 1)
					{
						return string.Format(TranslationSourceViewModel.Instance["UsedByApp"], list[0]);
					}
					if (num > 1)
					{
						return string.Format(TranslationSourceViewModel.Instance["UsedByApps"], num);
					}
				}
				return TranslationSourceViewModel.Instance["InUse"];
			}
			return TranslationSourceViewModel.Instance["NotUsed"];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
