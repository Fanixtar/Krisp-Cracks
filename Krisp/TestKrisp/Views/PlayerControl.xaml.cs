using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Krisp.TestKrisp.Views
{
	public partial class PlayerControl : UserControl
	{
		public PlayerControl()
		{
			this.InitializeComponent();
			this.PlayerButtons.Visibility = Visibility.Hidden;
			this.Toggle.Checked += new RoutedEventHandler(this.ToggleChecked);
		}

		public void DoneClicked(object sender, RoutedEventArgs e)
		{
			Window.GetWindow(this).Close();
		}

		public void ToggleChecked(object sender, EventArgs e)
		{
			this.Toggle.Checked -= new RoutedEventHandler(this.ToggleChecked);
			this.PlayerButtons.Visibility = Visibility.Visible;
		}

		private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.Toggle.IsChecked = !this.Toggle.IsChecked;
		}
	}
}
