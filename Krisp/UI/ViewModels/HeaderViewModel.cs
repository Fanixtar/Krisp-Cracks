using System;
using System.Windows;
using System.Windows.Input;
using Krisp.Analytics;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.UI.Views;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	public class HeaderViewModel : BindableBase
	{
		public bool HasWarning
		{
			get
			{
				return this._hasWarning;
			}
			set
			{
				if (this._hasWarning != value)
				{
					this._hasWarning = value;
					base.RaisePropertyChanged("HasWarning");
				}
			}
		}

		public UserProfileInfo UserProfileInfo
		{
			get
			{
				return this._userProfileInfo;
			}
			set
			{
				if (this._userProfileInfo != value)
				{
					this._userProfileInfo = value;
					base.RaisePropertyChanged("UserProfileInfo");
				}
			}
		}

		public HeaderViewModel()
		{
			this.ShareCommandAvailable = true;
			this.CreateTeamCommandAvailable = !DeviceLoginHelper.DeviceMode;
			AccountManager.Instance.UserProfileInfoChanged += delegate(object s, UserProfileInfo e)
			{
				this.UserProfileInfo = e;
				this.ShareCommandAvailable = this.UserProfileInfo.team == null;
				this.CreateTeamCommandAvailable = !DeviceLoginHelper.DeviceMode && this.UserProfileInfo.team == null;
			};
		}

		public bool ShareCommandAvailable
		{
			get
			{
				return this._shareCommandAvailable;
			}
			set
			{
				if (this._shareCommandAvailable != value)
				{
					this._shareCommandAvailable = value;
					base.RaisePropertyChanged("ShareCommandAvailable");
				}
			}
		}

		public ICommand ShareCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._shareCommand) == null)
				{
					relayCommand = (this._shareCommand = new RelayCommand(delegate(object param)
					{
						if (this._shareWindow == null)
						{
							this._shareWindow = new ShareWindow(this.UserProfileInfo);
							this._shareWindow.Closed += delegate(object s, EventArgs e1)
							{
								this._shareWindow = null;
							};
							this._shareWindow.Show();
							return;
						}
						this._shareWindow.BringWindowToTop();
					}));
				}
				return relayCommand;
			}
		}

		public ICommand OpenProfileCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._openProfileCommand) == null)
				{
					relayCommand = (this._openProfileCommand = new RelayCommand(delegate(object param)
					{
						if (!DeviceLoginHelper.DeviceMode)
						{
							Helpers.OpenUrl(AccountManager.Instance.GetProfileURL());
						}
					}));
				}
				return relayCommand;
			}
		}

		public bool CreateTeamCommandAvailable
		{
			get
			{
				return this._createTeamCommandAvailable;
			}
			set
			{
				if (this._createTeamCommandAvailable != value)
				{
					this._createTeamCommandAvailable = value;
					base.RaisePropertyChanged("CreateTeamCommandAvailable");
				}
			}
		}

		public ICommand CreateTeamCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._createTeamCommand) == null)
				{
					relayCommand = (this._createTeamCommand = new RelayCommand(delegate(object param)
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.CreateTeamEvent());
						Helpers.OpenUrl(AccountManager.Instance.GetCreateTeamURL());
					}));
				}
				return relayCommand;
			}
		}

		public ICommand RefreshProfileCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._refreshProfileCommand) == null)
				{
					relayCommand = (this._refreshProfileCommand = new RelayCommand(delegate(object param)
					{
						ServiceContainer.Instance.GetService<IAccountManager>().FetchUserProfileInfoAsync();
					}));
				}
				return relayCommand;
			}
		}

		private UserProfileInfo _userProfileInfo;

		private bool _hasWarning;

		private bool _shareCommandAvailable;

		private Window _shareWindow;

		private RelayCommand _shareCommand;

		private RelayCommand _openProfileCommand;

		private bool _createTeamCommandAvailable;

		private RelayCommand _createTeamCommand;

		private RelayCommand _refreshProfileCommand;
	}
}
