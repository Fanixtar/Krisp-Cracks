using System;
using System.Windows;
using System.Windows.Input;
using Krisp.Analytics;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	internal class PostSignInPageViewModel : BindableBase, IPageViewModel
	{
		public PostSignInPageViewModel()
		{
			AccountManager.Instance.UserProfileInfoChanged += delegate(object s, UserProfileInfo e)
			{
				this.Email = e.email;
			};
		}

		public string Email { get; set; }

		public ICommand StartSetupCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._startSetupCommand) == null)
				{
					relayCommand = (this._startSetupCommand = new RelayCommand(delegate(object param)
					{
						this.StartSetup();
					}));
				}
				return relayCommand;
			}
		}

		private void StartSetup()
		{
			Settings.Default.DemoMode = false;
			try
			{
				Settings.Default.Save();
			}
			catch
			{
			}
			Mediator.Instance.NotifyColleagues<PageViews>("SelectPageViewModel", PageViews.KrispAppPage);
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.OnboardingStartSetup(true));
			OnboardingWindow onboardingWindow = new OnboardingWindow();
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			krispWindow.AutomaticalyHide = false;
			krispWindow.Activated += this.MainWnd_Activated;
			onboardingWindow.Show();
			onboardingWindow.Closed += this.Wnd_Closed;
		}

		private void Wnd_Closed(object sender, EventArgs e)
		{
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			if (krispWindow != null)
			{
				krispWindow.AutomaticalyHide = true;
				krispWindow.Activated -= this.MainWnd_Activated;
				krispWindow.Show();
			}
		}

		private void MainWnd_Activated(object sender, EventArgs e)
		{
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			krispWindow.AutomaticalyHide = true;
			krispWindow.Activated -= this.MainWnd_Activated;
		}

		public MenuItemsVisibility MenuItemsVisibility { get; } = new MenuItemsVisibility
		{
			SignOut = !DeviceLoginHelper.DeviceMode
		};

		private RelayCommand _startSetupCommand;
	}
}
