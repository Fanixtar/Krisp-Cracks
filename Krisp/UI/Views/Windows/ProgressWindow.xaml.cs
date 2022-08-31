using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Views.Windows
{
	public partial class ProgressWindow : Window
	{
		public ProgressWindow(Func<bool> f)
		{
			ProgressWindow <>4__this = this;
			this.InitializeComponent();
			base.Closing += this.ProgressWindow_Closing;
			Task.Run(delegate()
			{
				bool result = f();
				<>4__this.Dispatcher.Invoke(delegate()
				{
					<>4__this.LoadingControl.Visibility = Visibility.Hidden;
					<>4__this.OkButton.Visibility = Visibility.Visible;
					<>4__this.InfoText.Visibility = Visibility.Visible;
					if (!result)
					{
						<>4__this.InfoText.Text = TranslationSourceViewModel.Instance["ExportLogsFailedMessage"];
						<>4__this.GetDebugLogButton.Visibility = Visibility.Visible;
					}
					<>4__this.Closing -= <>4__this.ProgressWindow_Closing;
				});
			});
		}

		private void ProgressWindow_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			base.Close();
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
				new ReportViewModel(ReportSource.manual, null, null).GenerateReportFile(folderBrowserDialog.SelectedPath);
				base.Close();
			}
		}
	}
}
