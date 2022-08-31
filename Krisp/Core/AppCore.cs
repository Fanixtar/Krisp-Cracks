using System;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using Krisp.Services;
using Krisp.SysTray.Notifications;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.Core
{
	public class AppCore : DisposableBase, IAppCore
	{
		public IKrispController SpeakerController
		{
			get
			{
				return this.SpkController;
			}
		}

		public IKrispController MicController { get; private set; }

		public IKrispController SpkController { get; private set; }

		public AppCore()
		{
			this._logger.LogInfo("+++ AppCore-AudioEngine");
			this._dispatcher = Dispatcher.CurrentDispatcher;
			StreamProcessor instance = StreamProcessor.Instance;
			if (instance.IsInitialized)
			{
				this.OnStreamsProcessorInitialized(this, true);
			}
			else
			{
				instance.SPInitializedNotification += this.OnStreamsProcessorInitialized;
			}
			instance.SPErrorNotification += this.OnSPGeneralError;
			instance.SPStreamDucked += this.OnStreamDucked;
			bool? flag = null;
			if (string.Compare(Settings.Default.KrispSpeakerAsSystemDefault, bool.TrueString, true) == 0)
			{
				flag = new bool?(true);
			}
			else if (string.Compare(Settings.Default.KrispSpeakerAsSystemDefault, bool.FalseString, true) == 0)
			{
				flag = new bool?(false);
			}
			this.SpkController = new KrispController(AudioDeviceKind.Speaker, flag);
			flag = null;
			if (string.Compare(Settings.Default.KrispMicrophoneAsSystemDefault, bool.TrueString, true) == 0)
			{
				flag = new bool?(true);
			}
			else if (string.Compare(Settings.Default.KrispMicrophoneAsSystemDefault, bool.FalseString, true) == 0)
			{
				flag = new bool?(false);
			}
			this.MicController = new KrispController(AudioDeviceKind.Microphone, flag);
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				StreamProcessor instance = StreamProcessor.Instance;
				instance.SPInitializedNotification -= this.OnStreamsProcessorInitialized;
				instance.SPErrorNotification -= this.OnSPGeneralError;
				instance.SPStreamDucked -= this.OnStreamDucked;
				if (this.SpkController != null)
				{
					this.SpkController.Dispose();
				}
				this.SpkController = null;
				if (this.MicController != null)
				{
					this.MicController.Dispose();
				}
				this.MicController = null;
				DataModelFactory.DestroyKrispActivityNotificationClient();
				StreamProcessor.Instance.SPRelease();
			}
			this._disposed = true;
			base.Dispose(disposing);
		}

		public bool Initialize()
		{
			if (this._initialized)
			{
				return false;
			}
			this._initialized = true;
			StreamProcessor.Instance.SPInitialize();
			this.ScanStates();
			if (StreamProcessor.Instance.IsInitializationFailed)
			{
				this.OnSPGeneralError(this, null);
			}
			return true;
		}

		private HRESULT ScanStates()
		{
			HRESULT hresult = 0;
			if (this.MicController != null)
			{
				hresult = this.MicController.ScanStates(null);
				hresult.LogOnHerror(this._logger, "scanStates: Unable to select Microphone device:");
			}
			if (this.SpkController != null)
			{
				hresult = this.SpkController.ScanStates(null);
				hresult.LogOnHerror(this._logger, "scanStates: Unable to select Speaker device:");
			}
			return hresult;
		}

		private void OnStreamsProcessorInitialized(object sender, bool inited)
		{
			if (inited)
			{
				this._dispatcher.InvokeAsync(delegate()
				{
					this.startProcessing(inited);
				});
			}
		}

		private void startProcessing(bool spInited)
		{
			this._logger.LogInfo("--- Start Processing ---");
			IKrispController spkController = this.SpkController;
			if (spkController != null)
			{
				spkController.OnNCSwitch(this, Settings.Default.SpeakerNCState);
			}
			IKrispController micController = this.MicController;
			if (micController != null)
			{
				micController.OnNCSwitch(this, Settings.Default.MicrophoneNCState);
			}
			try
			{
				if (string.IsNullOrWhiteSpace(Settings.Default.LastSelectedSpeaker))
				{
					Settings.Default.LastSelectedSpeaker = "Default_Device";
					Settings.Default.Save();
				}
				if (string.IsNullOrWhiteSpace(Settings.Default.LastSelectedMic))
				{
					Settings.Default.LastSelectedMic = "Default_Device";
					Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error on StoreDefaultDevs. {0}", new object[] { ex.Message });
			}
			if (this.SpkController == null || this.MicController == null)
			{
				this._logger.LogError("Error on startProcessing ...");
				return;
			}
			this.MicController.StartProcessing();
			this.SpkController.StartProcessing();
		}

		private void OnStreamDucked(object sender, bool e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				if (AudioEngineHelper.IsDuckingDisabled())
				{
					this._logger.LogInfo("Stream attenuated while ducked is disabled");
				}
				ServiceContainer.Instance.GetService<IMessageNotifierService>().NotifyMessage(new KrispDuckNotification());
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.DuckingNotifiedEvent());
			});
		}

		private void OnSPGeneralError(object snder, SPMessage msg)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.UnhealthyEvent());
				IKrispController micController = this.MicController;
				if (micController != null)
				{
					micController.NotifyError(ADMStateFlags.UnHealtyState);
				}
				IKrispController spkController = this.SpkController;
				if (spkController == null)
				{
					return;
				}
				spkController.NotifyError(ADMStateFlags.UnHealtyState);
			});
		}

		public string GetDiagnosticInfo()
		{
			throw new NotImplementedException();
		}

		private Logger _logger = LogWrapper.GetLogger("Core");

		private readonly Dispatcher _dispatcher;

		private bool _initialized;

		private bool _disposed;
	}
}
