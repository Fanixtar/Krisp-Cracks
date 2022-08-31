using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	public class Propsys
	{
		[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int PSGetNameFromPropertyKey(ref PROPERTYKEY propkey, [MarshalAs(UnmanagedType.LPWStr)] out string ppszCanonicalName);
	}
}
