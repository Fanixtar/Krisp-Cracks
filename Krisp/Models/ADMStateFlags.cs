using System;

namespace Krisp.Models
{
	[Flags]
	public enum ADMStateFlags : uint
	{
		HealtyState = 0U,
		UnHealtyState = 1U,
		UnRecoverable = 2U,
		KrispDeviceNotDetected = 268435464U,
		KrispDevice_Disabled = 268435472U,
		NoDeviceDetected = 268435488U,
		KrispDefaultNotPermited = 268435520U,
		UI_UnRecoverable = 268435456U,
		SP_NOTIFICATION_START_ERROR = 256U,
		SP_NOTIFICATION_DISCONNECTED = 512U,
		SP_NOTIFICATION_ACCESS_ERROR = 1024U,
		SP_ALL_STATES = 768U
	}
}
