using System;
using System.Runtime.InteropServices;
using Shared.Interops;

namespace Krisp.Models
{
	public class SPMessage
	{
		public SPMessage(uint sesID, SPNotificationType nCode, HRESULT res, string msg)
		{
			this.sesId = sesID;
			this.notifyCode = nCode;
			this.hr = res;
			this.Message = msg;
		}

		public uint sesId;

		public SPNotificationType notifyCode;

		public HRESULT hr;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Message;
	}
}
