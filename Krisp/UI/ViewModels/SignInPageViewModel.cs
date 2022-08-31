using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.UI.Views;
using MVVMFoundation;
using Shared.Analytics;

namespace Krisp.UI.ViewModels
{
	internal class SignInPageViewModel : BindableBase, IPageViewModel
	{
		public SignInPageViewModel()
		{
			this._dispatcher = Dispatcher.CurrentDispatcher;
		}

		private void SignIn()
		{
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.SigninAttemptEvent());
			ServiceContainer.Instance.GetService<IAccountManager>().Login(new Action<string>(Helpers.OpenUrl));
			AccountManager.Instance.StateChanged += this.AccountManagerStateChanged;
		}

		private void AccountManagerStateChanged(object sender, AccountManagerStateChangedEventArgs e)
		{
			if (e != null && e.State == AccountManagerState.LoggedIn)
			{
				this._dispatcher.Invoke(delegate()
				{
					Application.Current.MainWindow.Show();
				});
			}
			AccountManager.Instance.StateChanged -= this.AccountManagerStateChanged;
		}

		public ICommand SignInCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._signInCommand) == null)
				{
					relayCommand = (this._signInCommand = new RelayCommand(delegate(object param)
					{
						this.SignIn();
					}));
				}
				return relayCommand;
			}
		}

		public MenuItemsVisibility MenuItemsVisibility { get; } = new MenuItemsVisibility();

		private RelayCommand _signInCommand;

		private Dispatcher _dispatcher;
	}
}
