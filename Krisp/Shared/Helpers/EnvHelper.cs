using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Shared.Helpers
{
	public static class EnvHelper
	{
		public static string TwoHzLocalFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "2Hz");
			}
		}

		public static string TransFormToOldFilePath(string newFPath)
		{
			return Path.Combine(EnvHelper.TwoHzLocalFolder, "Krisp", Path.GetFileName(newFPath));
		}

		public static string KrispLocalFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Krisp");
			}
		}

		public static string KrispAppLocalFolder
		{
			get
			{
				return EnvHelper.KrispLocalFolder;
			}
		}

		public static string TestKrispRecordingsFolder
		{
			get
			{
				return Path.Combine(EnvHelper.KrispAppLocalFolder, "Recordings");
			}
		}

		public static string KrispAppLogFolder
		{
			get
			{
				return Path.Combine(EnvHelper.KrispAppLocalFolder, "Logs");
			}
		}

		public static Configuration GlobalConfig
		{
			get
			{
				return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
				{
					ExeConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Krisp", "Krisp.exe.config")
				}, ConfigurationUserLevel.None, false);
			}
		}

		public static string KrispFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Krisp");
			}
		}

		public static string KrispExeFullPath
		{
			get
			{
				return Path.Combine(EnvHelper.KrispFolder, "Krisp.exe");
			}
		}

		private static Version GetKrispVersion()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return new Version(version.Major, version.Minor, version.Build);
		}

		public static Version KrispVersion { get; } = EnvHelper.GetKrispVersion();
	}
}
