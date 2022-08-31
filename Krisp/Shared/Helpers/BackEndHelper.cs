using System;

namespace Shared.Helpers
{
	public static class BackEndHelper
	{
		public static string GetHTTPUserAgent()
		{
			return string.Concat(new string[]
			{
				"krisp ",
				EnvHelper.KrispVersion.ToString(),
				" (",
				Environment.OSVersion.VersionString,
				")"
			});
		}

		public static readonly int ANALYTIC_REQUEST_TIMEOUT = 100000;

		public static readonly int MSI_ANALYTIC_REQUEST_TIMEOUT = 6000;

		public static readonly int API_REQUEST_TIMEOUT = 30000;
	}
}
