using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	public static class CfgMgr32
	{
		[DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
		public static extern uint CM_Get_Device_Interface_List_Size(out uint size, ref Guid interfaceClassGuid, string deviceID, uint flags);

		[DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
		public static extern uint CM_Get_Device_Interface_List(ref Guid interfaceClassGuid, string deviceID, char[] buffer, uint bufferLength, uint flags);

		public const uint CM_GET_DEVICE_INTERFACE_LIST_PRESENT = 0U;

		public const uint CM_GET_DEVICE_INTERFACE_LIST_ALL_DEVICES = 1U;
	}
}
