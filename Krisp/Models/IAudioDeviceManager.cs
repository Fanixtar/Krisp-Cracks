using System;
using Shared.Interops;

namespace Krisp.Models
{
	public interface IAudioDeviceManager : IDiagnosticsBase
	{
		event EventHandler<IAudioDevice> DefaultChanged;

		event EventHandler<bool> DevicesLoaded;

		IAudioDevice Default { get; }

		IAudioDevice PreviewDefault { get; }

		IAudioDeviceCollection Devices { get; }

		IAudioDevice GetDefaultDevice(ERole role = ERole.eMultimedia);

		void UnregisterEventNotifications();

		string KrispDevId { get; }

		HRESULT SetKrispDeviceStatus(bool bEnable);
	}
}
