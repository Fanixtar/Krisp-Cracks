using System;
using System.Runtime.InteropServices;

namespace P7
{
	internal static class Dll
	{
		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Exceptional_Flush")]
		private static extern void P7_Exceptional_Flush32();

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Exceptional_Flush")]
		private static extern void P7_Exceptional_Flush64();

		public static void Exceptional_Buffers_Flush()
		{
			if (8 == IntPtr.Size)
			{
				Dll.P7_Exceptional_Flush64();
				return;
			}
			Dll.P7_Exceptional_Flush32();
		}

		public const string DLL_NAME_32 = "P7x32.dll";

		public const string DLL_NAME_64 = "P7x64.dll";
	}
}
