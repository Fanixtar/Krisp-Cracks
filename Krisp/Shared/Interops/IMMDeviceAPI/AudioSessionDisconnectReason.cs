using System;

namespace Shared.Interops.IMMDeviceAPI
{
	internal enum AudioSessionDisconnectReason
	{
		DeviceRemoval,
		ServerShutdown,
		FormatChanged,
		SessionLogoff,
		SessionDisconnected,
		ExclusiveModeOverride
	}
}
