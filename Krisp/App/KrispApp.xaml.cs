using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using CommandLine;
using Krisp.Analytics;
using Krisp.App.SysTray;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using Krisp.Properties;
using Krisp.Services;
using Krisp.UI;
using Krisp.UI.ViewModels;
using Krisp.UI.Views.Windows;
using Microsoft.Win32;
using MVVMFoundation;
using P7;
using Sentry;
using Shared.Analytics;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.App
{
	public partial class KrispApp : Application
	{
		private KrispApp()
		{
			this.InitializeComponent();
		}

		protected override void OnStartup(StartupEventArgs se)
		{
			base.OnStartup(se);
			bool IsSilent = false;
			bool IsInTestMode = false;
			bool IsFromMSI = false;
			if (se.Args.Length != 0)
			{
				Parser.Default.ParseArguments<CmdOptions>(se.Args).WithParsed(delegate(CmdOptions o)
				{
					IsSilent = o.Silent;
					IsInTestMode = o.TestMode;
					IsFromMSI = o.FromMSI;
				});
			}
			KrispApp._logger = LogWrapper.GetLogger("Main");
			bool flag = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
			KrispApp._logger.LogInfo("Krisp v{0} started on Windows {1} ({2}) {3}, environment: {4}, elevation: {5}, hypervisor: {6}, silent: {7} # {8}", new object[]
			{
				EnvHelper.KrispVersion.ToString(),
				SystemInfo.WinReleaseID,
				SystemInfo.WinVersion,
				Environment.Is64BitOperatingSystem ? "x64" : "x86",
				RunModeChecker.Mode.ToString(),
				flag ? "yes" : "none",
				SPInterop.IsRunningOnVM() ? "true" : "false",
				IsSilent ? "true" : "false",
				InstallationID.ID
			});
			GarbageCleaner.Clean();
			ServiceInjector.InjectServices();
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.AppRunEvent());
			if (!AudioEngineHelper.IsDuckingDisabled() && !Settings.Default.DuckingDisabled)
			{
				if (AudioEngineHelper.SetDuckingMode(AudioEngineHelper.DuckingMode.Do_nothing))
				{
					Settings.Default.DuckingDisabled = true;
					Settings.Default.Save();
					KrispApp._logger.LogInfo("Audio ducking disabled");
				}
				else
				{
					KrispApp._logger.LogWarning("Unable to disable audio ducking feature");
				}
			}
			if (Settings.Default.FirstRun)
			{
				Krisp.AppHelper.Startup.RunOnStartup();
			}
			this._krispWindow = new KrispWindow();
			this._sysTryIcon = new SysTryIcon(this._krispWindow);
			this._sysTryIcon.DataContext = new SysTryIconViewModel();
			ServiceContainer.Instance.AddService<IMessageNotifierService>(new MessageNotifierService(delegate(INotification notification)
			{
				this._sysTryIcon.PushNotification(notification);
			}));
			DataModelFactory.SPInstance.SPInitialize();
			KrispSDKFactory.Instance.ProxyCredentialsPrompt = new ProxyCredentialsPrompt();
			this._krispWindowViewModel = new KrispWindowViewModel(IsFromMSI);
			this._krispWindow.DataContext = this._krispWindowViewModel;
			ServiceContainer.Instance.GetService<IAccountManager>().Initialize();
			if (IsInTestMode)
			{
				this._krispWindow.AutomaticalyHide = false;
			}
			SystemEvents.SessionSwitch += this.SystemEvents_SessionSwitch;
			SystemEvents.PowerModeChanged += this.SystemEvents_PowerModeChanged;
			if (!IsSilent)
			{
				this._krispWindow.Show();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Settings.Default.FirstRun = false;
			try
			{
				Settings.Default.Save();
			}
			catch
			{
			}
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.AppQuitEvent());
			this._sysTryIcon.UnregisterEvents();
			SystemEvents.PowerModeChanged -= this.SystemEvents_PowerModeChanged;
			SystemEvents.SessionSwitch -= this.SystemEvents_SessionSwitch;
			this.UnLoadKrispInternals("OnExit");
			this._sysTryIcon.Hide();
			this.finalySPRelease();
			KrispApp._logger.LogInfo("Exit App.");
			base.OnExit(e);
		}

		protected void finalySPRelease()
		{
			int num;
			while ((num = DataModelFactory.SPInstance.SPRelease()) > 0)
			{
				KrispApp._logger.LogWarning(string.Format("SPRelease {0}.", num));
			}
		}

		private static void TaskSchedulerUnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs evArgs)
		{
			using (SentrySdk.Init("https://d490cb1fc1fb41cd87fb7c8345a7674c@sentry.io/1430452"))
			{
				try
				{
					SentrySdk.ConfigureScope(delegate(Scope scope)
					{
						scope.Environment = RunModeChecker.Mode.ToString();
						scope.SetTag("installation_id", InstallationID.ID);
						scope.User.IpAddress = "{{auto}}";
					});
				}
				catch
				{
				}
				SentrySdk.CaptureException(evArgs.Exception);
			}
		}

		private static void InitializeLogger()
		{
			Traces.Level level = (Traces.Level)Math.Min(Settings.Default.LogLevel, 5U);
			LogWrapper.Init(EnvHelper.KrispAppLogFolder, level);
			LogWrapper.ShareClient("StreamsProcessor");
		}

		private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			try
			{
				KrispApp._logger.LogInfo("SystemEvents_SessionSwitch with: {0}", new object[] { e.Reason });
				SessionSwitchReason reason = e.Reason;
				if (reason != SessionSwitchReason.ConsoleConnect)
				{
					if (reason != SessionSwitchReason.ConsoleDisconnect)
					{
						if (reason != SessionSwitchReason.SessionUnlock)
						{
							KrispApp._logger.LogDebug("The following SesseionSwitchReason has been caught: " + e.Reason + " , No action!");
						}
						else
						{
							IAccountManager service = ServiceContainer.Instance.GetService<IAccountManager>();
							if (service.WorkingMode == WorkingMode.Stopped && service.State != AccountManagerState.Uninitialized)
							{
								service.Resume();
							}
						}
					}
					else
					{
						this.UnLoadKrispInternals("SystemEvents_SessionSwitch");
					}
				}
				else
				{
					this.LoadKrispInternals("SystemEvents_SessionSwitch");
				}
			}
			catch (Exception ex)
			{
				KrispApp._logger.LogError("Something bad happened while managing session switching events", new object[] { ex });
			}
		}

		private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			KrispApp._logger.LogInfo("SystemEvents_PowerModeChanged with: {0}", new object[] { e.Mode });
			PowerModes mode = e.Mode;
			if (mode == PowerModes.Suspend)
			{
				ServiceContainer.Instance.GetService<IAccountManager>().Stop();
			}
		}

		internal void LoadKrispInternals([CallerMemberName] string callerName = "")
		{
			KrispApp._logger.LogInfo(callerName + ", LoadKrispInternals start");
			ServiceContainer.Instance.GetService<IAccountManager>().Resume();
			if (ServiceContainer.Instance.GetService<IAccountManager>().IsLoggedIn)
			{
				KrispWindowViewModel krispWindowViewModel = this._krispWindowViewModel;
				if (krispWindowViewModel != null)
				{
					krispWindowViewModel.ChangeKrispAppState(true, false);
				}
			}
			KrispApp._logger.LogInfo(callerName + ", LoadKrispInternals done");
		}

		internal void UnLoadKrispInternals([CallerMemberName] string callerName = "")
		{
			KrispApp._logger.LogInfo(callerName + ", UnLoadKrispInternals start");
			KrispWindowViewModel krispWindowViewModel = this._krispWindowViewModel;
			if (krispWindowViewModel != null)
			{
				krispWindowViewModel.ChangeKrispAppState(false, false);
			}
			ServiceContainer.Instance.GetService<IAccountManager>().Stop();
			this.finalySPRelease();
			KrispApp._logger.LogInfo(callerName + ", UnLoadKrispInternals done");
		}

		private const string _sentryURL = "https://d490cb1fc1fb41cd87fb7c8345a7674c@sentry.io/1430452";

		private static Logger _logger;

		private SysTryIcon _sysTryIcon;

		private KrispWindow _krispWindow;

		private KrispWindowViewModel _krispWindowViewModel;
	}
}
