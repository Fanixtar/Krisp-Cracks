using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using Gu.Wpf.Media;
using Krisp.Analytics;
using Krisp.UI.ViewModels;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.Views.Controls
{
	public partial class OnboardingSetupView : UserControl
	{
		public OnboardingSetupView()
		{
			this.InitializeComponent();
			base.Loaded += new RoutedEventHandler(this.ViewLoaded);
		}

		public void ViewLoaded(object s, EventArgs e)
		{
			Window.GetWindow(this).Closed += delegate(object sender, EventArgs eventArgs)
			{
				this.MediaElement.Stop();
			};
		}

		private void OpenHelpdeskClick(object sender, RoutedEventArgs e)
		{
			Helpers.OpenUrl(UrlProvider.GetHelpdeskUrl());
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.OnboardingHelp());
		}

		private void ContactSupportClick(object sender, RoutedEventArgs e)
		{
			Helpers.OpenUrl(UrlProvider.GetContactSupportUrl(TranslationSourceViewModel.Instance.SelectedCulture.Name));
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.ChatEvent(true));
		}
	}
}
