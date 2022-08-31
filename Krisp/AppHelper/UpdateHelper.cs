using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Win32;

namespace Krisp.AppHelper
{
	public static class UpdateHelper
	{
		public static string GetAppInstallProductCode(string appName)
		{
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", false))
			{
				if (registryKey != null)
				{
					foreach (string text in registryKey.GetSubKeyNames())
					{
						try
						{
							using (RegistryKey registryKey2 = registryKey.OpenSubKey(text))
							{
								string text2;
								if (registryKey2 == null)
								{
									text2 = null;
								}
								else
								{
									object value = registryKey2.GetValue("DisplayName");
									text2 = ((value != null) ? value.ToString() : null);
								}
								string text3 = text2;
								if (text3 != null && text3 == appName)
								{
									return text;
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			return "";
		}

		public static void PerformKrispRepair()
		{
			try
			{
				string text = Path.Combine(Path.GetTempPath(), DateTime.Now.ToFileTime().ToString() + ".log");
				string appInstallProductCode = UpdateHelper.GetAppInstallProductCode("Krisp");
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("/i \"{0}\" /passive /norestart /L*V \"{1}\"  REINSTALL=ALL REINSTALLMODE=a", appInstallProductCode, text);
				Process.Start(new ProcessStartInfo
				{
					FileName = "msiexec",
					Arguments = stringBuilder.ToString(),
					UseShellExecute = false,
					CreateNoWindow = true
				});
			}
			catch
			{
			}
		}

		public static bool checkForKrispSign(string filePath)
		{
			X509Certificate x509Certificate = X509Certificate.CreateFromSignedFile(filePath);
			X509Certificate2 x509Certificate2 = new X509Certificate2(x509Certificate);
			if (x509Certificate2.Verify())
			{
				bool flag = new X509Chain().Build(x509Certificate2);
				AssemblyProductAttribute customAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>();
				return flag && x509Certificate.Subject.Contains("CN=\"" + customAttribute.Product + " Technologies, Inc\"");
			}
			return false;
		}
	}
}
