using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	internal static class WinINet
	{
		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool InternetCheckConnection(string lpszUrl, int dwFlags, int dwReserved);
	}
}
