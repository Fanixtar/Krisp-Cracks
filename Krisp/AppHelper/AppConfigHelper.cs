using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Krisp.Properties;
using Shared.Helpers;

namespace Krisp.AppHelper
{
	public static class AppConfigHelper
	{
		public static uint GetConfigUIntValue(string cfgKey, uint defVal)
		{
			uint num;
			try
			{
				string text = ConfigurationManager.AppSettings[cfgKey];
				uint num2;
				num = (uint.TryParse((text != null) ? text.ToString() : null, out num2) ? num2 : defVal);
			}
			catch
			{
				num = defVal;
			}
			return num;
		}

		public static string GetConfigStringValue(string cfgKey, string defVal)
		{
			string text3;
			try
			{
				string text = ConfigurationManager.AppSettings[cfgKey];
				string text2 = ((text != null) ? text.ToString() : null);
				text3 = (string.IsNullOrEmpty(text2) ? defVal : text2);
			}
			catch
			{
				text3 = defVal;
			}
			return text3;
		}

		public static void UpgradeConfig()
		{
			bool flag = false;
			try
			{
				flag = Settings.Default.UpgradeRequired;
				if (flag)
				{
					int num = AppConfigHelper.checkForMigration();
					if (num != -1)
					{
						AppConfigHelper.deleteOldFolders();
					}
					Settings.Default.Upgrade();
					Settings.Default.UpgradeRequired = false;
					if (num != -1)
					{
						Settings.Default.DemoMode = false;
					}
					Settings.Default.Save();
				}
			}
			catch (ConfigurationErrorsException ex)
			{
				string text = ex.Filename;
				if (ex.Filename == null && ex.InnerException != null)
				{
					text = ((ConfigurationException)ex.InnerException).Filename;
				}
				if (!string.IsNullOrEmpty(text) && File.Exists(text))
				{
					File.Delete(text);
					if (flag)
					{
						Settings.Default.Reset();
						Settings.Default.UpgradeRequired = false;
						Settings.Default.Save();
						return;
					}
				}
				throw ex;
			}
		}

		private static int checkForMigration()
		{
			int num = AppLocalCache.checkForCacheMigration();
			string text = null;
			string text2 = "Krisp.exe_Url*";
			string text3 = "user.config";
			try
			{
				if (Directory.Exists(EnvHelper.KrispLocalFolder))
				{
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(EnvHelper.KrispAppLocalFolder, text2);
					if (enumerable.Count<string>() == 0)
					{
						Settings.Default.UpgradeRequired = true;
						Settings.Default.Save();
						enumerable = Directory.EnumerateDirectories(EnvHelper.KrispAppLocalFolder, text2);
					}
					if (enumerable.Count<string>() != 1)
					{
						return num;
					}
					text = enumerable.FirstOrDefault<string>();
				}
				if (Directory.Exists(EnvHelper.TwoHzLocalFolder) && !string.IsNullOrEmpty(text))
				{
					FileInfo fileInfo = (from sf in new DirectoryInfo(EnvHelper.TwoHzLocalFolder).EnumerateFiles(text3, SearchOption.AllDirectories)
						orderby sf.LastAccessTime descending
						select sf).FirstOrDefault<FileInfo>();
					if (fileInfo != null)
					{
						string fileName = Path.GetFileName(fileInfo.DirectoryName);
						Version version;
						if (Version.TryParse(fileName, out version))
						{
							text = Path.Combine(text, fileName);
							if (!Directory.Exists(text) && File.Exists(fileInfo.FullName))
							{
								Directory.CreateDirectory(text);
								File.Copy(fileInfo.FullName, Path.Combine(text, text3), true);
							}
						}
					}
				}
			}
			catch
			{
			}
			return num;
		}

		private static bool deleteOldFolders()
		{
			try
			{
				string text = EnvHelper.TransFormToOldFilePath(EnvHelper.KrispAppLogFolder);
				if (Directory.Exists(text))
				{
					string[] files = Directory.GetFiles(text);
					if (!Directory.Exists(EnvHelper.KrispAppLogFolder))
					{
						Directory.CreateDirectory(EnvHelper.KrispAppLogFolder);
					}
					foreach (string text2 in files)
					{
						File.Copy(text2, Path.Combine(EnvHelper.KrispAppLogFolder, Path.GetFileName(text2)), true);
					}
					Directory.Delete(EnvHelper.TwoHzLocalFolder, true);
				}
			}
			catch
			{
			}
			return true;
		}
	}
}
