using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Krisp.UI.Views.Controls
{
	public partial class OnboardingFinishView : UserControl
	{
		public OnboardingFinishView()
		{
			this.InitializeComponent();
		}

		public void FinishClicked(object sender, EventArgs e)
		{
			Window.GetWindow(this).Close();
		}
	}
}
