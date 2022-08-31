﻿using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMMDeviceCollection
	{
		uint GetCount();

		[return: MarshalAs(UnmanagedType.Interface)]
		IMMDevice Item(uint nDevice);
	}
}
