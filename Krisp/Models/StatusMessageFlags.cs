using System;

namespace Krisp.Models
{
	[Flags]
	public enum StatusMessageFlags
	{
		ShowStatus = 1,
		DisabledToggle = 2,
		DisabledDeviceCombo = 4
	}
}
