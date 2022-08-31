using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Krisp.UI.Views.Windows
{
	public partial class ProxyCredentialsPromptWindow : Window
	{
		public ProxyCredentialsPromptWindow()
		{
			this.InitializeComponent();
			this.Submit.Click += delegate(object sender, RoutedEventArgs e)
			{
				base.DialogResult = new bool?(true);
				base.Close();
			};
		}
	}
}
