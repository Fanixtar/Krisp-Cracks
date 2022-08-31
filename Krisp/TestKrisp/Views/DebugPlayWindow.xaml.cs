using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Krisp.TestKrisp.ViewModels;

namespace Krisp.TestKrisp.Views
{
	public partial class DebugPlayWindow : Window
	{
		public DebugPlayWindow()
		{
			this.InitializeComponent();
			base.DataContext = new DebugPlayViewModel();
			base.Closed += delegate(object s, EventArgs e)
			{
				(base.DataContext as DebugPlayViewModel).Dispose();
			};
		}
	}
}
