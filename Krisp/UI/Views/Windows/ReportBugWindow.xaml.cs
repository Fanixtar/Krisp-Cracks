using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Markup;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.UI.ViewModels;
using Shared.Analytics;

namespace Krisp.UI.Views.Windows
{
	public partial class ReportBugWindow : Window
	{
		private ReportBugWindow()
		{
			this.InitializeComponent();
			this.Cancel.Click += this.Cancel_Click;
			this.Report.Click += this.Report_Click;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		private void Report_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			this._logger.LogInfo("Get debug logs clicked.");
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = false,
				Description = TranslationSourceViewModel.Instance["ExportLogsDestinaitonMessage"]
			};
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.ReportGetDebugLogsEvent());
				(base.DataContext as ReportViewModel).GenerateReportFile(folderBrowserDialog.SelectedPath);
				base.Close();
			}
		}

		public static void Show(ReportViewModel vm)
		{
			if (ReportBugWindow._instance == null)
			{
				ReportBugWindow._instance = new ReportBugWindow();
				ReportBugWindow._instance.DataContext = vm;
				ReportBugWindow._instance.Closed += delegate(object s, EventArgs e)
				{
					ReportBugWindow._instance = null;
				};
				ReportBugWindow._instance.Show();
				return;
			}
			ReportBugWindow._instance.DataContext = vm;
			ReportBugWindow._instance.BringWindowToTop();
		}

		private Logger _logger = LogWrapper.GetLogger("ReportProblem");

		private static Window _instance;
	}
}
