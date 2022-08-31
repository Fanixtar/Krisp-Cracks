using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using Krisp.Analytics;
using Krisp.UI.ViewModels;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.Views
{
	public class ReportProblemManually : Window, IComponentConnector
	{
		private ReportProblemManually()
		{
			this.InitializeComponent();
		}

		public static void Show(ReportViewModel vm)
		{
			if (ReportProblemManually._instance == null)
			{
				ReportProblemManually._instance = new ReportProblemManually();
				ReportProblemManually._instance.DataContext = vm;
				ReportProblemManually._instance.Closed += delegate(object s, EventArgs e)
				{
					ReportProblemManually._instance = null;
				};
				ReportProblemManually._instance.Show();
				return;
			}
			ReportProblemManually._instance.DataContext = vm;
			ReportProblemManually._instance.BringWindowToTop();
		}

		private void GetDebugLogClicked(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = false,
				Description = TranslationSourceViewModel.Instance["ExportLogsDestinaitonMessage"]
			};
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				(base.DataContext as ReportViewModel).GenerateReportFile(folderBrowserDialog.SelectedPath);
				base.Close();
			}
		}

		private void ContactSupportClicked(object sender, RoutedEventArgs e)
		{
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.ChatEvent(false));
			Helpers.OpenUrl(UrlProvider.GetContactSupportUrl(TranslationSourceViewModel.Instance.SelectedCulture.Name));
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri uri = new Uri("/Krisp;component/ui/views/windows/reportproblemmanually.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, uri);
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		internal Delegate _CreateDelegate(Type delegateType, string handler)
		{
			return Delegate.CreateDelegate(delegateType, this, handler);
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				this.Message = (TextBlock)target;
				return;
			case 2:
				((System.Windows.Controls.Button)target).Click += this.GetDebugLogClicked;
				return;
			case 3:
				((System.Windows.Controls.Button)target).Click += this.ContactSupportClicked;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}

		private static Window _instance;

		internal TextBlock Message;

		private bool _contentLoaded;
	}
}
