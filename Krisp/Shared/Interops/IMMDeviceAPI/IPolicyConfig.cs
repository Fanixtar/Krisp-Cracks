using System;
using System.Runtime.InteropServices;

namespace Shared.Interops.IMMDeviceAPI
{
	[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPolicyConfig
	{
		void Unused1();

		void Unused2();

		void Unused3();

		void Unused4();

		void Unused5();

		void Unused6();

		void Unused7();

		void Unused8();

		int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ref PROPERTYKEY pkey, ref PropVariant pv);

		int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ref PROPERTYKEY pkey, ref PropVariant pv);

		int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ERole eRole);

		int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, [MarshalAs(UnmanagedType.I2)] short isVisible);
	}
}
