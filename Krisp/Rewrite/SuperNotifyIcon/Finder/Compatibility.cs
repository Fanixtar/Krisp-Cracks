using System;
using System.Windows;

namespace Rewrite.SuperNotifyIcon.Finder
{
	public class Compatibility
	{
		public static bool IsRemoteSession
		{
			get
			{
				return SystemParameters.IsRemoteSession || SystemParameters.IsRemotelyControlled;
			}
		}

		public static int WindowEdgeOffset
		{
			get
			{
				if (!Compatibility.IsDWMEnabled)
				{
					return 0;
				}
				if (Compatibility.CurrentWindowsVersion == Compatibility.WindowsVersion.WindowsVista)
				{
					return 1;
				}
				return 8;
			}
		}

		public static bool BorderVisibility
		{
			get
			{
				return Compatibility.IsDWMEnabled;
			}
		}

		public static Compatibility.WindowsVersion CurrentWindowsVersion
		{
			get
			{
				if (Environment.OSVersion.Version.Major < 6)
				{
					return Compatibility.WindowsVersion.WindowsLegacy;
				}
				if (Environment.OSVersion.Version.Minor == 0)
				{
					return Compatibility.WindowsVersion.WindowsVista;
				}
				return Compatibility.WindowsVersion.Windows7Plus;
			}
		}

		public static bool IsDWMEnabled
		{
			get
			{
				if (Compatibility.CurrentWindowsVersion == Compatibility.WindowsVersion.WindowsLegacy)
				{
					return false;
				}
				bool flag;
				NativeMethods.DwmIsCompositionEnabled(out flag);
				return flag;
			}
		}

		public enum WindowsVersion
		{
			Windows7Plus,
			WindowsVista,
			WindowsLegacy
		}
	}
}
