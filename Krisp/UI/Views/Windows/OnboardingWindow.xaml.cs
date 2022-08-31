using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Krisp.AppHelper;
using Krisp.UI.ViewModels;

namespace Krisp.UI.Views.Windows
{
	public partial class OnboardingWindow : Window
	{
		private Logger Logger { get; } = LogWrapper.GetLogger("Onboarding");

		public OnboardingWindow()
		{
			this.InitializeComponent();
			base.Closing += new CancelEventHandler(this.OnboardingWindow_Closing);
			base.DataContext = new OnboardingViewModel();
		}

		private void OnboardingWindow_Closing(object sender, EventArgs e)
		{
			if (base.DataContext is OnboardingViewModel)
			{
				OnboardingSetupViewModel onboardingSetupViewModel = (base.DataContext as OnboardingViewModel).CurrentView as OnboardingSetupViewModel;
				if (onboardingSetupViewModel != null)
				{
					onboardingSetupViewModel.SendVideoAnalytics();
				}
			}
			this.Logger.LogInfo("Window close initiated.");
		}
	}
}
