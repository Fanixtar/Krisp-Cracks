using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Views.Windows
{
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			this.InitializeComponent();
			AboutControlViewModel aboutControlViewModel = new AboutControlViewModel();
			base.DataContext = aboutControlViewModel;
			this.OkButton.Click += delegate(object sender, RoutedEventArgs e)
			{
				base.Close();
			};
		}
	}
}
