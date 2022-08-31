using System;
using System.Configuration;
using System.IO;

namespace Shared.Helpers
{
	public sealed class AppLocalCache
	{
		public static AppLocalCache Instance
		{
			get
			{
				return AppLocalCache.lazy.Value;
			}
		}

		public static string CacheFilePath
		{
			get
			{
				return AppLocalCache.ConfigPath;
			}
		}

		public static string LastError
		{
			get
			{
				string lastError = AppLocalCache._lastError;
				AppLocalCache._lastError = "";
				return lastError;
			}
		}

		public static void ResetCacheFile()
		{
			if (File.Exists(AppLocalCache.ConfigPath))
			{
				File.Delete(AppLocalCache.ConfigPath);
			}
		}

		private AppLocalCache()
		{
			AppLocalCache.checkForCacheMigration();
			this._configuration = this.tryToLoadConfig(new ExeConfigurationFileMap
			{
				ExeConfigFilename = AppLocalCache.ConfigPath
			});
		}

		private Configuration tryToLoadConfig(ExeConfigurationFileMap fileMap)
		{
			Configuration configuration;
			try
			{
				configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None, false);
			}
			catch (ConfigurationException ex)
			{
				AppLocalCache._lastError += string.Format("tryToLoadConfig: {0}\n", ex.Message);
				AppLocalCache.ResetCacheFile();
				configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None, false);
			}
			return configuration;
		}

		public void Set(string key, string val)
		{
			object obj = AppLocalCache.lockObj;
			lock (obj)
			{
				if (this._configuration.AppSettings.Settings[key] == null)
				{
					this._configuration.AppSettings.Settings.Add(key, val);
				}
				else
				{
					this._configuration.AppSettings.Settings[key].Value = val;
				}
				try
				{
					this._configuration.Save(ConfigurationSaveMode.Modified);
				}
				catch (Exception ex)
				{
					AppLocalCache._lastError += string.Format("Set: {0}\n", ex.Message);
				}
			}
		}

		public string Get(string key)
		{
			object obj = AppLocalCache.lockObj;
			string text;
			lock (obj)
			{
				KeyValueConfigurationElement keyValueConfigurationElement = this._configuration.AppSettings.Settings[key];
				text = ((keyValueConfigurationElement != null) ? keyValueConfigurationElement.Value : null);
			}
			return text;
		}

		public static int checkForCacheMigration()
		{
			int num = -1;
			try
			{
				object obj = AppLocalCache.lockObj;
				lock (obj)
				{
					string text = EnvHelper.TransFormToOldFilePath(AppLocalCache.CacheFilePath);
					if (File.Exists(text))
					{
						num = 0;
						if (!File.Exists(AppLocalCache.CacheFilePath))
						{
							if (!Directory.Exists(EnvHelper.KrispAppLocalFolder))
							{
								Directory.CreateDirectory(EnvHelper.KrispAppLocalFolder);
							}
							File.Copy(text, AppLocalCache.CacheFilePath);
							num = 1;
						}
					}
				}
			}
			catch
			{
				num = -1;
			}
			return num;
		}

		private static readonly string DEFAULT_CACHE_FILE_NAME = (RunModeChecker.IsProduction ? "app.cache" : (RunModeChecker.Mode.ShortName() + ".app.cache"));

		private static readonly Lazy<AppLocalCache> lazy = new Lazy<AppLocalCache>(() => new AppLocalCache());

		private static readonly string ConfigPath = Path.Combine(EnvHelper.KrispAppLocalFolder, AppLocalCache.DEFAULT_CACHE_FILE_NAME);

		private static readonly object lockObj = new object();

		private Configuration _configuration;

		private static string _lastError = "";
	}
}
