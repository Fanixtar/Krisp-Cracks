using System;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Views.Windows;

namespace Krisp.UI.ViewModels
{
	public static class ADMStateExtension
	{
		public static string ToStrFromStringTable(this ADMStateFlags st, bool ConsideringLocalization = false)
		{
			string text = "";
			foreach (object obj in Enum.GetValues(typeof(ADMStateFlags)))
			{
				ADMStateFlags admstateFlags = (ADMStateFlags)obj;
				if (admstateFlags != ADMStateFlags.UI_UnRecoverable && admstateFlags != ADMStateFlags.HealtyState && st.HasFlag(admstateFlags))
				{
					text = Enum.GetName(typeof(ADMStateFlags), admstateFlags);
					break;
				}
			}
			text = (ConsideringLocalization ? TranslationSourceViewModel.Instance[text] : Resources.ResourceManager.GetString(text));
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
			return "Unknonwn Error";
		}

		public static Action HasRepairHandler(this ADMStateFlags st)
		{
			if (st == ADMStateFlags.KrispDeviceNotDetected)
			{
				return new Action(RepairKrispWindow.ShowOrBringToTop);
			}
			return null;
		}

		public static bool IsUiUnrecoverable(this ADMStateFlags st)
		{
			return (st & ADMStateFlags.UI_UnRecoverable) == ADMStateFlags.UI_UnRecoverable;
		}
	}
}
