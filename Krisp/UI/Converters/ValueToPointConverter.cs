using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Krisp.UI.Converters
{
	internal class ValueToPointConverter : IValueConverter
	{
		public double Radius { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double num = (double)value / 100.0 * 360.0 * 3.141592653589793 / 180.0;
			double num2 = Math.Sin(num) * this.Radius + this.Radius;
			double num3 = -Math.Cos(num) * this.Radius + this.Radius;
			return new Point(num2, num3);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
