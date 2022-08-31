using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Shared.Helpers
{
	public static class RunModeChecker
	{
		public static string ShortName(this RunModeChecker.RunMode mode)
		{
			return RunModeChecker.ModeEnumToName[mode];
		}

		private static RunModeChecker.RunMode GetMode()
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Krisp\\");
				int num = (((registryKey != null) ? registryKey.GetValue("RunMode", 0) : null) as int?) ?? 0;
				if (!Enum.IsDefined(typeof(RunModeChecker.RunMode), num))
				{
					return RunModeChecker.RunMode.Production;
				}
				return (RunModeChecker.RunMode)num;
			}
			catch
			{
			}
			return RunModeChecker.RunMode.Production;
		}

		public static RunModeChecker.RunMode Mode = RunModeChecker.GetMode();

		public static bool IsProduction = RunModeChecker.Mode == RunModeChecker.RunMode.Production;

		private static readonly Dictionary<RunModeChecker.RunMode, string> ModeEnumToName = new Dictionary<RunModeChecker.RunMode, string>
		{
			{
				RunModeChecker.RunMode.Production,
				"prod"
			},
			{
				RunModeChecker.RunMode.Staging,
				"stage"
			},
			{
				RunModeChecker.RunMode.Development,
				"dev"
			},
			{
				RunModeChecker.RunMode.Local,
				"local"
			},
			{
				RunModeChecker.RunMode.NgRok,
				"ngrok"
			}
		};

		public enum RunMode
		{
			Production,
			Staging = 255,
			Development = 221,
			Local = 170,
			NgRok = 187
		}
	}
}
