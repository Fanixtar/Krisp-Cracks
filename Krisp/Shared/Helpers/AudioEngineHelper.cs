using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Shared.Interops;
using Shared.Interops.Extensions;
using Shared.Interops.IMMDeviceAPI;

namespace Shared.Helpers
{
	public static class AudioEngineHelper
	{
		public static bool GetDeviceSampleRate(string devID, out WaveFormatExtensible wfex)
		{
			bool flag;
			try
			{
				IPropertyStore propertyStore = ((IMMDeviceEnumerator)new MMDeviceEnumerator()).GetDevice(devID).OpenPropertyStore(STGM.DIRECT);
				wfex = propertyStore.GetValue(PropertyKeys.PKEY_AudioEngine_DeviceFormat);
				flag = true;
			}
			catch
			{
				wfex = default(WaveFormatExtensible);
				flag = false;
			}
			return flag;
		}

		public static bool GetDeviceSampleRate(IMMDevice device, out WaveFormatExtensible wfex)
		{
			bool flag;
			try
			{
				IPropertyStore propertyStore = device.OpenPropertyStore(STGM.DIRECT);
				wfex = propertyStore.GetValue(PropertyKeys.PKEY_AudioEngine_DeviceFormat);
				flag = true;
			}
			catch
			{
				wfex = default(WaveFormatExtensible);
				flag = false;
			}
			return flag;
		}

		public static string ToFormatedString(this WaveFormatExtensible wfex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				stringBuilder.AppendFormat("fmTag:{0},ch:{1},SPS:{2},avgBPS:{3},blckAlgn:{4},BPS:{5},cbSz:{6},vldBPS:{7},chMsk:{8}", new object[] { wfex.wFormatTag, wfex.nChannels, wfex.nSamplesPerSec, wfex.nAvgBytesPerSec, wfex.nBlockAlign, wfex.wBitsPerSample, wfex.cbSize, wfex.wValidBitsPerSample, wfex.dwChannelMask });
			}
			catch
			{
			}
			return stringBuilder.ToString();
		}

		public static AudioEngineHelper.DuckingMode GetDuckingMode()
		{
			AudioEngineHelper.DuckingMode duckingMode;
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Multimedia\\Audio");
				duckingMode = (AudioEngineHelper.DuckingMode)((((registryKey != null) ? registryKey.GetValue("UserDuckingPreference", -1) : null) as int?) ?? (-1));
			}
			catch
			{
				duckingMode = AudioEngineHelper.DuckingMode.Unknown;
			}
			return duckingMode;
		}

		public static bool SetDuckingMode(AudioEngineHelper.DuckingMode mode)
		{
			bool flag;
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Multimedia\\Audio", true);
				if (registryKey != null)
				{
					registryKey.SetValue("UserDuckingPreference", (int)mode);
				}
				flag = AudioEngineHelper.GetDuckingMode() == mode;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		public static bool IsDuckingDisabled()
		{
			return AudioEngineHelper.GetDuckingMode() == AudioEngineHelper.DuckingMode.Do_nothing;
		}

		public static string MicrophoneAccessPrivacySettings()
		{
			string text = "Error";
			try
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\microphone", false))
				{
					text = (((registryKey != null) ? registryKey.GetValue("Value", "Error") : null) as string) ?? "Error";
				}
			}
			catch
			{
			}
			return text;
		}

		public static bool MicrophoneAccessPerApplication(out Dictionary<string, string> appList)
		{
			bool flag = true;
			appList = null;
			string text = "";
			string text2 = WindowsIdentity.GetCurrent().Name.ToString();
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI\\SessionData", false))
			{
				if (registryKey != null)
				{
					foreach (string text3 in registryKey.GetSubKeyNames())
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(text3, false))
						{
							foreach (string text4 in registryKey2.GetValueNames())
							{
								if (string.Compare(text4, "LoggedOnSAMUser", true) == 0)
								{
									object value = registryKey2.GetValue(text4);
									if (string.Compare((value != null) ? value.ToString() : null, text2, true) == 0)
									{
										object value2 = registryKey2.GetValue("LoggedOnUserSID");
										text = ((value2 != null) ? value2.ToString() : null);
									}
								}
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				using (RegistryKey registryKey3 = Registry.Users.OpenSubKey(text + "\\Software\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\microphone", false))
				{
					appList = new Dictionary<string, string>();
					foreach (string text5 in registryKey3.GetSubKeyNames())
					{
						using (RegistryKey registryKey4 = registryKey3.OpenSubKey(text5, false))
						{
							string text6 = (((registryKey4 != null) ? registryKey4.GetValue("Value", "Error") : null) as string) ?? "Error";
							appList.Add(text5, text6);
						}
					}
				}
			}
			return flag;
		}

		public static string RegPathToHidPath(string regPath)
		{
			return string.Format("\\\\?\\{0}#{{4d1e55b2-f16f-11cf-88cb-001111000030}}", regPath.ToLower().Replace('\\', '#'));
		}

		public static List<string> GetDevicesByContainerID(Guid containerID)
		{
			List<string> list = new List<string>();
			try
			{
				string text = string.Format("SYSTEM\\CurrentControlSet\\Control\\DeviceContainers\\{{{0}}}\\BaseContainers\\{{{1}}}", containerID, containerID);
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text, false))
				{
					string[] valueNames = registryKey.GetValueNames();
					list.AddRange(valueNames);
				}
			}
			catch
			{
			}
			return list;
		}

		private static string fromRegValueToId(string value)
		{
			string text = "";
			if (!string.IsNullOrWhiteSpace(value))
			{
				string[] array = value.Split(new char[] { '#' });
				if (array.Length >= 4)
				{
					text = array[2];
				}
			}
			return text;
		}

		public static bool GetAppSpecificDefaultDevices(out Dictionary<string, List<string>> appDefs)
		{
			appDefs = new Dictionary<string, List<string>>();
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(AudioEngineHelper.REGKEY_APPSPECIFICDEFAULTDEVICES, false))
			{
				string[] subKeyNames = registryKey.GetSubKeyNames();
				if (subKeyNames != null)
				{
					foreach (string text in subKeyNames)
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(text))
						{
							string text2 = (string)registryKey2.GetValue("");
							appDefs[text2] = new List<string>();
							if (text2 != null)
							{
								string text3 = (string)registryKey2.GetValue("000_000");
								if (text3 != null)
								{
									appDefs[text2].Add(AudioEngineHelper.fromRegValueToId(text3));
								}
								text3 = (string)registryKey2.GetValue("000_001");
								if (text3 != null)
								{
									appDefs[text2].Add(AudioEngineHelper.fromRegValueToId(text3));
								}
								text3 = (string)registryKey2.GetValue("001_000");
								if (text3 != null)
								{
									appDefs[text2].Add(AudioEngineHelper.fromRegValueToId(text3));
								}
								text3 = (string)registryKey2.GetValue("001_001");
								if (text3 != null)
								{
									appDefs[text2].Add(AudioEngineHelper.fromRegValueToId(text3));
								}
							}
						}
					}
				}
			}
			return true;
		}

		public static bool LaunchSystemSoundSettings(int tabIndex)
		{
			bool flag;
			try
			{
				Process.Start("control.exe", "mmsys.cpl,," + tabIndex);
				flag = true;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		private static string REGKEY_APPSPECIFICDEFAULTDEVICES = "Software\\Microsoft\\Multimedia\\Audio\\DefaultEndpoint";

		public enum DuckingMode
		{
			Unknown = -1,
			Mute_all_other_sounds,
			Reduce_the_volume_by_80,
			Reduce_the_volume_by_50,
			Do_nothing
		}
	}
}
