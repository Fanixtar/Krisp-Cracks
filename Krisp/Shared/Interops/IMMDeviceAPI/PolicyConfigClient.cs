using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
	[ComImport]
	internal class PolicyConfigClient
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern PolicyConfigClient();
	}
}
