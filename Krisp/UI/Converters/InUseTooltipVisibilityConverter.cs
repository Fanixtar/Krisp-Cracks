using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	public class InUseTooltipVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Any((object e) => e == DependencyProperty.UnsetValue))
			{
				return DependencyProperty.UnsetValue;
			}
			bool flag = (bool)values[0];
			IList<string> list = values[1] as IList<string>;
			return (flag && list != null && list.Count == 0) ? Visibility.Hidden : Visibility.Visible;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
