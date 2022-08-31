using System;

namespace Shared.Interops.IMMDeviceAPI
{
	public static class IMMDeviceExtensions
	{
		public static HRESULT Activate<T>(this IMMDevice device, out T res)
		{
			Guid guid = typeof(T).GUID;
			object obj;
			HRESULT hresult = device.Activate(ref guid, 1U, IntPtr.Zero, out obj);
			res = (T)((object)obj);
			return hresult;
		}
	}
}
