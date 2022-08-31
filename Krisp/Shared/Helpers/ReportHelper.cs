using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;
using Shared.Interops;
using Shared.Interops.Extensions;
using Shared.Interops.IMMDeviceAPI;

namespace Shared.Helpers
{
	public static class ReportHelper
	{
		public static void dumpInstalledAudioDevices(StringBuilder sb)
		{
			sb.AppendLine("Begin dump_AudioDevices.");
			try
			{
				IMMDeviceCollection immdeviceCollection = ((IMMDeviceEnumerator)new MMDeviceEnumerator()).EnumAudioEndpoints(EDataFlow.eAll, DeviceState.MASK_ALL);
				for (uint num = 0U; num < immdeviceCollection.GetCount(); num += 1U)
				{
					try
					{
						IMMDevice immdevice = immdeviceCollection.Item(num);
						sb.AppendLine(immdevice.GetId() ?? "");
						sb.Append("EnumeratorName: ");
						ReportHelper.tryDumpPropValue(sb, immdevice, PropertyKeys.DEVPKEY_Device_EnumeratorName);
						sb.Append("Description: ");
						ReportHelper.tryDumpPropValue(sb, immdevice, PropertyKeys.DEVPKEY_Device_DeviceDesc);
						sb.Append("Name: ");
						ReportHelper.tryDumpPropValue(sb, immdevice, PropertyKeys.PKEY_Device_FriendlyName);
						sb.Append("InterfaceName: ");
						ReportHelper.tryDumpPropValue(sb, immdevice, PropertyKeys.DEVPKEY_DeviceInterface_FriendlyName);
						sb.AppendLine(string.Format("State: {0}", immdevice.GetState()));
					}
					catch (Exception ex)
					{
						sb.AppendLine("---------");
						sb.AppendLine(string.Format("Error AudioDevice: {0}", ex));
						sb.AppendLine("---------");
					}
					sb.AppendLine("=====================");
				}
			}
			catch (Exception ex2)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("dump_AudioDevices: {0}", ex2));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End dump_AudioDevices.");
		}

