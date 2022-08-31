using System;
using System.Configuration;
using System.IO;
using Krisp.Properties;
using Sentry;
using Shared.Helpers;

namespace Krisp.AppHelper
{
	public static class GarbageCleaner
	{
		public static bool LocalConfigChangedAtInstall { get; private set; } = false;

		public static void Clean()
		{
			try
			{
				GarbageCleaner.CleanTestKrisp();
				GarbageCleaner.CleanAtInstall();
			}
			catch (Exception ex)
			{
				GarbageCleaner._logger.LogError("Garbage cleaner failed. Exception: {0}", new object[] { ex.Message });
				SentrySdk.CaptureException(ex);
			}
		}

		private static void CleanTestKrisp()
		{
			string testKrispRecordingsFolder = EnvHelper.TestKrispRecordingsFolder;
			if (Directory.Exists(testKrispRecordingsFolder))
			{
				try
				{
					Directory.Delete(testKrispRecordingsFolder, true);
				}
				catch (Exception ex)
				{
					GarbageCleaner._logger.LogError("Failed to clean after TestKrisp. Exception: {0}", new object[] { ex.Message });
				}
			}
		}

		private static void CleanAtInstall()
		{
			KeyValueConfigurationElement keyValueConfigurationElement = EnvHelper.GlobalConfig.AppSettings.Settings["Installation_Guid"];
			string text = ((keyValueConfigurationElement != null) ? keyValueConfigurationElement.Value : null);
			if (string.IsNullOrWhiteSpace(text))
			{
				GarbageCleaner._logger.LogDebug("GlobalInstallationGuid was null");
				return;
			}
			if (Settings.Default.InstallationGuid != text)
			{
				AppLocalCache.Instance.Set("AppToken", null);
				AppLocalCache.Instance.Set("SessionID", null);
				AppLocalCache.Instance.Set("Secret", null);
				AppLocalCache.Instance.Set("InstallationInfoStr", null);
				GarbageCleaner.DeleteLocalConfig();
				Settings.Default.Reset();
				Settings.Default.InstallationGuid = text;
				Settings.Default.Save();
				GarbageCleaner.LocalConfigChangedAtInstall = true;
			}
		}

		private static void DeleteLocalConfig()
		{
			string krispAppLocalFolder = EnvHelper.KrispAppLocalFolder;
			if (Directory.Exists(krispAppLocalFolder))
			{
				try
				{
					DirectoryInfo[] directories = new DirectoryInfo(krispAppLocalFolder).GetDirectories("Krisp.exe*");
					for (int i = 0; i < directories.Length; i++)
					{
						directories[i].Delete(true);
					}
				}
				catch (Exception ex)
				{
					GarbageCleaner._logger.LogError("Failed to delete local config folders at {0}. Exception: {1}", new object[] { krispAppLocalFolder, ex.Message });
				}
			}
		}

		private static readonly Logger _logger = LogWrapper.GetLogger("GarbageCleaner");
	}
}
