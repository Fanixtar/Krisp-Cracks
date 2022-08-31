using System;
using System.Windows.Input;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Interops;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.TestKrisp.ViewModels
{
	public class RecorderViewModel : DeviceViewModel
	{
		public bool SPInitialized
		{
			get
			{
				return this._spInitialized;
			}
			set
			{
				if (value != this._spInitialized)
				{
					this._spInitialized = value;
					base.RaisePropertyChanged("SPInitialized");
				}
			}
		}

		public bool RecordingInProgress
		{
			get
			{
				return this._recordingInProgress;
			}
			set
			{
				if (value != this._recordingInProgress)
				{
					this._recordingInProgress = value;
					base.RaisePropertyChanged("RecordingInProgress");
				}
			}
		}

		public string TimerTimeSpan
		{
			get
			{
				return this._timerTimeSpan.ToString("mm\\:ss");
			}
			set
			{
				if (value != this._timerTimeSpan.ToString())
				{
					this._timerTimeSpan = TimeSpan.Parse(value);
					base.RaisePropertyChanged("TimerTimeSpan");
				}
			}
		}

		private IMMDevice SelectedMic { get; set; }

		private uint SPSessionId { get; } = DataModelFactory.SPInstance.GenerateRecordingSessionId();

		private bool SessionReleased { get; set; }

		private IAccountManager AccountMngr { get; } = ServiceContainer.Instance.GetService<IAccountManager>();

		private Logger Logger { get; } = LogWrapper.GetLogger("TestNoiseCancellation");

		public RecorderViewModel(string sourceSoundPath, string beforeNCSoundPath, string afterNCSoundPath)
		{
			this.Logger.LogInfo("Initializing Recorder");
			this._sourceSoundPath = sourceSoundPath;
			this._beforeNCSoundPath = beforeNCSoundPath;
			this._afterNCSoundPath = afterNCSoundPath;
			this._timer = new DispatcherTimer();
			this._timer.Tick += this.TimerTick;
			this._timer.Interval = new TimeSpan(0, 0, 1);
			this._timerTimeSpan = TimeSpan.FromSeconds(this._timerSeconds);
			this.SPInitialized = DataModelFactory.SPInstance.IsInitialized;
			if (!this.SPInitialized)
			{
				DataModelFactory.SPInstance.SPInitializedNotification += delegate(object sender, bool e)
				{
					this.SPInitialized = e;
				};
			}
			DataModelFactory.SPInstance.SPOutboundNotification += this.SPOutboundNotification;
			this.SessionReleased = true;
		}

		public event EventHandler RecordCompleted;

		public event EventHandler<string> Error;

		public ICommand StartRecordingCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._startRecordingCommand) == null)
				{
					relayCommand = (this._startRecordingCommand = new RelayCommand(delegate(object param)
					{
						this.Logger.LogInfo("Start Recording");
						if (!this.SessionReleased)
						{
							return;
						}
						this.SelectedMic = this.DetermineWorkingDevice();
						if (this.SelectedMic == null)
						{
							return;
						}
						SPFeature spfeature = SPFeature.Feature_NoiseClean;
						if (this.AccountMngr.IsLoggedIn)
						{
							try
							{
								BaseProfileSetting room_echo = this.AccountMngr.UserProfileInfo.settings.nc_out.room_echo;
								if (room_echo != null && room_echo.available && Settings.Default.MicrophoneRoomModeOn)
								{
									spfeature = SPFeature.Feature_Dereverb;
								}
							}
							catch (Exception ex)
							{
								this.Logger.LogWarning("Failed to get user-chosen NC mode for testNoiseCancellation. Moving on with default NoiseClean feature. exception: {0}", new object[] { ex.Message });
							}
						}
						IAudioDevice audioDevice = new AudioDevice(AudioDeviceKind.Microphone, this.SelectedMic);
						DataModelFactory.SPInstance.RecordSession(EnStreamDirection.Microphone, this.SPSessionId, spfeature, audioDevice.DefaultWaveFormat.nSamplesPerSec, this.SelectedMic.GetId(), this._sourceSoundPath, this._beforeNCSoundPath, this._afterNCSoundPath);
						this.SessionReleased = false;
					}));
				}
				return relayCommand;
			}
		}

		public ICommand StopRecordingCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._stopRecordingCommand) == null)
				{
					relayCommand = (this._stopRecordingCommand = new RelayCommand(delegate(object param)
					{
						if (this.SessionReleased)
						{
							return;
						}
						IAudioDevice audioDevice = new AudioDevice(AudioDeviceKind.Microphone, this.SelectedMic);
						this.Logger.LogInfo("Stop Recording. Recorded {0} seconds", new object[] { Convert.ToUInt32(this._timerMaxSeconds - this._timerSeconds) });
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispRecEndEvent(audioDevice.DisplayName, Convert.ToUInt32(this._timerMaxSeconds - this._timerSeconds)));
						this._timer.Stop();
						DataModelFactory.SPInstance.SessionRelease(EnStreamDirection.Microphone, this.SPSessionId);
						this.SessionReleased = true;
					}));
				}
				return relayCommand;
			}
		}

		private void TimerTick(object sender, EventArgs e)
		{
			double num = this._timerSeconds - 1.0;
			this._timerSeconds = num;
			this.TimerTimeSpan = TimeSpan.FromSeconds(num).ToString();
			if (this._timerSeconds == 0.0)
			{
				this.StopRecordingCommand.Execute(null);
			}
		}

		private void SPOutboundNotification(object sender, SPMessage msg)
		{
			if (msg.sesId == this.SPSessionId)
			{
				this._dispatcher.InvokeAsync(delegate()
				{
					this.SPStreamNotify(msg.sesId, msg.notifyCode, msg.hr, msg.Message);
				});
			}
		}

		private void SPStreamNotify(uint sesId, SPNotificationType notifyCode, HRESULT hr, string msg)
		{
			this.Logger.LogDebug("Received SP notification. reason: {0}, HR: {1}, Msg: {2}", new object[] { notifyCode, hr, msg });
			if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_DISCONNECTED)
			{
				if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_START_FAILED && notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_DISCONNECTED)
				{
					goto IL_111;
				}
			}
			else
			{
				if (notifyCode == SPNotificationType.SP_NOTIFICATION_OUT_STARTED)
				{
					this._timerSeconds = this._timerMaxSeconds;
					this.TimerTimeSpan = TimeSpan.FromSeconds(this._timerSeconds).ToString();
					this._timer.Start();
					this.RecordingInProgress = true;
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispRecStartEvent());
					return;
				}
				if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STOPPED)
				{
					if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_MISSING)
					{
						goto IL_111;
					}
				}
				else
				{
					EventHandler recordCompleted = this.RecordCompleted;
					if (recordCompleted == null)
					{
						return;
					}
					recordCompleted(this, null);
					return;
				}
			}
			this.Logger.LogError("Recording failed. SP reason: {0}, HR: {1}, Msg: {2}", new object[] { notifyCode, hr, msg });
			EventHandler<string> error = this.Error;
			if (error == null)
			{
				return;
			}
			error(this, "Recording fail due to stream processor error.");
			return;
			IL_111:
			this.Logger.LogError("Unhandled notification from SP reason: {0}, HR: {1}, Msg: {2}", new object[] { notifyCode, hr, msg });
		}

		private IMMDevice DetermineWorkingDevice()
		{
			IMMDevice immdevice = null;
			try
			{
				IMMDeviceEnumerator immdeviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
				string lastSelectedMic = Settings.Default.LastSelectedMic;
				if (this.AccountMngr.IsLoggedIn && lastSelectedMic != "Default_Device")
				{
					try
					{
						immdevice = immdeviceEnumerator.GetDevice(lastSelectedMic);
					}
					catch
					{
					}
				}
				if (immdevice == null || !immdevice.GetState().HasFlag(DeviceState.ACTIVE))
				{
					IKrispController krispController = DataModelFactory.KrispController(AudioDeviceKind.Microphone);
					immdevice = immdeviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eMultimedia);
					if (immdevice != null)
					{
						string id = immdevice.GetId();
						string text;
						if (krispController == null)
						{
							text = null;
						}
						else
						{
							IAudioDeviceManager deviceManager = krispController.DeviceManager;
							text = ((deviceManager != null) ? deviceManager.KrispDevId : null);
						}
						if (id == text)
						{
							immdevice = immdeviceEnumerator.GetDevice(krispController.WorkingDevice.Id);
						}
					}
				}
			}
			catch (Exception ex) when (ex.Is(-2147023728))
			{
				this.Logger.LogWarning("Couldn't determine device. Exception: {0}", new object[] { ex.Message });
			}
			catch (Exception ex2)
			{
				this.Logger.LogError("Error: {0}", new object[] { ex2.Message });
			}
			if (immdevice == null)
			{
				this.Logger.LogWarning("No device detected for recording.");
				EventHandler<string> error = this.Error;
				if (error != null)
				{
					error(this, "No device detected for recording.");
				}
			}
			else
			{
				this.Logger.LogInfo("{0} device chosen for recording.", new object[] { immdevice.GetId() });
			}
			return immdevice;
		}

		public override void Destroy()
		{
			if (this.RecordingInProgress)
			{
				this._timer.Stop();
				if (!this.SessionReleased)
				{
					DataModelFactory.SPInstance.SessionRelease(EnStreamDirection.Microphone, this.SPSessionId);
				}
			}
			DataModelFactory.SPInstance.SPOutboundNotification -= this.SPOutboundNotification;
		}

		private RelayCommand _startRecordingCommand;

		private RelayCommand _stopRecordingCommand;

		private bool _recordingInProgress;

		private bool _spInitialized;

		private double _timerSeconds;

		private readonly double _timerMaxSeconds = 15.0;

		private TimeSpan _timerTimeSpan;

		private readonly DispatcherTimer _timer;

		private readonly string _sourceSoundPath;

		private readonly string _beforeNCSoundPath;

		private readonly string _afterNCSoundPath;

		private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
	}
}
