using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioSessionManager
	{
		void GetAudioSessionControl(ref Guid AudioSessionGuid, uint StreamFlags, [MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl SessionControl);

		void GetSimpleAudioVolume(ref Guid AudioSessionGuid, uint StreamFlags, [MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume AudioVolume);
	}
}
