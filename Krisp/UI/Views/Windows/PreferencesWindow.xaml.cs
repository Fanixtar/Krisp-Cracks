using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.UI.ViewModels;
using Shared.Analytics;

namespace Krisp.UI.Views.Windows
{
	public partial class PreferencesWindow : Window
	{
		public PreferencesWindow()
		{
			this._logger.LogInfo("Preferences window opened.");
			this.InitializeComponent();
			base.DataContext = PreferencesViewModel.Instance;
			this.RunOnStartup.IsChecked = new bool?(Startup.IsInStartup());
			this.RunOnStartup.Checked += this.RunOnStartup_Checked;
			this.RunOnStartup.Unchecked += this.RunOnStartup_Checked;
			this.AdvancedPreferencesSection.Visibility = (AccountManager.Instance.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed);
			this.LanguageSwitch.DataContext = TranslationSourceViewModel.Instance;
		}

		private void RunOnStartup_Checked(object sender, RoutedEventArgs e)
		{
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.PreferencesLaunchAtStartup(!Startup.IsInStartup()));
			if (Startup.IsInStartup())
			{
				this._logger.LogInfo("Removing Krisp from startup.");
				Startup.RemoveFromStartup();
				return;
			}
			this._logger.LogInfo("Adding Krisp to startup.");
			Startup.RunOnStartup();
		}

		private Logger _logger = LogWrapper.GetLogger("Preferences");
	}
}
