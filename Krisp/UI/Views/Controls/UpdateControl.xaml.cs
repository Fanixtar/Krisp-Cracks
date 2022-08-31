using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Krisp.UI.ViewModels;
using Krisp.UI.Views.Windows;

namespace Krisp.UI.Views.Controls
{
	public partial class UpdateControl : UserControl
	{
		public UpdateControl()
		{
			this.InitializeComponent();
		}

		private void UpdateControl_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (this.updateWindow == null)
			{
				this.updateWindow = new UpdateInfoWindow((base.DataContext as UpdateInfoViewModel).UpdateInfo);
				this.updateWindow.Closed += delegate(object s, EventArgs e1)
				{
					this.updateWindow = null;
				};
				this.updateWindow.Show();
				return;
			}
			this.updateWindow.BringWindowToTop();
		}

		private UpdateInfoWindow updateWindow;
	}
}
