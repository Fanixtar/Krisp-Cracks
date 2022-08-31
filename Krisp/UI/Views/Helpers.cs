using System;
using System.Diagnostics;
using Krisp.AppHelper;
using Krisp.UI.ViewModels;
using Krisp.UI.Views.Windows;

namespace Krisp.UI.Views
{
	public static class Helpers
	{
		public static void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch (Exception ex)
			{
				try
				{
					Process.Start("IExplore.exe", url);
					LogWrapper.GetLogger("UrlOpener").LogWarning("Unable to open '{0}' in default browser, opened in IE # ex: {1}", new object[] { url, ex.Message });
				}
				catch (Exception ex2)
				{
					LogWrapper.GetLogger("UrlOpener").LogError("Unable to open '{0}' by any browser # ex: {1}", new object[] { url, ex2.Message });
					MessageBox.Show(TranslationSourceViewModel.Instance["NoBrowserErrorMessage"]);
				}
			}
		}
	}
}
