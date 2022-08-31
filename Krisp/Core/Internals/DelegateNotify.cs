using System;
using System.Runtime.InteropServices;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public delegate void DelegateNotify(uint sesId, SPNotificationType notifyCode, HRESULT hr, [MarshalAs(UnmanagedType.LPWStr)] string Message);
}
