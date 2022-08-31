using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMDevice
	{
		HRESULT Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.Interface)] out object ppInterface);

		[return: MarshalAs(UnmanagedType.Interface)]
		IPropertyStore OpenPropertyStore(STGM stgmAccess);

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetId();

		DeviceState GetState();
	}
}
