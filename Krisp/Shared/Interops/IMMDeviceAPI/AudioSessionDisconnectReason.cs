using System;

namespace Shared.Interops.IMMDeviceAPI
{
	public enum AudioSessionDisconnectReason
	{
		DeviceRemoval,
		ServerShutdown,
		FormatChanged,
		SessionLogoff,
		SessionDisconnected,
		ExclusiveModeOverride
	}
}