		private static void tryDumpPropValue(StringBuilder sb, IMMDevice dev, PROPERTYKEY propKey)
		{
			try
			{
				string value = dev.OpenPropertyStore(STGM.DIRECT).GetValue(propKey);
				sb.AppendLine(value ?? "");
			}
			catch (Exception ex) when (ex.HResult == -536870389)
			{
				sb.Append(string.Format("----- Error: 0x{0:X} ----", ex.HResult));
				sb.AppendLine();
			}
			catch (Exception ex2)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("Error DumpPropValue: {0}", ex2));
				sb.AppendLine("---------");
			}
		}

		public static bool IsElevatedUser()
		{
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		}

		public static void dumpInstalledApps(StringBuilder sb)
		{
			List<string> list = null;
			sb.AppendLine("Begin MachineInstalledApps.");
			try
			{
				ReportHelper.GetMachineInstalledApps(out list);
				if (list != null)
				{
					foreach (string text in list)
					{
						sb.AppendLine(text ?? "");
					}
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("MachineInstalledApps: {0}", ex));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End MachineInstalledApps.");
			sb.AppendLine();
			sb.AppendLine("Begin UserInstalledApps.");
			try
			{
				ReportHelper.GetUserInstalledApps(out list);
				if (list != null)
				{
					foreach (string text2 in list)
					{
						sb.AppendLine(text2 ?? "");
					}
				}
			}
			catch (Exception ex2)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("UserInstalledApps: {0}", ex2));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End UserInstalledApps.");
			sb.AppendLine();
			sb.AppendLine("Begin GetUserRegisteredApps.");
			try
			{
				ReportHelper.GetUserRegisteredApps(out list);
				if (list != null)
				{
					foreach (string text3 in list)
					{
						sb.AppendLine(text3 ?? "");
					}
				}
			}
			catch (Exception ex3)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("GetUserRegisteredApps: {0}", ex3));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End GetUserRegisteredApps.");
		}

		public static void dumpMicrophoneAccessPerApplication(StringBuilder sb)
		{
			sb.AppendLine("Begin dumpMicrophoneAccessPerApplication.");
			try
			{
				Dictionary<string, string> dictionary;
				if (AudioEngineHelper.MicrophoneAccessPerApplication(out dictionary) && dictionary != null)
				{
					foreach (KeyValuePair<string, string> keyValuePair in dictionary)
					{
						sb.AppendLine(keyValuePair.Key + ": " + keyValuePair.Value);
					}
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("dumpMicrophoneAccessPerApplication: {0}", ex));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End dumpMicrophoneAccessPerApplication.");
		}

		public static void dumpAppSpecificDefaultsDevices(StringBuilder sb)
		{
			try
			{
				sb.AppendLine("-- dumpAppSpecificDefaultsDevices.");
				Dictionary<string, List<string>> dictionary;
				if (AudioEngineHelper.GetAppSpecificDefaultDevices(out dictionary) && dictionary != null)
				{
					foreach (string text in dictionary.Keys)
					{
						sb.AppendLine("ASDD: " + text);
						if (dictionary[text] != null && dictionary[text].Count > 0)
						{
							using (List<string>.Enumerator enumerator2 = dictionary[text].GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									string text2 = enumerator2.Current;
									sb.AppendLine("\tASDD -> Device:  " + text2);
								}
								continue;
							}
						}
						sb.AppendLine("\t-");
					}
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("dumpAppSpecificDefaultsDevices: {0}", ex));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End of dumpAppSpecificDefaultsDevices");
		}

		public static void dumpEventLog(StringBuilder sb, EventLogEntryType entryType, string logname, DateTime oldest, string keyword = null)
		{
			try
			{
				sb.AppendLine("LogEntries for " + logname + " log.");
				string text = ((keyword != null) ? keyword.ToLower() : null);
				using (EventLog eventLog = new EventLog(logname))
				{
					EventLogEntryCollection entries = eventLog.Entries;
					for (int i = entries.Count - 1; i >= 0; i--)
					{
						EventLogEntry eventLogEntry = entries[i];
						if (eventLogEntry.TimeGenerated < oldest)
						{
							break;
						}
						if (eventLogEntry.EntryType <= entryType && keyword != null && (eventLogEntry.Message.Contains(keyword) || eventLogEntry.Message.Contains(text)))
						{
							sb.AppendLine("LogEntry.From: " + logname);
							sb.AppendLine(string.Format("LogEntry.EventId: {0}", eventLogEntry.Index));
							sb.AppendLine(string.Format("LogEntry.InstanceId: {0}", eventLogEntry.InstanceId));
							sb.AppendLine("LogEntry.Source: " + eventLogEntry.Source);
							sb.AppendLine(string.Format("LogEntry.EntryType: {0}", eventLogEntry.EntryType));
							sb.AppendLine(string.Format("LogEntry.TimeGenerated: {0}", eventLogEntry.TimeGenerated));
							sb.AppendLine("LogEntry.Category: " + eventLogEntry.Category);
							sb.AppendLine("LogEntry.Message: " + eventLogEntry.Message);
							sb.AppendLine("---------");
						}
					}
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("dumpEventLog: {0}", ex));
				sb.AppendLine("---------");
			}
			sb.AppendLine("End of LogEntries for " + logname + " log.");
		}

		public static void GetMachineInstalledApps(out List<string> lstInstalled)
		{
			lstInstalled = new List<string>();
			string text = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(text, false))
			{
				if (registryKey != null)
				{
					foreach (string text2 in registryKey.GetSubKeyNames())
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(text2))
						{
							string text3 = "";
							try
							{
								text3 = (string)((registryKey2 != null) ? registryKey2.GetValue("DisplayName") : null);
								if (text3 != null)
								{
									text3 = text3 + " - " + (string)registryKey2.GetValue("DisplayVersion");
									lstInstalled.Add(text3);
								}
							}
							catch (Exception ex)
							{
								text3 += string.Format("Error: {0}", ex);
								lstInstalled.Add(text3);
							}
						}
					}
				}
			}
		}

		public static void GetUserInstalledApps(out List<string> lstInstalled)
		{
			lstInstalled = new List<string>();
			string text = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(text, false))
			{
				if (registryKey != null)
				{
					foreach (string text2 in registryKey.GetSubKeyNames())
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(text2))
						{
							string text3 = "";
							try
							{
								text3 = (string)((registryKey2 != null) ? registryKey2.GetValue("DisplayName") : null);
								if (text3 != null)
								{
									text3 = text3 + " - " + (string)registryKey2.GetValue("DisplayVersion");
									lstInstalled.Add(text3);
								}
							}
							catch (Exception ex)
							{
								text3 += string.Format("Error: {0}", ex);
								lstInstalled.Add(text3);
							}
						}
					}
				}
			}
		}

		public static void GetUserRegisteredApps(out List<string> lstInstalled)
		{
			lstInstalled = new List<string>();
			string text = "Software\\RegisteredApplications";
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(text, false))
			{
				if (registryKey != null)
				{
					foreach (string text2 in registryKey.GetValueNames())
					{
						try
						{
							string text3 = (string)((registryKey != null) ? registryKey.GetValue(text2) : null);
							if (!string.IsNullOrWhiteSpace(text3))
							{
								using (Registry.CurrentUser.OpenSubKey(text3, false))
								{
									lstInstalled.Add(text3);
								}
							}
						}
						catch (Exception ex)
						{
							lstInstalled.Add(string.Format("Error: {0}", ex));
						}
					}
				}
			}
		}

		public static void GetInstalledServices(StringBuilder sb)
		{
			sb.AppendLine("+++++ Begin ServicesStatuses. +++++");
			try
			{
				foreach (ServiceController serviceController in ServiceController.GetServices())
				{
					try
					{
						sb.AppendLine("Service.DisplayName: " + serviceController.DisplayName);
						sb.AppendLine("Service.ServiceName: " + serviceController.ServiceName);
						sb.AppendLine(string.Format("Service.Status: {0}", serviceController.Status));
						sb.AppendLine("-------");
					}
					catch (Exception ex)
					{
						sb.AppendLine(string.Format("Error ServicesStatuses {0}", ex));
					}
				}
			}
			catch (Exception ex2)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("ServicesStatuses: {0}", ex2));
				sb.AppendLine("---------");
			}
			sb.AppendLine("------- End of ServicesStatuses. ---------");
			sb.AppendLine("+++++ Begin ServiceDevices. +++++");
			try
			{
				foreach (ServiceController serviceController2 in ServiceController.GetDevices())
				{
					try
					{
						sb.AppendLine("ServiceDevices.DisplayName: " + serviceController2.DisplayName);
						sb.AppendLine("ServiceDevices.ServiceName: " + serviceController2.ServiceName);
						sb.AppendLine(string.Format("ServiceDevices.Status: {0}", serviceController2.Status));
						sb.AppendLine("-------");
					}
					catch (Exception ex3)
					{
						sb.AppendLine(string.Format("Error ServiceDevices {0}", ex3));
					}
				}
			}
			catch (Exception ex4)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("ServiceDevices: {0}", ex4));
				sb.AppendLine("---------");
			}
			sb.AppendLine("------- End of ServiceDevices. ---------");
		}

		public static string ZipDirectory(string dirPath, string zipFile)
		{
			string[] files = Directory.GetFiles(dirPath);
			using (ZipArchive zipArchive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
			{
				foreach (string text in files)
				{
					zipArchive.CreateEntryFromFile(text, Path.GetFileName(text));
				}
			}
			return zipFile;
		}

		public static void CopyLogFiles(string source, string destination)
		{
			try
			{
				if (!Directory.Exists(destination))
				{
					Directory.CreateDirectory(destination);
				}
				foreach (string text in Directory.GetFiles(source))
				{
					if (string.Compare(Path.GetExtension(text), ".txt", true) == 0 || string.Compare(Path.GetExtension(text), ".log", true) == 0)
					{
						string fileName = Path.GetFileName(text);
						string text2 = Path.Combine(destination, fileName);
						File.Copy(text, text2);
					}
				}
			}
			catch
			{
			}
		}

		public static void moveAdditionalFileCopies(List<string> fileList, string destination)
		{
			try
			{
				if (fileList != null)
				{
					if (!Directory.Exists(destination))
					{
						Directory.CreateDirectory(destination);
					}
					foreach (string text in fileList)
					{
						if (File.Exists(text))
						{
							string text2 = Path.Combine(destination, Path.GetFileName(text));
							File.Move(text, text2);
						}
					}
				}
			}
			catch
			{
			}
		}

		public static void CopyCacheAndConfig(string destination)
		{
			try
			{
				if (!Directory.Exists(destination))
				{
					Directory.CreateDirectory(destination);
				}
				string cacheFilePath = AppLocalCache.CacheFilePath;
				if (File.Exists(cacheFilePath))
				{
					string text = Path.Combine(destination, Path.GetFileName(cacheFilePath));
					File.Copy(cacheFilePath, text);
					ReportHelper.MasqueradeConfigFile(text);
				}
				Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
				if (configuration != null && File.Exists(configuration.FilePath))
				{
					string text2 = Path.Combine(destination, Path.GetFileName(configuration.FilePath));
					File.Copy(configuration.FilePath, text2);
				}
			}
			catch
			{
			}
		}

		private static void MasqueradeConfigFile(string destPath)
		{
			Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
			{
				ExeConfigFilename = destPath
			}, ConfigurationUserLevel.None, true);
			try
			{
				KeyValueConfigurationElement keyValueConfigurationElement = configuration.AppSettings.Settings["AppToken"];
				string text = ((keyValueConfigurationElement != null) ? keyValueConfigurationElement.Value : null);
				if (text != null)
				{
					configuration.AppSettings.Settings["AppToken"].Value = text.Substring(0, text.Length - text.Length / 4) + "*****";
					configuration.Save(ConfigurationSaveMode.Modified);
				}
			}
			catch
			{
			}
		}

		public static bool IsAdmin()
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			if (current != null)
			{
				if (new List<Claim>(new WindowsPrincipal(current).UserClaims).Find((Claim p) => p.Value.Contains("S-1-5-32-544")) != null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
