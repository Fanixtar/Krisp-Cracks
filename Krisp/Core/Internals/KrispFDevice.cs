using System;
using System.IO;
using Microsoft.Win32.SafeHandles;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class KrispFDevice : DisposableBase
	{
		public KrispFDevice()
		{
			string text;
			if (KrispFDevice.GetKrispDeviceID(out text))
			{
				this.krispDeviceHandle = Kernel32.CreateFile(text, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, (FileAttributes)1073741824, IntPtr.Zero);
			}
		}

		public static bool GetKrispDeviceID(out string devID)
		{
			Guid guid = Guid.Parse(KrispDevicePublic.GUID_DEVINTF_KRISP);
			uint num = 0U;
			devID = "";
			if (CfgMgr32.CM_Get_Device_Interface_List_Size(out num, ref guid, null, 0U) != 0U)
			{
				return false;
			}
			char[] array = new char[num + 1U];
			if (CfgMgr32.CM_Get_Device_Interface_List(ref guid, null, array, num, 0U) != 0U)
			{
				return false;
			}
			devID = new string(array);
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (this.krispDeviceHandle != null && !this.krispDeviceHandle.IsInvalid && !this.krispDeviceHandle.IsClosed)
			{
				this.krispDeviceHandle.Close();
			}
			this.krispDeviceHandle.Dispose();
			this.krispDeviceHandle = null;
			this._disposed = true;
			base.Dispose(disposing);
		}

		protected SafeFileHandle krispDeviceHandle;

		private bool _disposed;
	}
}
