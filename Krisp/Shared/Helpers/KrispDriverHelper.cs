using System;
using Shared.Interops;

namespace Shared.Helpers
{
	public static class KrispDriverHelper
	{
		public static string GetKrispDeviceInterface()
		{
			Guid guid = Guid.Parse(KrispDevicePublic.GUID_DEVINTF_KRISP);
			uint num = 0U;
			uint num2 = CfgMgr32.CM_Get_Device_Interface_List_Size(out num, ref guid, null, 0U);
			if (num2 != 0U || num <= 1U)
			{
				throw new Exception(string.Format("Unable to find Krisp device[GetSize]. Error code:{0}", num2));
			}
			char[] array = new char[num];
			num2 = CfgMgr32.CM_Get_Device_Interface_List(ref guid, null, array, num, 0U);
			if (num2 != 0U || num <= 1U)
			{
				throw new Exception(string.Format("Unable to find Krisp device[GetList]. Error code:{0}", num2));
			}
			return new string(array);
		}
	}
}
