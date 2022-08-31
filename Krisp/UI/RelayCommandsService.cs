using System;
using System.Windows;
using System.Windows.Input;
using Krisp.Analytics;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Services;
using Krisp.TestKrisp.ViewModels;
using Krisp.TestKrisp.Views;
using Krisp.UI.Models;
using Krisp.UI.ViewModels;
using Krisp.UI.Views;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI
{
	internal class RelayCommandsService : IRelayCommandsService
	{
		public static RelayCommandsService Instance
		{
			get
			{
				if (RelayCommandsService._instance == null)
				{
					RelayCommandsService._instance = new RelayCommandsService();
				}
				return RelayCommandsService._instance;
			}
		}

		public ICommand AboutCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._aboutCommand) == null)
				{
					relayCommand = (this._aboutCommand = new RelayCommand(delegate(object param)
					{
						if (this.AboutWindow == null)
						{
							this.AboutWindow = new AboutWindow();
							this.AboutWindow.Closed += delegate(object s, EventArgs e1)
							{
								this.AboutWindow = null;
							};
							this.AboutWindow.Show();
							AnalyticsFactory.Instance.Report(AnalyticEventComposer.AboutEvent());
							return;
						}
						this.AboutWindow.BringWindowToTop();
					}));
				}
				return relayCommand;
			}
		}

		public ICommand UpdateWindowCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._checkForUpdateCommand) == null)
				{
					relayCommand = (this._checkForUpdateCommand = new RelayCommand(delegate(object param)
					{
						this._checkingForUpdateWindow = new Krisp.UI.Views.Windows.MessageBox(TranslationSourceViewModel.Instance["CheckingForUpdate"]);
						this._checkingForUpdateWindow.Show();
						ServiceContainer.Instance.GetService<IAccountManager>().UpdateVersionInfoChanged += this.UpdateVersionInfoChanged;
						ServiceContainer.Instance.GetService<IAccountManager>().CheckForUpdateAsync(true);
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.CheckForUpdate());
					}));
				}
				return relayCommand;
			}
		}

		private void UpdateVersionInfoChanged(object sender, VersionInfo e)
		{
			Application.Current.Dispatcher.Invoke(delegate()
			{
				try
				{
					Window checkingForUpdateWindow = this._checkingForUpdateWindow;
					if (checkingForUpdateWindow != null)
					{
						checkingForUpdateWindow.Close();
					}
					this._checkingForUpdateWindow = null;
				}
				finally
				{
					ServiceContainer.Instance.GetService<IAccountManager>().UpdateVersionInfoChanged -= this.UpdateVersionInfoChanged;
					if (e.resultCode == 0)
					{
						Krisp.UI.Views.Windows.MessageBox.Show(TranslationSourceViewModel.Instance["KrispIsUpToDate"]);
					}
					else if (e.resultCode == -1)
					{
						Krisp.UI.Views.Windows.MessageBox.Show(TranslationSourceViewModel.Instance["GeneralErrorMessage"]);
					}
					else
					{
						new UpdateInfoWindow(new UpdateInfo(e)).Show();
					}
				}
			});
		}

		public ICommand ReportBugCommand
		{
			get
			{
				return new RelayCommand<object>(delegate(object param)
				{
					if (AccountManager.Instance.IsLoggedIn)
					{
						ReportBugWindow.Show(new ReportViewModel(ReportSource.manual, null, null));
						return;
					}
					ReportProblemManually.Show(new ReportViewModel(ReportSource.manual, null, null));
				});
			}
		}

		public ICommand SetupKrispCommand
		{
			get
			{
				return new RelayCommand(delegate(object param)
				{
					if (this._onboardingWindow == null)
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.OnboardingStartSetup(false));
						this._onboardingWindow = new OnboardingWindow();
						this._onboardingWindow.Closed += this.OnboardnigWindow_Closed;
						this._onboardingWindow.Show();
						return;
					}
					this._onboardingWindow.BringWindowToTop();
				});
			}
		}

		private void OnboardnigWindow_Closed(object sender, EventArgs e)
		{
			this._onboardingWindow.Closed -= this.OnboardnigWindow_Closed;
			this._onboardingWindow = null;
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			if (krispWindow != null)
			{
				krispWindow.Show();
			}
		}

		public ICommand TestNoiseCancellationCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._testNoiseCancellationCommand) == null)
				{
					relayCommand = (this._testNoiseCancellationCommand = new RelayCommand(delegate(object param)
					{
						if (RelayCommandsService._testKrispWindow != null)
						{
							RelayCommandsService._testKrispWindow.BringWindowToTop();
							return;
						}
						RelayCommandsService._testKrispWindow = new TestKrispWindow();
						RelayCommandsService._testKrispWindow.DataContext = new TestKrispViewModel();
						RelayCommandsService._testKrispWindow.Closed += this.TestKrispWindow_Closed;
						RelayCommandsService._testKrispWindow.Show();
						if (param != null && param is bool && (bool)param)
						{
							AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispInitEvent(false));
							return;
						}
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispInitEvent(true));
					}));
				}
				return relayCommand;
			}
		}

		private void TestKrispWindow_Closed(object sender, EventArgs e)
		{
			RelayCommandsService._testKrispWindow.Closed -= this.TestKrispWindow_Closed;
			RelayCommandsService._testKrispWindow = null;
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			if (krispWindow != null)
			{
				krispWindow.Show();
			}
		}

		public ICommand ContactSupportCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._ContactSupportCommand) == null)
				{
					relayCommand = (this._ContactSupportCommand = new RelayCommand(delegate(object param)
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.ChatEvent(false));
						Helpers.OpenUrl(UrlProvider.GetContactSupportUrl(TranslationSourceViewModel.Instance.SelectedCulture.Name));
					}));
				}
				return relayCommand;
			}
		}

		private static RelayCommandsService _instance;

		private Window AboutWindow;

		private RelayCommand _aboutCommand;

		private RelayCommand _checkForUpdateCommand;

		private Window _checkingForUpdateWindow;

		private Window _onboardingWindow;

		private RelayCommand _testNoiseCancellationCommand;

		private static Window _testKrispWindow;

		private RelayCommand _ContactSupportCommand;
	}
}
