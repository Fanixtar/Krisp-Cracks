using System;
using System.Text;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.Properties;
using Shared.Analytics;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class AudioEngine : AudioEngineStateMachine, IDiagnosticsBase
	{
		public string InstanceName
		{
			get
			{
				return string.Format("{0}AudioEngine", this._kind);
			}
		}

		public bool DevicesLoaded { get; private set; }

		public KAudioSession CurrentSession { get; private set; }

		private bool isSPInitialized
		{
			get
			{
				return StreamProcessor.Instance.IsInitialized;
			}
		}

		private bool _NCState { get; set; }

		private SPFeature _NCFeature { get; set; } = SPFeature.Feature_NoiseClean;

		public AudioEngine(AudioDeviceKind kind)
		{
			this._kind = kind;
			this._logger = LogWrapper.GetLogger(string.Format("AudioEngine ({0})", this._kind));
			this._sesCounter = (uint)((AudioDeviceKind)1000000 * (this._kind + 1));
			StreamProcessor instance = StreamProcessor.Instance;
			if (kind == AudioDeviceKind.Speaker)
			{
				instance.SPInboundNotification += this.OnSPNotify;
			}
			else
			{
				instance.SPOutboundNotification += this.OnSPNotify;
			}
			this._krispControlStatus = DataModelFactory.CreateKrispControlStatus(this._kind);
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
				if (this._kind == AudioDeviceKind.Speaker)
				{
					instance.SPInboundNotification -= this.OnSPNotify;
				}
				else
				{
					instance.SPOutboundNotification -= this.OnSPNotify;
				}
				DataModelFactory.DestroyKrispActivityNotificationClient();
				KrispAudioDevice krispDev = this._krispDev;
				if (krispDev != null)
				{
					krispDev.Dispose();
				}
				IKrispControlStatus krispControlStatus = this._krispControlStatus;
				if (krispControlStatus != null)
				{
					krispControlStatus.SetStreamActivityStatus(false);
				}
				this._krispControlStatus = null;
				if (this.CurrentSession != null)
				{
					this.StopSession(this.CurrentSession.SessionId);
					this.CurrentSession.Dispose();
				}
			}
			this._disposed = true;
			base.Dispose(disposing);
		}

		public void ChangeStFlag(ADMStateFlags st = ADMStateFlags.HealtyState)
		{
			IKrispControlStatus krispControlStatus = this._krispControlStatus;
			if (krispControlStatus == null)
			{
				return;
			}
			krispControlStatus.ChangeStFlag(st);
		}

		public HRESULT StartSession(IAudioDevice selDev, bool reset = false)
		{
			HRESULT hresult = 1;
			if (selDev == null || this._krispDev == null)
			{
				this._logger.LogWarning("{0} {1}  {2}", new object[]
				{
					reset ? "resetSession" : "",
					(selDev == null) ? "the SelectedDevice is null" : "",
					(this._krispDev == null) ? "the KrispDevice is null" : ""
				});
				return hresult;
			}
			if (this.isSPInitialized && this.DevicesLoaded)
			{
				try
				{
					if (!reset && (base.Machine.IsInState(AudioEngineStateMachine.EngineState.SPSessionStarted) || base.Machine.IsInState(AudioEngineStateMachine.EngineState.SPSessionStarting)))
					{
						KAudioSession currentSession = this.CurrentSession;
						string text;
						if (currentSession == null)
						{
							text = null;
						}
						else
						{
							IAudioDevice usedDevice = currentSession.UsedDevice;
							text = ((usedDevice != null) ? usedDevice.Id : null);
						}
						if (text == selDev.Id)
						{
							return 1;
						}
					}
					bool flag = false;
					if (this.CurrentSession != null)
					{
						flag = this.CurrentSession.KState == KStreamSessionState.KSessionStarted;
						hresult = this.StopSession(this.CurrentSession.SessionId);
						this.CurrentSession.Dispose();
					}
					SDKModel sdkmodel = StreamProcessor.Instance.ModelManager.FindModel((EnStreamDirection)this._kind, this._NCFeature, selDev.DefaultWaveFormat.nSamplesPerSec);
					this.CurrentSession = new KAudioSession(this._krispDev, selDev, this._NCState, this.newSessionID(), this._krispDev, sdkmodel.modelName);
					hresult = StreamProcessor.Instance.SessionSetDevice((EnStreamDirection)this._kind, this.CurrentSession.SessionId, sdkmodel.modelName, selDev.Id);
					hresult.LogOnHerror(this._logger, string.Format("StartSession: Unable to select {0} device: {1} - {2}.", this._kind, selDev.Id, selDev.DisplayName));
					if (hresult == 0)
					{
						if (flag)
						{
							this.CurrentSession.SetStreamStarted(this._NCState);
						}
						this._logger.LogDebug("StartSession: sessionID: {0}, model: {1}, device: {2} - {3}", new object[]
						{
							this.CurrentSession.SessionId,
							(sdkmodel != null) ? sdkmodel.modelName : null,
							selDev.Id,
							selDev.DisplayName
						});
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPRequestedStart);
						this.TraceMachine();
					}
					return hresult;
				}
				catch (Exception ex)
				{
					this.TraceLine(ex);
					return hresult;
				}
			}
			this._logger.LogDebug("StartSession: devloaded: {0}, spinited: {1}, device: {2}", new object[] { this.DevicesLoaded, this.isSPInitialized, selDev.DisplayName });
			return hresult;
		}

		internal void FoundKrispDevice(KrispAudioDevice krispDev)
		{
			if (this._krispDev != null)
			{
				this._krispDev.StreamActivityChanged -= this.KrispStreamActivityChanged;
				this._krispDev.Dispose();
			}
			this._krispDev = krispDev;
			this._krispDev.StreamActivityChanged += this.KrispStreamActivityChanged;
		}

		private void KrispStreamActivityChanged(object sender, StreamActivityState state)
		{
			Logger logger = this._logger;
			string text = "StreamActivityChanged # sesId: {0}, state: {1}, state: {2}";
			object[] array = new object[3];
			int num = 0;
			KAudioSession currentSession = this.CurrentSession;
			array[num] = ((currentSession != null) ? new uint?(currentSession.SessionId) : null);
			int num2 = 1;
			KAudioSession currentSession2 = this.CurrentSession;
			array[num2] = ((currentSession2 != null) ? new KStreamSessionState?(currentSession2.KState) : null);
			array[2] = state;
			logger.LogDebug(text, array);
			switch (state)
			{
			case StreamActivityState.StreamClosed:
				break;
			case StreamActivityState.StreamOpened:
			{
				KAudioSession currentSession3 = this.CurrentSession;
				if (currentSession3 != null && currentSession3.KState == KStreamSessionState.KSessionStarted)
				{
					this._logger.LogWarning("Got stream open event in wrong state # sesId: {0}, state: {1}", new object[]
					{
						this.CurrentSession.SessionId,
						this.CurrentSession.KState
					});
					this.SetActivityState(false);
					return;
				}
				return;
			}
			case StreamActivityState.StreamClosed | StreamActivityState.StreamOpened:
				return;
			case StreamActivityState.StreamStarted:
			{
				this.SetActivityState(true);
				IKrispControlStatus krispControlStatus = this._krispControlStatus;
				if (krispControlStatus == null)
				{
					return;
				}
				krispControlStatus.SetStreamActivityStatus(true);
				return;
			}
			default:
				if (state != StreamActivityState.StreamStoped)
				{
					return;
				}
				break;
			}
			this.SetActivityState(false);
			IKrispControlStatus krispControlStatus2 = this._krispControlStatus;
			if (krispControlStatus2 == null)
			{
				return;
			}
			krispControlStatus2.SetStreamActivityStatus(false);
		}

		public HRESULT StopSession(uint sesID = 0U)
		{
			HRESULT hresult = 1;
			if (sesID == 0U)
			{
				sesID = ((this.CurrentSession != null) ? this.CurrentSession.SessionId : 0U);
			}
			if (sesID != 0U)
			{
				this.TraceLine(string.Format("Stop the Session with sId: {0}", sesID));
				hresult = StreamProcessor.Instance.SessionRelease((EnStreamDirection)this._kind, sesID);
			}
			return hresult;
		}

		public void SetDevicesLoaded()
		{
			if (Settings.Default.ActivityThreshold < 5U)
			{
				this._activityThreshold = 5U;
			}
			if (Settings.Default.ActivityThreshold > 60U)
			{
				this._activityThreshold = 60U;
			}
			KAudioSession.s_ActivityThreshold = this._activityThreshold;
			this.SetActivityState(false);
			this.DevicesLoaded = true;
		}

		public HRESULT NCModeChange(SPFeature feature)
		{
			HRESULT hresult = 0;
			this._logger.LogInfo(string.Format("{0} NC mode changed from {1} to {2}, result: {3}", new object[] { this._kind, this._NCFeature, feature, hresult }));
			this._NCFeature = feature;
			KAudioSession currentSession = this.CurrentSession;
			IAudioDevice audioDevice = ((currentSession != null) ? currentSession.UsedDevice : null);
			return this.StartSession(audioDevice, true);
		}

		public HRESULT NCSwitch(bool state)
		{
			if (this._NCState == state)
			{
				return 1;
			}
			HRESULT hresult = this.SetFeatureState(SPFeature.Feature_NoiseClean, state);
			this._logger.LogDebug(string.Format("{0} SPSwitch NC State: {1}, result: {2}", this._kind, state ? "On" : "Off", hresult));
			if (hresult)
			{
				this._NCState = state;
				if (this.CurrentSession != null)
				{
					this.CurrentSession.SetNCState(this._NCState);
				}
				Settings.Default[string.Format("{0}NCState", this._kind)] = state;
				try
				{
					Settings.Default.Save();
				}
				catch
				{
				}
			}
			return hresult;
		}

		public void OnSPNotify(object sender, SPMessage msg)
		{
			if (msg != null)
			{
				uint sesId = msg.sesId;
				KAudioSession currentSession = this.CurrentSession;
				uint? num = ((currentSession != null) ? new uint?(currentSession.SessionId) : null);
				if ((sesId == num.GetValueOrDefault()) & (num != null))
				{
					try
					{
						SPNotificationType notifyCode = msg.notifyCode;
						if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_STATS)
						{
							if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_STOPPED)
							{
								if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_DISCONNECTED)
								{
									if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_START_FAILED)
									{
										if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_DISCONNECTED)
										{
											goto IL_32E;
										}
										goto IL_233;
									}
								}
								else
								{
									if (notifyCode == SPNotificationType.SP_NOTIFICATION_IN_STARTED)
									{
										goto IL_2BC;
									}
									if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STOPPED)
									{
										goto IL_32E;
									}
									goto IL_301;
								}
							}
							else if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_STARTING)
							{
								if (notifyCode == SPNotificationType.SP_NOTIFICATION_IN_MISSING)
								{
									goto IL_35F;
								}
								if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STARTING)
								{
									goto IL_32E;
								}
								goto IL_2D3;
							}
							else
							{
								if (notifyCode == SPNotificationType.SP_NOTIFICATION_IN_RESETTING)
								{
									goto IL_2EA;
								}
								if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STATS)
								{
									goto IL_32E;
								}
								goto IL_318;
							}
						}
						else if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_STOPPED)
						{
							if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_DISCONNECTED)
							{
								if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_START_FAILED)
								{
									if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_DISCONNECTED)
									{
										goto IL_32E;
									}
									goto IL_233;
								}
							}
							else
							{
								if (notifyCode == SPNotificationType.SP_NOTIFICATION_OUT_STARTED)
								{
									goto IL_2BC;
								}
								if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STOPPED)
								{
									goto IL_32E;
								}
								goto IL_301;
							}
						}
						else if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_STARTING)
						{
							if (notifyCode == SPNotificationType.SP_NOTIFICATION_OUT_MISSING)
							{
								goto IL_35F;
							}
							if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STARTING)
							{
								goto IL_32E;
							}
							goto IL_2D3;
						}
						else
						{
							if (notifyCode == SPNotificationType.SP_NOTIFICATION_OUT_RESETTING)
							{
								goto IL_2EA;
							}
							if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STATS)
							{
								goto IL_32E;
							}
							goto IL_318;
						}
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationStartFailed);
						if (msg.hr == -2147024891)
						{
							this.ChangeStFlag(ADMStateFlags.SP_NOTIFICATION_ACCESS_ERROR);
						}
						else
						{
							this.ChangeStFlag(ADMStateFlags.SP_NOTIFICATION_START_ERROR);
						}
						IKrispAnalytics instance = AnalyticsFactory.Instance;
						bool flag = this._kind == AudioDeviceKind.Speaker;
						KAudioSession currentSession2 = this.CurrentSession;
						string text;
						if (currentSession2 == null)
						{
							text = null;
						}
						else
						{
							IAudioDevice usedDevice = currentSession2.UsedDevice;
							text = ((usedDevice != null) ? usedDevice.DisplayName : null);
						}
						instance.Report(AnalyticEventComposer.StreamStartErrorEvent(flag, text));
						this.TraceMachine();
						goto IL_35F;
						IL_233:
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationDisconnected);
						if (msg.hr == -2147024891)
						{
							this.ChangeStFlag(ADMStateFlags.SP_NOTIFICATION_ACCESS_ERROR);
						}
						else
						{
							this.ChangeStFlag(ADMStateFlags.SP_NOTIFICATION_DISCONNECTED);
						}
						IKrispAnalytics instance2 = AnalyticsFactory.Instance;
						bool flag2 = this._kind == AudioDeviceKind.Speaker;
						KAudioSession currentSession3 = this.CurrentSession;
						string text2;
						if (currentSession3 == null)
						{
							text2 = null;
						}
						else
						{
							IAudioDevice usedDevice2 = currentSession3.UsedDevice;
							text2 = ((usedDevice2 != null) ? usedDevice2.DisplayName : null);
						}
						instance2.Report(AnalyticEventComposer.StreamDisconnectedEvent(flag2, text2));
						this.TraceMachine();
						goto IL_35F;
						IL_2BC:
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationStarted);
						this.TraceMachine();
						goto IL_35F;
						IL_2D3:
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationStarting);
						this.TraceMachine();
						goto IL_35F;
						IL_2EA:
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationResetting);
						this.TraceMachine();
						goto IL_35F;
						IL_301:
						base.Machine.Fire(AudioEngineStateMachine.EngineTrigger.SPNotificationStoped);
						this.TraceMachine();
						goto IL_35F;
						IL_318:
						this.CurrentSession.SetLastStats(msg.Message);
						goto IL_35F;
						IL_32E:
						this._logger.LogWarning(string.Format("OnNotify got Unknown Notification. sesId: {0}, notifyCode: {1},  Message: {2}", msg.sesId, msg.notifyCode, msg.Message));
						IL_35F:;
					}
					catch (Exception ex)
					{
						this._logger.LogWarning("Error: {0}", new object[] { ex.Message });
						throw;
					}
					return;
				}
			}
			this.TraceLine(string.Format("Ignore the Notification with sId: {0}", (msg != null) ? new uint?(msg.sesId) : null));
		}

		private HRESULT SetActivityState(bool state)
		{
			HRESULT hresult = 1;
			try
			{
				hresult = StreamProcessor.Instance.SetActivityState((EnStreamDirection)this._kind, state);
				hresult.LogOnHerror(this._logger, string.Format("Unable to SetActivityState for {0} device.", this._kind));
			}
			catch (Exception ex) when (ex.HResult == -2147483638)
			{
				this._logger.LogWarning("SetActivityState: SP Not Initialized yet. Error: {0}", new object[] { ex.Message });
			}
			catch (Exception ex2)
			{
				this._logger.LogError("SetActivityState Error: {0}", new object[] { ex2 });
			}
			return hresult;
		}

		private HRESULT SetFeatureState(SPFeature feature, bool state)
		{
			HRESULT hresult = 1;
			try
			{
				if (feature == SPFeature.Feature_NoiseClean)
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.NCSwitchedEvent(this._kind == AudioDeviceKind.Speaker, state));
				}
				hresult = StreamProcessor.Instance.SetFeatureState((EnStreamDirection)this._kind, feature, state);
			}
			catch (Exception ex) when (ex.HResult == -2147483638)
			{
				this._logger.LogWarning("SetFeatureState: SP Not Initialized yet.");
			}
			catch (Exception ex2)
			{
				this._logger.LogError("SetFeatureState Error: {0}", new object[] { ex2 });
			}
			return hresult;
		}

		private uint newSessionID()
		{
			uint sesCounter = this._sesCounter;
			this._sesCounter = sesCounter + 1U;
			return sesCounter;
		}

		protected override void started()
		{
			this.CurrentSession.SessionStarted();
			this.ChangeStFlag(ADMStateFlags.HealtyState);
		}

		protected override void stoped()
		{
		}

		protected override void errorStartFailed()
		{
			this._logger.LogWarning("{0}: {1}", new object[]
			{
				ADMStateFlags.SP_NOTIFICATION_START_ERROR,
				"Unable to use the selected audio device"
			});
			this.ChangeStFlag(ADMStateFlags.SP_NOTIFICATION_START_ERROR);
		}

		protected override void resetting(AudioEngineStateMachine.EngineState st)
		{
			this._logger.LogInfo(string.Format("In state {0} resetting ...", st));
		}

		public int GetStreamActivityLevel()
		{
			return StreamProcessor.Instance.GetStreamActivityLevel((EnStreamDirection)this._kind);
		}

		private void TraceMachine()
		{
			this._logger.LogDebug("ESM ({0})", new object[] { base.Machine });
		}

		private void TraceLine(string message)
		{
			this._logger.LogDebug(message);
		}

		private void TraceLine(Exception ex)
		{
			this._logger.LogError(ex);
		}

		public void DumpDiagnosticInfo(StringBuilder sb, int indent)
		{
			this.beginLineSeparator(sb, 0);
			try
			{
				string text = "{0}CurrentSessionId: {1}";
				object obj = "".PadLeft(indent);
				KAudioSession currentSession = this.CurrentSession;
				sb.AppendLine(string.Format(text, obj, (currentSession != null) ? new uint?(currentSession.SessionId) : null));
				if (this.CurrentSession != null)
				{
					string text2 = "".PadLeft(indent);
					string text3 = "CurrentSession_UsedDeviceId: ";
					KAudioSession currentSession2 = this.CurrentSession;
					string text4;
					if (currentSession2 == null)
					{
						text4 = null;
					}
					else
					{
						IAudioDevice usedDevice = currentSession2.UsedDevice;
						text4 = ((usedDevice != null) ? usedDevice.Id : null);
					}
					sb.AppendLine(text2 + text3 + text4);
					string text5 = "{0}CurrentSession_KState: {1}";
					object obj2 = "".PadLeft(indent);
					KAudioSession currentSession3 = this.CurrentSession;
					sb.AppendLine(string.Format(text5, obj2, (currentSession3 != null) ? new KStreamSessionState?(currentSession3.KState) : null));
					string text6 = "{0}CurrentSession_CleanedDuration: {1}";
					object obj3 = "".PadLeft(indent);
					KAudioSession currentSession4 = this.CurrentSession;
					sb.AppendLine(string.Format(text6, obj3, (currentSession4 != null) ? new TimeSpan?(currentSession4.CleanedDuration) : null));
					string text7 = "{0}CurrentSession_StreamDuration: {1}";
					object obj4 = "".PadLeft(indent);
					KAudioSession currentSession5 = this.CurrentSession;
					sb.AppendLine(string.Format(text7, obj4, (currentSession5 != null) ? new TimeSpan?(currentSession5.StreamDuration) : null));
				}
				sb.AppendLine(string.Format("{0}ESM ({1})", "".PadLeft(indent), base.Machine));
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("Error on DumpDiagnosticInfo for AudioEngine: {0}", ex));
				sb.AppendLine("---------");
			}
			this.endLineSeparator(sb, 0);
		}

		private Logger _logger;

		private readonly AudioDeviceKind _kind;

		private uint _sesCounter;

		private IKrispControlStatus _krispControlStatus;

		private uint _activityThreshold = 5U;

		private bool _disposed;

		private KrispAudioDevice _krispDev;
	}
}
