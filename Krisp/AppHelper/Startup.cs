using System;
using System.Windows.Forms;
using Microsoft.Win32;
using Shared.Helpers;

namespace Krisp.AppHelper
{
	public class Startup
	{
		public static bool RunOnStartup()
		{
			try
			{
				Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).SetValue(Application.ProductName, "\"" + EnvHelper.KrispExeFullPath + "\" -s");
			}
			catch (Exception ex)
			{
				LogWrapper.GetLogger("Startup Helper").LogError("Can't set registry to run on startup. EX.: {0}", new object[] { ex });
				return false;
			}
			return true;
		}

		public static bool RemoveFromStartup()
		{
			try
			{
				Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).DeleteValue(Application.ProductName);
			}
			catch (Exception ex)
			{
				LogWrapper.GetLogger("Startup Helper").LogError("Can't delete registry to prevent run on startup. EX.: {0}", new object[] { ex });
				return false;
			}
			return true;
		}

		public static bool IsInStartup()
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				return registryKey != null && registryKey.GetValue(Application.ProductName) != null;
			}
			catch (Exception ex)
			{
				LogWrapper.GetLogger("Startup Helper").LogError("Can't get registry value to determine Startup state. EX.: {0}", new object[] { ex });
			}
			return false;
		}
	}
}
