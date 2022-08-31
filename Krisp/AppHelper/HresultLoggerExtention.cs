using System;
using Shared.Interops;

namespace Krisp.AppHelper
{
	public static class HresultLoggerExtention
	{
		public static HRESULT LogOnHerror(this HRESULT hr, Logger logger, string s)
		{
			if (hr.Failed)
			{
				logger.LogError("{0} : {1}", new object[] { s, hr });
			}
			return hr;
		}

		public static HRESULT LogOnNotSuccess(this HRESULT hr, Logger logger, string s)
		{
			if (!hr)
			{
				logger.LogWarning("{0} : {1}", new object[] { s, hr });
			}
			return hr;
		}

		public static bool LogDebugOnFalse(this bool b, Logger logger, string s)
		{
			if (!b)
			{
				logger.LogDebug("{0}", new object[] { s });
			}
			return b;
		}
	}
}
