using System;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Shared.Analytics;
using Shared.Helpers;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	internal class AudioDeviceSession : DisposableBase, IAudioDeviceSession, IAudioSessionEvents
	{
		public event EventHandler<AudioSessionDisconnectReason> SessionDisconnected;

		public event EventHandler<AudioSessionState> StateChanged;

		public AudioDeviceKind Kind { get; private set; }

		public string SessionID
		{
			get
			{
				return this._sess_id;
			}
		}

		public IAppInfo AppInfo { get; private set; }

		public IAudioSessionControl SessionControl { get; private set; }

		public AudioDeviceSession(AudioDeviceKind kind, IAudioSessionControl sessCtl, IAppInfo appInfo)
		{
			try
			{
				this.Kind = kind;
				this._logger = LogWrapper.GetLogger(string.Format("AudioDeviceSession ({0})", this.Kind));
				this.SessionControl = sessCtl;
				this.AppInfo = appInfo;
				((IAudioSessionControl2)this.SessionControl).GetSessionInstanceIdentifier(out this._sess_id);
				this.SessionControl.RegisterAudioSessionNotification(this);
				this._isRegistered = true;
				this._activityStartDT = DateTime.Now;
				this._state = this.SessionControl.GetState();
				this._logger.LogDebug("Audio session started # App: {0} ({1} - {2}), Session: {3} - {4}", new object[]
				{
					this.AppInfo.ExeName,
					this.AppInfo.PID,
					this.AppInfo.Type.ToString(),
					this._state.ToString(),
					this._sess_id
				});
			}
			catch (Exception ex)
			{
				this._logger.LogError("AudioDeviceSession failed: {0}", new object[] { ex.Message });
			}
		}

		~AudioDeviceSession()
		{
			this._logger.LogDebug("~AudioDeviceSession {0} # {1}", new object[]
			{
				this.AppInfo.ExeName,
				this._sess_id
			});
			this.Dispose(false);
		}

		protected override void Dispose(bool disposing)
		{
			this._logger.LogDebug("Dispose # {0} # {1} - {2}", new object[]
			{
				disposing,
				this.AppInfo.ExeName,
				this._sess_id
			});
			if (this._disposed)
			{
				return;
			}
			if (this._isRegistered && this.SessionControl != null)
			{
				this._isRegistered = false;
				this.SessionControl.UnregisterAudioSessionNotification(this);
			}
			this.SessionControl = null;
			this._disposed = true;
			base.Dispose(disposing);
		}

		public void UnregisterAudioSessionNotification()
		{
			if (this._isRegistered && this.SessionControl != null)
			{
				this._isRegistered = false;
				this.SessionControl.UnregisterAudioSessionNotification(this);
			}
		}

		void IAudioSessionEvents.OnChannelVolumeChanged(uint ChannelCount, IntPtr afNewChannelVolume, uint ChangedChannel, ref Guid EventContext)
		{
		}

		void IAudioSessionEvents.OnDisplayNameChanged(string NewDisplayName, ref Guid EventContext)
		{
		}

		void IAudioSessionEvents.OnGroupingParamChanged(ref Guid NewGroupingParam, ref Guid EventContext)
		{
		}

		void IAudioSessionEvents.OnIconPathChanged(string NewIconPath, ref Guid EventContext)
		{
		}

		void IAudioSessionEvents.OnSimpleVolumeChanged(float NewVolume, int NewMute, ref Guid EventContext)
		{
		}

		void IAudioSessionEvents.OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason)
		{
			this._logger.LogDebug("OnSessionDisconnected # DisconnectReason: {0} # App: {1} ({2} - {3}), Session: {4} - {5}", new object[]
			{
				DisconnectReason,
				this.AppInfo.ExeName,
				this.AppInfo.PID,
				this.AppInfo.Type.ToString(),
				this._state,
				this._sess_id
			});
			EventHandler<AudioSessionDisconnectReason> sessionDisconnected = this.SessionDisconnected;
			if (sessionDisconnected == null)
			{
				return;
			}
			sessionDisconnected(this, DisconnectReason);
		}

		void IAudioSessionEvents.OnStateChanged(AudioSessionState NewState)
		{
			this._logger.LogDebug("OnStateChanged # State: {0} ({1}) # App: {2} ({3} - {4}) # Session: {5}", new object[]
			{
				NewState,
				this._state,
				this.AppInfo.ExeName,
				this.AppInfo.PID,
				this.AppInfo.Type.ToString(),
				this._sess_id
			});
			object lockobj = this._lockobj;
			lock (lockobj)
			{
				switch (NewState)
				{
				case AudioSessionState.Inactive:
					if (this._state == AudioSessionState.Active)
					{
						uint num = Convert.ToUInt32((DateTime.Now - this._activityStartDT).TotalSeconds);
						if (num > 5U)
						{
							AnalyticsFactory.Instance.Report(AnalyticEventComposer.CallEndEvent(this.Kind == AudioDeviceKind.Speaker, this.AppInfo.ExeName, num));
						}
						this._logger.LogInfo(string.Format("CallEnd (Inactive)# ({0} sec.) # App: {1} ({2}).", num, this.AppInfo.ExeName, this.AppInfo.PID));
					}
					break;
				case AudioSessionState.Active:
					this._activityStartDT = DateTime.Now;
					this._logger.LogInfo(string.Format("ActivateCall (Active) # for App: {0} ({1}).", this.AppInfo.ExeName, this.AppInfo.PID));
					break;
				case AudioSessionState.Expired:
					if (this._state == AudioSessionState.Active)
					{
						uint num2 = Convert.ToUInt32((DateTime.Now - this._activityStartDT).TotalSeconds);
						if (num2 > 5U)
						{
							AnalyticsFactory.Instance.Report(AnalyticEventComposer.CallEndEvent(this.Kind == AudioDeviceKind.Speaker, this.AppInfo.ExeName, num2));
						}
						this._logger.LogInfo(string.Format("CallEnd (Expired) # ({0} sec.) # App: {1} ({2}).", num2, this.AppInfo.ExeName, this.AppInfo.PID));
					}
					break;
				default:
					this._logger.LogWarning("OnStateChanged # Invalid state: {0} ({1}) # App: {2} ({3} - {4}) # Session: {5}", new object[]
					{
						NewState,
						this._state,
						this.AppInfo.ExeName,
						this.AppInfo.PID,
						this.AppInfo.Type.ToString(),
						this._sess_id
					});
					break;
				}
				this._state = NewState;
				EventHandler<AudioSessionState> stateChanged = this.StateChanged;
				if (stateChanged != null)
				{
					stateChanged(this, this._state);
				}
			}
		}

		private const uint REPORT_DURATION_THRESHOLD = 5U;

		private Logger _logger;

		private bool _isRegistered;

		private string _sess_id;

		private AudioSessionState _state;

		private DateTime _activityStartDT;

		private bool _disposed;

		private object _lockobj = new object();
	}
}
