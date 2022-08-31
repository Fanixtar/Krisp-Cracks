using System;
using System.Globalization;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Converters
{
	public class RoomEchoCancelationStateToBoolean : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			RoomEchoCancelationState roomEchoCancelationState = RoomEchoCancelationState.Disabled;
			if (value is RoomEchoCancelationState)
			{
				roomEchoCancelationState = (RoomEchoCancelationState)value;
			}
			return roomEchoCancelationState == RoomEchoCancelationState.Available;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new Exception("Cannot convert back.");
		}
	}
}
