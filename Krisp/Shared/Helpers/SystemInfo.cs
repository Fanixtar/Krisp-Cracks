using System;
using System.Globalization;
using System.Linq;
using System.Management;
using Microsoft.Win32;

namespace Shared.Helpers
{
	public static class SystemInfo
	{
		private static string GetFirstMgmtObjPropertyValue(string cl, string prop)
		{
			string text;
			try
			{
				text = new ManagementObjectSearcher("select * from " + cl).Get().Cast<ManagementObject>().First<ManagementObject>()[prop].ToString();
			}
			catch
			{
				text = "";
			}
			return text;
		}

		private static int GetSystemDriveIndex()
		{
			try
			{
				string firstMgmtObjPropertyValue = SystemInfo.GetFirstMgmtObjPropertyValue("Win32_OperatingSystem", "SystemDrive");
				foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get())
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					foreach (ManagementBaseObject managementBaseObject2 in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + managementObject.Properties["DeviceID"].Value + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
					{
						ManagementObject managementObject2 = (ManagementObject)managementBaseObject2;
						foreach (ManagementBaseObject managementBaseObject3 in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + managementObject2["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
						{
							ManagementObject managementObject3 = (ManagementObject)managementBaseObject3;
							if (firstMgmtObjPropertyValue == managementObject3["Name"].ToString())
							{
								return Convert.ToInt32(managementObject.GetPropertyValue("Index"));
							}
						}
					}
				}
			}
			catch
			{
			}
			return -1;
		}

		public static string Timezone
		{
			get
			{
				return TimeZoneInfo.Local.DisplayName;
			}
		}

		public static string Locale
		{
			get
			{
				return CultureInfo.CurrentCulture.Name;
			}
		}

		public static string HostName
		{
			get
			{
				return Environment.MachineName;
			}
		}

		public static string OSName
		{
			get
			{
				return "win";
			}
		}

		public static string OsBitness
		{
			get
			{
				if (!Environment.Is64BitProcess)
				{
					return "x86";
				}
				return "x64";
			}
		}

		public static string WinVersion
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_OperatingSystem", "Version");
			}
		}

		public static string WinCaption
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_OperatingSystem", "Caption");
			}
		}

		public static string WinReleaseID
		{
			get
			{
				string text;
				try
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
					text = ((registryKey != null) ? registryKey.GetValue("ReleaseId") : null) as string;
				}
				catch
				{
					text = "";
				}
				return text;
			}
		}

		public static string CompName
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_ComputerSystemProduct", "Name");
			}
		}

		public static string CompUUID
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_ComputerSystemProduct", "UUID");
			}
		}

		public static string CPUModel
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_Processor", "Name");
			}
		}

		public static string GPUModel
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_VideoController", "Name");
			}
		}

		public static string SysDriveSerial
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_DiskDrive where index=" + SystemInfo.GetSystemDriveIndex().ToString(), "SerialNumber");
			}
		}

		public static string SysDriveModel
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_DiskDrive where index=" + SystemInfo.GetSystemDriveIndex().ToString(), "Model");
			}
		}

		public static string MBSerial
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_BaseBoard", "SerialNumber");
			}
		}

		public static string MBManufacturer
		{
			get
			{
				return SystemInfo.GetFirstMgmtObjPropertyValue("Win32_BaseBoard", "Manufacturer");
			}
		}
	}
}
