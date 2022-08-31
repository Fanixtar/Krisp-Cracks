using System;
using System.IO;
using Krisp.AppHelper;
using Shared.Helpers;

namespace Krisp.Analytics
{
	public sealed class AnalyticsFactory
	{
		public static IKrispAnalytics Instance
		{
			get
			{
				if (AnalyticsFactory.krispAnalytics == null)
				{
					object obj = AnalyticsFactory.lockobj;
					lock (obj)
					{
						if (AnalyticsFactory.krispAnalytics == null)
						{
							string text = Path.Combine(EnvHelper.KrispAppLocalFolder, AnalyticsFactory.DEFAULT_CACHE_FILE_NAME);
							string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(AppConfigHelper.GetConfigStringValue("Krisp.Analytics.Cache", text)));
							ServerInfo analyticInfo = ServerInfoLoader.Instance.AnalyticInfo;
							AnalyticsFactory.krispAnalytics = new AnalyticsManager(new AnalyticsManager.AnalyticsConfig
							{
								rest_url = analyticInfo.url,
								stoken = analyticInfo.stoken,
								cache_file = fullPath,
								interval = AppConfigHelper.GetConfigUIntValue("Krisp.Analytics.Interval", AnalyticsFactory.DEFAULT_INTERVAL),
								batchCount = AppConfigHelper.GetConfigUIntValue("Krisp.Analytics.BatchCount", AnalyticsFactory.DEFAULT_BATCH_COUNT),
								maxCacheCount = AppConfigHelper.GetConfigUIntValue("Krisp.Analytics.MaxCacheCount", AnalyticsFactory.DEFAULT_MAX_CACHE_COUNT)
							});
						}
					}
				}
				return AnalyticsFactory.krispAnalytics;
			}
		}

		private static readonly uint DEFAULT_INTERVAL = 360U;

		private static readonly uint DEFAULT_BATCH_COUNT = 100U;

		private static readonly uint DEFAULT_MAX_CACHE_COUNT = 1000U;

		private static readonly string DEFAULT_CACHE_FILE_NAME = "iqf847t4v.tmp";

		private static IKrispAnalytics krispAnalytics = null;

		private static readonly object lockobj = new object();
	}
}
