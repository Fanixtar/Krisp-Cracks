using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Shared.Interops
{
	internal static class Msi
	{
		[DllImport("msi.dll", CharSet = CharSet.Ansi)]
		public static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);

		[DllImport("msi.dll", CharSet = CharSet.Ansi)]
		public static extern int MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref int len);

		[DllImport("msi.dll", CharSet = CharSet.Ansi)]
		public static extern int MsiGetFileSignatureInformation(string fileName, int flags, out IntPtr certContext, IntPtr hashData, ref int hashDataLength);

		[DllImport("Crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int CertFreeCertificateContext(IntPtr certContext);

		[DllImport("Crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int CertGetNameString(IntPtr certContext, uint type, uint flags, IntPtr typeParameter, StringBuilder stringValue, uint stringLength);
	}
}
