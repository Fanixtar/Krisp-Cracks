using System;
using System.Linq;

namespace Shared.Helpers
{
	public static class InstallationID
	{
		public static string HardwareIdentifier
		{
			get
			{
				return SystemInfo.SysDriveModel + SystemInfo.SysDriveSerial + SystemInfo.MBManufacturer + SystemInfo.MBSerial;
			}
		}

		public static string HWID
		{
			get
			{
				string text = null;
				try
				{
					text = CryptoHelper.ComputeSha256(InstallationID.HardwareIdentifier, InstallationID.HW_ID_BYTES_COUNT);
				}
				catch
				{
				}
				finally
				{
					if (string.IsNullOrEmpty(text))
					{
						text = new string(Enumerable.Repeat<char>('0', InstallationID.HW_ID_BYTES_COUNT * 2).ToArray<char>());
					}
				}
				return text;
			}
		}

		private static bool IsDeviceInstId(string instId)
		{
			return instId.StartsWith(InstallationID.devicePrefix);
		}

		private static string InsID
		{
			get
			{
				string text = null;
				try
				{
					text = AppLocalCache.Instance.Get(InstallationID.INST_ID_CFG_KEY_NAME);
					if (DeviceLoginHelper.DeviceMode)
					{
						string hardwareIdentifier = InstallationID.HardwareIdentifier;
						AppLocalCache.Instance.Set(InstallationID.INST_ID_CFG_KEY_NAME, InstallationID.devicePrefix + CryptoHelper.ComputeSha256(hardwareIdentifier, InstallationID.KIID_BYTES_COUNT - InstallationID.devicePrefix.Length / 2));
						text = AppLocalCache.Instance.Get(InstallationID.INST_ID_CFG_KEY_NAME);
					}
					else if (string.IsNullOrWhiteSpace(text) || InstallationID.IsDeviceInstId(text))
					{
						AppLocalCache.Instance.Set(InstallationID.INST_ID_CFG_KEY_NAME, CryptoHelper.ComputeSha256(Guid.NewGuid().ToString(), InstallationID.KIID_BYTES_COUNT));
						text = AppLocalCache.Instance.Get(InstallationID.INST_ID_CFG_KEY_NAME);
					}
				}
				catch (Exception ex)
				{
					AppLocalCache.ResetCacheFile();
					throw new Exception("Failed to fetch installation ID from cache: " + ex.Message);
				}
				if (string.IsNullOrWhiteSpace(text))
				{
					throw new Exception("Unable to prepare installation ID");
				}
				return text;
			}
		}

		public static string ID
		{
			get
			{
				if (string.IsNullOrEmpty(InstallationID.instID))
				{
					object obj = InstallationID.lockobj;
					lock (obj)
					{
						if (string.IsNullOrEmpty(InstallationID.instID))
						{
							InstallationID.instID = InstallationID.HWID + "-" + InstallationID.InsID;
						}
					}
				}
				return InstallationID.instID;
			}
		}

		private static int HW_ID_BYTES_COUNT = 2;

		private static int KIID_BYTES_COUNT = 16;

		private static readonly string INST_ID_CFG_KEY_NAME = "KIID";

		private static object lockobj = new object();

		private static string instID;

		private static string devicePrefix = "DEVICE";
	}
}
