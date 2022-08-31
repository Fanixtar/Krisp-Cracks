using System;
using System.IO;
using Newtonsoft.Json;

namespace Shared.Helpers
{
	public static class DeviceLoginHelper
	{
		public static bool DeviceMode
		{
			get
			{
				object licenseFileLock = DeviceLoginHelper._licenseFileLock;
				lock (licenseFileLock)
				{
					if (DeviceLoginHelper._devicemode == null)
					{
						FileInfo fileInfo = new FileInfo(DeviceLoginHelper.TeamKeyPath);
						DeviceLoginHelper._devicemode = new bool?(fileInfo.Exists && fileInfo.Length != 0L);
					}
				}
				return DeviceLoginHelper._devicemode.Value;
			}
		}

		public static string TeamKey
		{
			get
			{
				string teamKey;
				try
				{
					if (File.Exists(DeviceLoginHelper.TeamKeyPath) && string.IsNullOrEmpty(DeviceLoginHelper._teamKey))
					{
						DeviceLoginHelper._teamKey = File.ReadAllText(DeviceLoginHelper.TeamKeyPath).Trim();
					}
					teamKey = DeviceLoginHelper._teamKey;
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to read team key from file", ex);
				}
				return teamKey;
			}
		}

		public static void ReloadKey()
		{
			try
			{
				if (File.Exists(DeviceLoginHelper.TeamKeyPath))
				{
					DeviceLoginHelper._teamKey = File.ReadAllText(DeviceLoginHelper.TeamKeyPath).Trim();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to reload team key from file", ex);
			}
		}

		public static void ReloadConfig()
		{
			try
			{
				if (File.Exists(DeviceLoginHelper.KeyConfigPath))
				{
					DeviceKeyConfig deviceKeyConfig = JsonConvert.DeserializeObject<DeviceKeyConfig>(File.ReadAllText(DeviceLoginHelper.KeyConfigPath).Trim());
					DeviceLoginHelper._FqdnBased = new bool?(deviceKeyConfig.installation_id_mech == "fqdn");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to reload key config from file", ex);
			}
		}

		public static bool FQDNBasedInstallID
		{
			get
			{
				try
				{
					if (DeviceLoginHelper.DeviceMode)
					{
						object configFileLock = DeviceLoginHelper._configFileLock;
						lock (configFileLock)
						{
							if (DeviceLoginHelper._FqdnBased == null)
							{
								FileInfo fileInfo = new FileInfo(DeviceLoginHelper.KeyConfigPath);
								if (fileInfo.Exists && fileInfo.Length != 0L)
								{
									DeviceKeyConfig deviceKeyConfig = JsonConvert.DeserializeObject<DeviceKeyConfig>(File.ReadAllText(DeviceLoginHelper.KeyConfigPath).Trim());
									DeviceLoginHelper._FqdnBased = new bool?(deviceKeyConfig.installation_id_mech == "fqdn");
								}
								else
								{
									DeviceLoginHelper._FqdnBased = new bool?(false);
								}
							}
						}
						return DeviceLoginHelper._FqdnBased.Value;
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to read device-based login key.config file", ex);
				}
				return false;
			}
		}

		private static readonly string TEAM_KEY_FILE_NAME = "team_secret.key";

		private static readonly string KEY_CONFIG_NAME = "key.config";

		private static readonly string TeamKeyPath = Path.Combine(EnvHelper.KrispFolder, DeviceLoginHelper.TEAM_KEY_FILE_NAME);

		private static readonly string KeyConfigPath = Path.Combine(EnvHelper.KrispFolder, DeviceLoginHelper.KEY_CONFIG_NAME);

		private static bool? _devicemode = null;

		private static bool? _FqdnBased = null;

		private static string _teamKey = "";

		private static readonly object _licenseFileLock = new object();

		private static readonly object _configFileLock = new object();

		private const string fqdn = "fqdn";
	}
}
