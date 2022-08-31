using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Krisp.UI.Views.Windows
{
	public partial class LanguageSwitchConfiramtionDialog : Window
	{
		public LanguageSwitchConfiramtionDialog()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			base.DialogResult = new bool?(true);
		}
	}
}
