using System;
using System.Windows.Input;
using Krisp.AppHelper;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class OnboardingViewModel : BindableBase
	{
		private Logger Logger { get; } = LogWrapper.GetLogger("Onboarding");

		public object CurrentView
		{
			get
			{
				return this._currentView;
			}
			private set
			{
				if (this._currentView != value)
				{
					this._currentView = value;
					base.RaisePropertyChanged("CurrentView");
				}
			}
		}

		public OnboardingViewModel()
		{
			this.Logger.LogInfo("Onboarding window opened.");
			this.CurrentView = new OnboardingAppSelectionViewModel();
		}

		public ICommand AppSelectedCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._appSelectedCommand) == null)
				{
					relayCommand = (this._appSelectedCommand = new RelayCommand(delegate(object param)
					{
						this.Logger.LogInfo("{0} video chosen", new object[] { (string)param });
						this.CurrentView = new OnboardingSetupViewModel((string)param);
					}));
				}
				return relayCommand;
			}
		}

		public ICommand BackToAppSelectionViewCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._backToAppSelectionCommand) == null)
				{
					relayCommand = (this._backToAppSelectionCommand = new RelayCommand(delegate(object param)
					{
						(this.CurrentView as OnboardingSetupViewModel).SendVideoAnalytics();
						this.CurrentView = new OnboardingAppSelectionViewModel();
						this.Logger.LogInfo("Navigating back to app selection screen.");
					}));
				}
				return relayCommand;
			}
		}

		public ICommand NextCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._nextCommand) == null)
				{
					relayCommand = (this._nextCommand = new RelayCommand(delegate(object param)
					{
						(this.CurrentView as OnboardingSetupViewModel).SendVideoAnalytics();
						this.CurrentView = new OnboardingFinishViewModel();
						this.Logger.LogInfo("Navigating to finish screen");
					}));
				}
				return relayCommand;
			}
		}

		private object _currentView;

		private RelayCommand _appSelectedCommand;

		private RelayCommand _backToAppSelectionCommand;

		private RelayCommand _nextCommand;
	}
}
