using System;
using System.IO;
using Krisp.AppHelper;
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

		private static bool GetKrispDeviceID(out string devID)
		{
			devID = "";
			try
			{
				devID = KrispDriverHelper.GetKrispDeviceInterface();
			}
			catch (Exception ex)
			{
				LogWrapper.GetLogger("Core").LogError(ex.Message);
				return false;
			}
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
