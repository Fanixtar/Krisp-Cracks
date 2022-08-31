using System;

namespace Krisp.Models
{
	public class DefaultDeviceItem
	{
		protected static string changeDisplayName(bool bAuto, string newName)
		{
			string text;
			if (bAuto)
			{
				text = "Auto - " + newName;
			}
			else
			{
				text = "Default - " + newName;
			}
			return text;
		}

		public static string s_AutoDisplayName = "Choose Automatically";

		public static string s_AutoDescription = "Choose Automatically As System Default";

		public static string s_DefaultDisplayName = "Default Device";

		public static string s_DefaultDescription = "Same As System Default";

		public static Func<bool, string, string> ChangeDisplayNameFn = new Func<bool, string, string>(DefaultDeviceItem.changeDisplayName);
	}
}
