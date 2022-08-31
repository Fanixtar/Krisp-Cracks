﻿using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioSessionManager2
	{
		void GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags, [MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl SessionControl);

		void GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags, [MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume AudioVolume);

		[return: MarshalAs(UnmanagedType.Interface)]
		IAudioSessionEnumerator GetSessionEnumerator();

		void RegisterSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionNotification SessionNotification);

		void UnregisterSessionNotification([MarshalAs(UnmanagedType.Interface)] IAudioSessionNotification SessionNotification);

		void RegisterDuckNotification([MarshalAs(UnmanagedType.LPWStr)] string sessionID, [MarshalAs(UnmanagedType.Interface)] IAudioVolumeDuckNotification duckNotification);

		void UnregisterDuckNotification([MarshalAs(UnmanagedType.Interface)] IAudioVolumeDuckNotification duckNotification);
	}
}
