using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.UI.ViewModels;
using Shared.Analytics;
using WUApiLib;

namespace Krisp.UI.Views.Windows
{
	public partial class RepairKrispWindow : Window
	{
		private RepairKrispWindow()
		{
			this.InitializeComponent();
			this.Cancel.Click += this.Cancel_Click;
			this.Repair.Click += this.Repair_Click;
			this.UpdateData();
			this._logger.LogInfo(string.Format("Repair action requested. PreSystemRestartRequired: {0}", this._preRestartRequired));
		}

		public static void ShowOrBringToTop()
		{
			if (RepairKrispWindow._reprotWindow == null)
			{
				RepairKrispWindow._reprotWindow = new RepairKrispWindow();
				RepairKrispWindow._reprotWindow.Closed += delegate(object s, EventArgs e)
				{
					RepairKrispWindow._reprotWindow = null;
				};
				RepairKrispWindow._reprotWindow.Show();
				return;
			}
			RepairKrispWindow repairKrispWindow = RepairKrispWindow._reprotWindow as RepairKrispWindow;
			if (repairKrispWindow != null)
			{
				repairKrispWindow.UpdateData();
			}
			RepairKrispWindow._reprotWindow.BringWindowToTop();
		}

		public void UpdateData()
		{
			this._preRestartRequired = this.retrieveWUSysState() == 1;
			if (this._preRestartRequired)
			{
				this.Repair.IsEnabled = false;
				this.Message.Text = TranslationSourceViewModel.Instance["SystemRestartMessage"];
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this._logger.LogInfo("Repair action canceled");
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.RepairEvent(false));
			this.Cancel.Click -= this.Cancel_Click;
			this.Repair.Click -= this.Repair_Click;
			base.Close();
			RepairKrispWindow._reprotWindow = null;
		}

		private void Repair_Click(object sender, RoutedEventArgs e)
		{
			this._logger.LogInfo("Repair action accepted");
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.RepairEvent(true));
			UpdateHelper.PerformKrispRepair();
			this.Cancel.Click -= this.Cancel_Click;
			this.Repair.Click -= this.Repair_Click;
			base.Close();
			RepairKrispWindow._reprotWindow = null;
		}

		private int retrieveWUSysState()
		{
			try
			{
				return ((SystemInformation)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("C01B9BA0-BEA7-41BA-B604-D0A36F469133")))).RebootRequired ? 1 : 0;
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to fetch systemInfo. Error: ", new object[] { ex.Message });
			}
			return -1;
		}

		private Logger _logger = LogWrapper.GetLogger("RepairKrispWindow");

		private static Window _reprotWindow;

		private bool _preRestartRequired;
	}
}
