using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	internal static class Combase
	{
		[DllImport("combase.dll", PreserveSig = false)]
		public static extern void RoGetActivationFactory([MarshalAs(UnmanagedType.HString)] string activatableClassId, [In] ref Guid iid, [MarshalAs(UnmanagedType.IInspectable)] out object factory);

		[DllImport("combase.dll", PreserveSig = false)]
		public static extern void WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string src, [In] uint length, out IntPtr hstring);
	}
}
