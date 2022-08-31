using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	public struct SPSDKModel
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string FilePath;
	}
}
