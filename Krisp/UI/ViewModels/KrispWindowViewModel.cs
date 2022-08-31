using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using Krisp.Services;
using Krisp.UI.Models;
using Krisp.UI.Views;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	internal class KrispWindowViewModel : BindableBase
	{
		public bool DeviceMode { get; } = DeviceLoginHelper.DeviceMode;

		private IRelayCommandsService RelayCommandsService { get; } = ServiceContainer.Instance.GetService<IRelayCommandsService>();

		public ICommand AboutCommand
		{
			get
			{
				return this.RelayCommandsService.AboutCommand;
			}
		}

		public ICommand UpdateWindowCommand
		{
			get
			{
				return this.RelayCommandsService.UpdateWindowCommand;
			}
		}

		public ICommand ReportBugCommand
		{
			get
			{
				return this.RelayCommandsService.ReportBugCommand;
			}
		}

		public ICommand SetupKrispCommand
		{
			get
			{
				return this.RelayCommandsService.SetupKrispCommand;
			}
		}

		public ICommand TestNoiseCancellationCommand
		{
			get
			{
				return this.RelayCommandsService.TestNoiseCancellationCommand;
			}
		}

		public ICommand ContactSupportCommand
		{
			get
			{
				return this.RelayCommandsService.ContactSupportCommand;
			}
		}

		public ICommand PreferencesCommand
		{
			get
			{
				return new RelayCommand(delegate(object param)
				{
					if (this._preferencesWindow == null)
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.Preferences());
						this._preferencesWindow = new PreferencesWindow();
						this._preferencesWindow.Closed += this.PreferencesWindow_Closed;
						this._preferencesWindow.Show();
						return;
					}
					this._preferencesWindow.BringWindowToTop();
				});
			}
		}

		private void PreferencesWindow_Closed(object sender, EventArgs e)
		{
			this._preferencesWindow.Closed -= this.PreferencesWindow_Closed;
			this._preferencesWindow = null;
			KrispWindow krispWindow = (KrispWindow)Application.Current.MainWindow;
			if (krispWindow != null)
			{
				krispWindow.Show();
			}
		}

		public ICommand QuitCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._quitCommand) == null)
				{
					relayCommand = (this._quitCommand = new RelayCommand(delegate(object param)
					{
						this._krispAppPageViewModel.OnQuit();
						Application.Current.Shutdown();
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SignOutCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._signOutCommand) == null)
				{
					relayCommand = (this._signOutCommand = new RelayCommand(delegate(object param)
					{
						this._krispAppPageViewModel.OnSignOut();
						ServiceContainer.Instance.GetService<IAccountManager>().LogoutAsync();
					}));
				}
				return relayCommand;
			}
		}

		public ICommand OpenPricePlanCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._openPricePlanCommand) == null)
				{
					relayCommand = (this._openPricePlanCommand = new RelayCommand(delegate(object param)
					{
						ServiceContainer.Instance.GetService<IAccountManager>().FetchUserProfileInfoAsync();
						Helpers.OpenUrl(AccountManager.Instance.GetUnlockURL());
					}));
				}
				return relayCommand;
			}
		}

		public IPageViewModel CurrentPageViewModel
		{
			get
			{
				return this._currentPageViewModel;
			}
		}

		public KrispWindowViewModel(bool delay)
		{
			Mediator.Instance.Register(this);
			this._logger = LogWrapper.GetLogger("KrispWindow");
			this._dispatcher = Dispatcher.CurrentDispatcher;
			this._acmService = ServiceContainer.Instance.GetService<IAccountManager>();
			this._acmService.CheckForUpdateAsync(false);
			this._krispAppPageViewModel = new KrispAppPageViewModel();
			this._pageViewModels = new Dictionary<PageViews, IPageViewModel>
			{
				{
					PageViews.KrispAppPage,
					this._krispAppPageViewModel
				},
				{
					PageViews.SignInPage,
					new SignInPageViewModel()
				},
				{
					PageViews.PostSignInPage,
					new PostSignInPageViewModel()
				},
				{
					PageViews.ProgressPage,
					new ProgressPageViewModel()
				},
				{
					PageViews.GenericPage,
					new GenericPageViewModel()
				}
			};
			this._acmService.StateChanged += this._acmService_StateChanged;
			this._acmService.UserProfileInfoChanged += this.UserProfileInfoChanged;
			this.ChangeViewModel(PageViews.ProgressPage, null);
			if (delay)
			{
				this._elapsed = false;
				this.DelayStart();
			}
			NetworkHelper.Instance.NetworkConnectionRestored += this.OnNetworkConnectionRestored;
		}

		private void UserProfileInfoChanged(object sender, UserProfileInfo e)
		{
			if (e != null)
			{
				Dispatcher dispatcher = this._dispatcher;
				if (dispatcher == null)
				{
					return;
				}
				dispatcher.InvokeAsync(delegate()
				{
					Mediator.Instance.NotifyColleagues<UserProfileInfo>("ProfileInfoChanged", e);
				});
			}
		}

		private void _acmService_StateChanged(object sender, AccountManagerStateChangedEventArgs args)
		{
			Dispatcher dispatcher = this._dispatcher;
			if (dispatcher == null)
			{
				return;
			}
			Action <>9__1;
			dispatcher.Invoke(delegate()
			{
				this._krispAppPageViewModel.HeaderViewModel.HasWarning = false;
				AccountManagerStateChangedEventArgs args2 = args;
				AccountManagerState? accountManagerState = ((args2 != null) ? new AccountManagerState?(args2.State) : null);
				if (accountManagerState != null)
				{
					switch (accountManagerState.GetValueOrDefault())
					{
					case AccountManagerState.LoggedIn:
						this.ChangeKrispAppState(true, false);
						this.ChangePageViewModel(PageViews.KrispAppPage);
						return;
					case AccountManagerState.LoggingIn:
						if (DeviceLoginHelper.DeviceMode)
						{
							this.ChangePageViewModel(PageViews.ProgressPage);
							return;
						}
						this.ChangePageViewModel(PageViews.GenericPage, ModelFactory.LoggingInModel());
						return;
					case AccountManagerState.LoggedOut:
						this.ChangeKrispAppState(false, false);
						this.ChangePageViewModel(PageViews.SignInPage);
						return;
					case AccountManagerState.NoInternetConnection:
					{
						Dispatcher dispatcher2 = this._dispatcher;
						if (dispatcher2 != null)
						{
							Action action;
							if ((action = <>9__1) == null)
							{
								action = (<>9__1 = delegate()
								{
									this._acmService.NoInternetConnectionPolling();
								});
							}
							dispatcher2.InvokeAsync(action);
						}
						NetworkHelper.Instance.StartMonitoring();
						if (!this._acmService.IsLoggedIn)
						{
							this.ChangePageViewModel(PageViews.GenericPage, ModelFactory.NoInternetConnectionModel());
							return;
						}
						this._krispAppPageViewModel.HeaderViewModel.HasWarning = true;
						this._logger.LogWarning("NoInternetConnection received from AccountManager. If connection doesn't recover for some time, NC features will be disabled");
						return;
					}
					case AccountManagerState.GeneralError:
						this.ChangePageViewModel(PageViews.GenericPage, ModelFactory.GenericPageModel(args));
						return;
					}
				}
				this.ChangePageViewModel(PageViews.GenericPage, ModelFactory.GenericPageModel(args));
			});
		}

		private void DelayStart()
		{
			this._dispatcher.InvokeAsync<Task>(async delegate()
			{
				await Task.Run(async delegate()
				{
					await Task.Delay(KrispWindowViewModel.DELAY_START_TIMEOUT).ContinueWith(delegate(Task <p0>)
					{
						this._elapsed = true;
						this.ChangeViewModel(this._delayedPage, this._delayedPageModel);
					});
				});
			});
		}

		private void ChangeViewModel(PageViews view, GenericPageModel genericPageModel)
		{
			view = this.Guard_CanNavigate(view);
			if (this._pageViewModels.Keys.Contains(view))
			{
				if (!this._elapsed && view != PageViews.ProgressPage)
				{
					this._delayedPage = view;
					this._delayedPageModel = genericPageModel;
					return;
				}
				IPageViewModel value = this._pageViewModels.FirstOrDefault((KeyValuePair<PageViews, IPageViewModel> vm) => vm.Key == view).Value;
				this._currentPageViewModel = value;
				if (genericPageModel != null)
				{
					(this.CurrentPageViewModel as IGenericPageViewModel).SetModel(genericPageModel);
				}
				base.RaisePropertyChanged("CurrentPageViewModel");
			}
		}

		private void ChangePageViewModel(PageViews view, GenericPageModel genericPageModel)
		{
			Dispatcher dispatcher = this._dispatcher;
			if (dispatcher == null)
			{
				return;
			}
			dispatcher.InvokeAsync(delegate()
			{
				this.ChangeViewModel(view, genericPageModel);
			});
		}

		[MediatorMessageSink("SelectPageViewModel")]
		private void ChangePageViewModel(PageViews view)
		{
			this.ChangeViewModel(view, null);
		}

		private PageViews Guard_CanNavigate(PageViews view)
		{
			PageViews pageViews = view;
			if (view == PageViews.KrispAppPage)
			{
				if (!this._acmService.IsLoggedIn)
				{
					pageViews = PageViews.SignInPage;
				}
				else if (Settings.Default.DemoMode)
				{
					pageViews = PageViews.PostSignInPage;
				}
			}
			return pageViews;
		}

		private void OnNetworkConnectionRestored(object sender, EventArgs e)
		{
			if (this._acmService.State == AccountManagerState.NoInternetConnection)
			{
				this._logger.LogInfo("OnNetworkConnectionRestored.");
				this._acmService.RecoverAfterSyncError();
			}
		}

		private void applyProfileSettings(ProfileSettings settings)
		{
			if (settings != null)
			{
				try
				{
					bool flag = false;
					StateOnlySetting krisp_mic_as_default = settings.nc_out.krisp_mic_as_default;
					if (((krisp_mic_as_default != null) ? krisp_mic_as_default.state : null) != null)
					{
						string text = settings.nc_out.krisp_mic_as_default.state;
						if (!(text == "always"))
						{
							if (!(text == "never"))
							{
								if (text == "none")
								{
									Settings.Default.KrispMicrophoneAsSystemDefault = "none";
									flag = true;
								}
							}
							else
							{
								Settings.Default.KrispMicrophoneAsSystemDefault = bool.FalseString;
								flag = true;
							}
						}
						else
						{
							Settings.Default.KrispMicrophoneAsSystemDefault = bool.TrueString;
							flag = true;
						}
					}
					StateOnlySetting krisp_speaker_as_default = settings.nc_out.krisp_speaker_as_default;
					if (((krisp_speaker_as_default != null) ? krisp_speaker_as_default.state : null) != null)
					{
						string text = settings.nc_out.krisp_mic_as_default.state;
						if (!(text == "always"))
						{
							if (!(text == "never"))
							{
								if (text == "none")
								{
									Settings.Default.KrispSpeakerAsSystemDefault = "none";
									flag = true;
								}
							}
							else
							{
								Settings.Default.KrispSpeakerAsSystemDefault = bool.FalseString;
								flag = true;
							}
						}
						else
						{
							Settings.Default.KrispSpeakerAsSystemDefault = bool.TrueString;
							flag = true;
						}
					}
					if (flag)
					{
						Settings.Default.Save();
					}
				}
				catch (Exception ex)
				{
					this._logger.LogError("Error on storing ProfileSettings. Error: {0}", new object[] { ex.Message });
				}
			}
		}

		public void ChangeKrispAppState(bool load, bool fromMSI = false)
		{
			try
			{
				this._logger.LogInfo("AppCore {0}loading", new object[] { load ? "" : "un" });
				if (load && this._acmService.IsLoggedIn)
				{
					this.applyProfileSettings(this._acmService.UserProfileInfo.settings);
					if (this._appCore == null)
					{
						this._appCore = DataModelFactory.CreateAppCore();
					}
					else
					{
						this._logger.LogDebug("appCoreExist.");
					}
					this._krispAppPageViewModel.AttachDevices();
					this._appCore.Initialize();
				}
				else
				{
					DataModelFactory.DestroyAppCore();
					this._appCore = null;
					this._krispAppPageViewModel.DeattachDevices();
				}
				this._logger.LogInfo("AppCore {0}loaded", new object[] { load ? "" : "un" });
			}
			catch (Exception ex)
			{
				this._logger.LogError("ChangeKrispAppState failed: {0}", new object[] { ex });
				throw;
			}
		}

		private static int DELAY_START_TIMEOUT = 5000;

		private IPageViewModel _currentPageViewModel;

		private Dictionary<PageViews, IPageViewModel> _pageViewModels;

		private IAccountManager _acmService;

		private KrispAppPageViewModel _krispAppPageViewModel;

		private IAppCore _appCore;

		private Dispatcher _dispatcher;

		private Logger _logger;

		private bool _elapsed = true;

		private PageViews _delayedPage = PageViews.ProgressPage;

		private GenericPageModel _delayedPageModel;

		private Window _preferencesWindow;

		private RelayCommand _quitCommand;

		private RelayCommand _signOutCommand;

		private RelayCommand _openPricePlanCommand;
	}
}
