using System;
using System.Windows.Input;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using MVVMFoundation;
using Newtonsoft.Json;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	public class KrispAppPageViewModel : BindableBase, IPageViewModel
	{
		public KrispControllerViewModel SpeakerControllerViewModel { get; private set; }

		public KrispControllerViewModel MicrophoneControllerViewModel { get; private set; }

		public AppModeViewModel AppModeViewModel
		{
			get
			{
				return this._appModeViewModel;
			}
			private set
			{
				if (this._appModeViewModel != value)
				{
					this._appModeViewModel = value;
					base.RaisePropertyChanged("AppModeViewModel");
				}
			}
		}

		public UpdateInfoViewModel UpdateInfoViewModel { get; private set; }

		public HeaderViewModel HeaderViewModel { get; private set; }

		public bool ShowGiftMessage
		{
			get
			{
				return this._showGiftMessage;
			}
			set
			{
				if (value != this._showGiftMessage)
				{
					this._showGiftMessage = value;
					base.RaisePropertyChanged("ShowGiftMessage");
				}
			}
		}

		public ICommand ContinueToFreeProCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._continueToFreeProCommand) == null)
				{
					relayCommand = (this._continueToFreeProCommand = new RelayCommand(delegate(object param)
					{
						this.ShowGiftMessage = false;
					}));
				}
				return relayCommand;
			}
		}

		public KrispAppPageViewModel()
		{
			this._logger = LogWrapper.GetLogger("KrispAppPageViewModel");
			if (DeviceLoginHelper.DeviceMode && Settings.Default.DemoMode)
			{
				Settings.Default.DemoMode = false;
				try
				{
					Settings.Default.Save();
				}
				catch (Exception ex)
				{
					this._logger.LogError("Error on storing DemoMode state. {0}", new object[] { ex.Message });
				}
			}
			this.SpeakerControllerViewModel = new KrispControllerViewModel(AudioDeviceKind.Speaker);
			this.MicrophoneControllerViewModel = new KrispControllerViewModel(AudioDeviceKind.Microphone);
			this.SpeakerControllerViewModel.NCSwitch = Settings.Default.SpeakerNCState;
			this.MicrophoneControllerViewModel.NCSwitch = Settings.Default.MicrophoneNCState;
			this.ShowGiftMessage = false;
			this.UpdateInfoViewModel = new UpdateInfoViewModel();
			this.HeaderViewModel = new HeaderViewModel();
			AccountManager.Instance.UserProfileInfoChanged += this.UserProfileInfoChanged;
		}

		private void UserProfileInfoChanged(object sender, UserProfileInfo userProfile)
		{
			this.MenuItemsVisibility.CheckForUpdate = AccountManager.ShowAvailabilityOfUpdate();
			this.MenuItemsVisibility.ContactSupport = AccountManager.ShowContactSupport();
			this.MenuItemsVisibility.ReportBug = AccountManager.ShowReportProblem();
			base.RaisePropertyChanged("MenuItemsVisibility");
			string name = userProfile.mode.name;
			this._logger.LogDebug("{0} appmode recieved", new object[] { name });
			if (name == "minutes")
			{
				MinutesBalance minutesBalance = new MinutesBalance(userProfile.settings.nc_out.minutes_settings);
				if (this.AppModeViewModel == null)
				{
					this.AppModeViewModel = new MinutesModeViewModel(this.MicrophoneControllerViewModel, this.SpeakerControllerViewModel);
					(this.AppModeViewModel as MinutesModeViewModel).Enable(minutesBalance);
				}
				else if (!(this.AppModeViewModel is MinutesModeViewModel))
				{
					this.AppModeViewModel.Disable();
					this.AppModeViewModel = new MinutesModeViewModel(this.MicrophoneControllerViewModel, this.SpeakerControllerViewModel);
					(this.AppModeViewModel as MinutesModeViewModel).Enable(minutesBalance);
				}
				else if (this.AppModeViewModel.Disabled)
				{
					(this.AppModeViewModel as MinutesModeViewModel).Enable(minutesBalance);
				}
				else
				{
					(this.AppModeViewModel as MinutesModeViewModel).MinuteModeProps = minutesBalance;
				}
			}
			else if (name == "trial")
			{
				TrialMode_Props trialMode_Props = JsonConvert.DeserializeObject<TrialMode_Props>(userProfile.mode.props);
				if (this.AppModeViewModel == null)
				{
					this.AppModeViewModel = new TrialModeViewModel();
					(this.AppModeViewModel as TrialModeViewModel).Enable(trialMode_Props);
				}
				else if (this.AppModeViewModel is MinutesModeViewModel)
				{
					this.AppModeViewModel.Disable();
					this.AppModeViewModel = new TrialModeViewModel();
					(this.AppModeViewModel as TrialModeViewModel).Enable(trialMode_Props);
					this.ShowGiftMessage = true;
					this.MicrophoneControllerViewModel.NCSwitch = true;
				}
				else if (this.AppModeViewModel is TrialModeViewModel)
				{
					if (this.AppModeViewModel.Disabled)
					{
						(this.AppModeViewModel as TrialModeViewModel).Enable(trialMode_Props);
					}
					else
					{
						(this.AppModeViewModel as TrialModeViewModel).Props = trialMode_Props;
					}
				}
				else
				{
					this.AppModeViewModel.Disable();
					this.AppModeViewModel = null;
					this.AppModeViewModel = new TrialModeViewModel();
					(this.AppModeViewModel as TrialModeViewModel).Enable(trialMode_Props);
				}
			}
			else if (name == "unlimited")
			{
				if (!(this.AppModeViewModel is UnlimitedModeViewModel))
				{
					if (this.AppModeViewModel != null)
					{
						this.AppModeViewModel.Disable();
						if (this.AppModeViewModel is MinutesModeViewModel)
						{
							this.MicrophoneControllerViewModel.NCSwitch = true;
						}
						this.AppModeViewModel = null;
					}
					this.AppModeViewModel = new UnlimitedModeViewModel();
				}
			}
			else
			{
				this._logger.LogError("Unknown mode recieved from backend: {0}", new object[] { name });
				Mediator.Instance.NotifyColleagues<PageViews>("SelectPageViewModel", PageViews.GenericPage);
			}
			BaseProfileSetting room_echo = userProfile.settings.nc_out.room_echo;
			this.MicrophoneControllerViewModel.RoomEchoAvailable = room_echo != null && room_echo.available;
		}

		~KrispAppPageViewModel()
		{
			this.DeattachDevices();
		}

		internal void AttachDevices()
		{
			this.SpeakerControllerViewModel.AttachHandlers();
			this.MicrophoneControllerViewModel.AttachHandlers();
			AppModeViewModel appModeViewModel = this.AppModeViewModel;
			if (appModeViewModel == null)
			{
				return;
			}
			appModeViewModel.AttachHandlers();
		}

		internal void DeattachDevices()
		{
			this.SpeakerControllerViewModel.DeAttachHandlers();
			this.MicrophoneControllerViewModel.DeAttachHandlers();
			AppModeViewModel appModeViewModel = this.AppModeViewModel;
			if (appModeViewModel == null)
			{
				return;
			}
			appModeViewModel.DeAttachHandlers();
		}

		public void OnSignOut()
		{
			MinutesModeViewModel minutesModeViewModel = this.AppModeViewModel as MinutesModeViewModel;
			if (minutesModeViewModel != null)
			{
				minutesModeViewModel.SendActiveUsage(false);
			}
			AppModeViewModel appModeViewModel = this.AppModeViewModel;
			if (appModeViewModel != null)
			{
				appModeViewModel.Disable();
			}
			this.AppModeViewModel = null;
		}

		public void OnQuit()
		{
			MinutesModeViewModel minutesModeViewModel = this.AppModeViewModel as MinutesModeViewModel;
			if (minutesModeViewModel == null)
			{
				return;
			}
			minutesModeViewModel.SendActiveUsage(false);
		}

		public MenuItemsVisibility MenuItemsVisibility { get; } = new MenuItemsVisibility
		{
			SignOut = !DeviceLoginHelper.DeviceMode
		};

		private AppModeViewModel _appModeViewModel;

		private RelayCommand _continueToFreeProCommand;

		private bool _showGiftMessage;

		private Logger _logger;
	}
}
