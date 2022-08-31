using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.UI.Models;
using Shared.Analytics;

namespace Krisp.UI.Views.Windows
{
	public partial class UpdateInfoWindow : Window
	{
		public UpdateInfoWindow(UpdateInfo updateInfo)
		{
			this.InitializeComponent();
			this._updateInfo = updateInfo;
			this.Update.Click += this.Update_Click;
			this.Cancel.Click += this.Cancel_Click;
			this.wb.LoadCompleted += delegate(object s, NavigationEventArgs e)
			{
				this.wb.Visibility = Visibility.Visible;
			};
			if (this._updateInfo != null && !string.IsNullOrWhiteSpace(this._updateInfo.ReleaseNotes))
			{
				try
				{
					this.wb.Navigate(this._updateInfo.ReleaseNotes + "?" + DateTime.Now.ToString("yyyyddMHHmmss"));
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.UpdatePopupEvent(this._updateInfo.Version.ToString()));
				}
				catch (Exception ex)
				{
					LogWrapper.GetLogger("UpdateInfoWindow").LogError("Error on retriving ReleaseNotes. {0}", new object[] { ex.Message });
				}
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		private void Update_Click(object sender, RoutedEventArgs e)
		{
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.UpdateAcceptedEvent(this._updateInfo.Version.ToString()));
			LogWrapper.GetLogger("UpdateInfoWindow").LogInfo("Trying to Update v." + this._updateInfo.Version.ToString());
			Dispatcher dispatcher = Application.Current.Dispatcher;
			if (dispatcher != null)
			{
				dispatcher.InvokeAsync(delegate()
				{
					this.PerformKrispUpdate(this._updateInfo.Package);
				});
			}
			base.Close();
		}

		private void PerformKrispUpdate(string pkg)
		{
			Logger logger = LogWrapper.GetLogger("KrispUpdate");
			string text = Path.Combine(Path.GetTempPath(), DateTime.Now.ToFileTime().ToString() + ".log");
			string fileName = Path.GetFileName(pkg);
			string text2 = Path.Combine(Path.GetTempPath(), fileName);
			DownloadProgressWindow downloadProgressWindow = new DownloadProgressWindow(pkg, text2);
			downloadProgressWindow.ShowDialog();
			HttpAsyncDownloadResult result = downloadProgressWindow.Result;
			result.HResult.LogOnHerror(logger, "Download error. " + result.Message);
			if (result.HResult != 0)
			{
				try
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.UpdateErrorEvent("download", string.Format("dwnRes: {0}", result.HResult)));
					MessageBox.Show("Update Error. " + result.Message);
					if (File.Exists(text2))
					{
						File.Delete(text2);
					}
				}
				catch
				{
					logger.LogWarning("error on deleting temp apdateFile.");
				}
				return;
			}
			if (File.Exists(text2))
			{
				bool flag = false;
				string text3 = null;
				try
				{
					flag = UpdateHelper.checkForKrispSign(text2);
				}
				catch (Exception ex)
				{
					text3 = string.Format("signError: 0x{0:X}, ", ex.HResult);
				}
				if (flag)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("/i \"{0}\" /passive /norestart /L*V \"{1}\"", text2, text);
					Process.Start(new ProcessStartInfo
					{
						FileName = "msiexec",
						Arguments = stringBuilder.ToString(),
						UseShellExecute = false,
						CreateNoWindow = true
					});
					return;
				}
				FileInfo fileInfo = null;
				string text4 = null;
				try
				{
					fileInfo = new FileInfo(text2);
				}
				catch (Exception ex2)
				{
					text4 = string.Format("fError: 0x{0:X}, ", ex2.HResult);
				}
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.UpdateErrorEvent("pkg_sanity_error", string.Format("{0}size:{1}, {2}'{3}'", new object[]
				{
					text3,
					(fileInfo != null) ? new long?(fileInfo.Length) : null,
					text4,
					Path.GetFileName(text2)
				})));
				new InvalidCertificationWindow().ShowDialog();
				try
				{
					if (File.Exists(text2))
					{
						File.Delete(text2);
					}
					return;
				}
				catch
				{
					logger.LogWarning("error on deleting temp apdateFile.");
					return;
				}
			}
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.UpdateErrorEvent("file_not_dound", Path.GetFileName(text2) ?? ""));
			logger.LogWarning("File not found: '" + text2 + "'");
			MessageBox.Show("Update Error. FileNotFound.");
		}

		private UpdateInfo _updateInfo;
	}
}
