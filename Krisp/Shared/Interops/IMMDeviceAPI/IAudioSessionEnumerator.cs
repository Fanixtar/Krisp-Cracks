using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IAudioSessionEnumerator
	{
		int GetCount();

		[return: MarshalAs(UnmanagedType.Interface)]
		IAudioSessionControl GetSession(int SessionCount);
	}
}
