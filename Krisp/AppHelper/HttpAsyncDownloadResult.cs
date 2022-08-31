using System;
using Shared.Interops;

namespace Krisp.AppHelper
{
	public class HttpAsyncDownloadResult
	{
		public HttpAsyncDownloadResult(Exception ex)
		{
			this.HResult = ex.HResult;
			this.Message = ex.Message;
		}

		public HttpAsyncDownloadResult(HRESULT hr)
		{
			this.HResult = hr;
		}

		public HRESULT HResult = 1;

		public string Message = "";
	}
}
