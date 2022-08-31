using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Krisp.Properties;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Views.Windows
{
	public partial class InvalidCertificationWindow : Window
	{
		public InvalidCertificationWindow()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ReportBugWindow.Show(new ReportViewModel(ReportSource.security, null, Krisp.Properties.Resources.ResourceManager.GetString("InvalidCertificateReport")));
			base.DialogResult = new bool?(true);
		}
	}
}
