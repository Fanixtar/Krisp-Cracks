using System;

namespace Shared.Interops
{
	[Flags]
	public enum DeviceState : uint
	{
		ACTIVE = 1U,
		DISABLED = 2U,
		NOTPRESENT = 4U,
		UNPLUGGED = 8U,
		MASK_ALL = 15U
	}
}
