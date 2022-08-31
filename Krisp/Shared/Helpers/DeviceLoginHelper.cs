using System;
using System.IO;

namespace Shared.Helpers
{
	public static class DeviceLoginHelper
	{
		public static bool DeviceMode
		{
			get
			{
				object lockObj = DeviceLoginHelper._lockObj;
				lock (lockObj)
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

		private static readonly string TEAM_KEY_FILE_NAME = "team_secret.key";

		private static readonly string TeamKeyPath = Path.Combine(EnvHelper.KrispFolder, DeviceLoginHelper.TEAM_KEY_FILE_NAME);

		private static bool? _devicemode = null;

		private static string _teamKey = "";

		private static readonly object _lockObj = new object();
	}
}
