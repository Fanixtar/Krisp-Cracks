using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Krisp.UI.Views.Windows
{
	public partial class MessageBox : Window
	{
		public MessageBox(string message)
		{
			this.InitializeComponent();
			this.Ok.Click += delegate(object sender, RoutedEventArgs e)
			{
				base.Close();
			};
			this.Message.Text = message;
		}

		public static void Show(string message)
		{
			Application.Current.Dispatcher.Invoke(delegate()
			{
				new MessageBox(message).Show();
			});
		}
	}
}
