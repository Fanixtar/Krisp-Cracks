using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Krisp.TestKrisp.ViewModels;
using Microsoft.Win32;

namespace Krisp.TestKrisp.Views
{
	public partial class TestKrispWindow : Window
	{
		public TestKrispWindow()
		{
			this.InitializeComponent();
			base.Closed += this.TestKrispWindow_Closed;
			SystemEvents.SessionSwitch += this.SystemEvents_SessionSwitch;
		}

		private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			if (e.Reason.Equals(SessionSwitchReason.ConsoleDisconnect))
			{
				base.Close();
			}
		}

		private void TestKrispWindow_Closed(object sender, EventArgs e)
		{
			SystemEvents.SessionSwitch -= this.SystemEvents_SessionSwitch;
			if (base.DataContext is TestKrispViewModel)
			{
				(base.DataContext as TestKrispViewModel).Cleanup();
			}
		}
	}
}
