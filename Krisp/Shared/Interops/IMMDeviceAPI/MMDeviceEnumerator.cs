using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	[ComImport]
	public class MMDeviceEnumerator
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern MMDeviceEnumerator();
	}
}
