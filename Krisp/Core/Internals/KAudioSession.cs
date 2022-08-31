using System;
using System.Diagnostics;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Shared.Analytics;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class KAudioSession : DisposableBase
	{
		public uint SessionId { get; }

		public IAudioDevice UsedDevice { get; private set; }

		public KStreamSessionState KState { get; private set; }

		public TimeSpan StreamDuration { get; private set; }

		public TimeSpan CleanedDuration { get; private set; }

		public KAudioSession(IAudioDevice krispDev, IAudioDevice dev, bool initialNC, uint sId, IStreamActivityHolder activityHolder, string sdkModel)
		{
			this.SessionId = sId;
			this._krispDev = krispDev;
			this.UsedDevice = dev;
			this.KState = KStreamSessionState.KSessionInactive;
			this._startTime = DateTime.MinValue;
			this._ncOnTime = DateTime.MinValue;
			this._ncState = initialNC;
			this._activityHolder = activityHolder;
			this.createVolumeMapper();
			this._activityHolder.RegisterActivityNotifications();
			this._activityHolder.StreamActivityChanged += this._activityHolder_StreamActivityChanged;
			this._sdkModelName = sdkModel;
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			Trace.TraceWarning(string.Format("Dispose Session. disposing: {0}, id: {1}", disposing, this.SessionId));
			if (disposing)
			{
				this.ReportAndClose();
			}
			this._disposed = true;
		}

		private void createVolumeMapper()
		{
			if (this._volumeMapper == null)
			{
				try
				{
					this._volumeMapper = new VolumeMapper(this._krispDev, this.UsedDevice);
				}
				catch (Exception ex)
				{
					string text = "AudioEngine ({0})";
					IAudioDevice usedDevice = this.UsedDevice;
					Logger logger = LogWrapper.GetLogger(string.Format(text, (usedDevice != null) ? new AudioDeviceKind?(usedDevice.Kind) : null));
					if (logger != null)
					{
						string text2 = "Error on creating VolumeMapper (SessionId: {0}) for {1} -and- {2}. Error: {3}";
						object[] array = new object[4];
						array[0] = this.SessionId;
						int num = 1;
						IAudioDevice krispDev = this._krispDev;
						array[num] = ((krispDev != null) ? krispDev.Id : null);
						int num2 = 2;
						IAudioDevice usedDevice2 = this.UsedDevice;
						array[num2] = ((usedDevice2 != null) ? usedDevice2.Id : null);
						array[3] = ex.Message;
						logger.LogError(text2, array);
					}
				}
			}
		}

		private void _activityHolder_StreamActivityChanged(object sender, StreamActivityState e)
		{
			if (e != StreamActivityState.StreamClosed)
			{
				if (e == StreamActivityState.StreamStarted)
				{
					this.SetStreamStarted(this._ncState);
					return;
				}
				if (e != StreamActivityState.StreamStoped)
				{
					return;
				}
			}
			this.SetStreamStoped();
		}

		public void ReportAndClose()
		{
			if (this._volumeMapper != null)
			{
				this._volumeMapper.Dispose();
			}
			this._volumeMapper = null;
			if (this._hidHeadset != null)
			{
				this._hidHeadset.setOnHook();
				this._hidHeadset.MuteTrigered -= this.HIDHeadset_MuteTrigered;
				this._hidHeadset.HookSwitched -= this.HIDHeadset_HookSwitched;
				this._hidHeadset.CloseDevice();
				this._hidHeadset = null;
			}
			this._activityHolder.StreamActivityChanged -= this._activityHolder_StreamActivityChanged;
			this.SetStreamStoped();
			this.reportAnalytics();
		}

		public void SessionStarted()
		{
			if (this._volumeMapper == null)
			{
				string text = "AudioEngine ({0})";
				IAudioDevice usedDevice = this.UsedDevice;
				Logger logger = LogWrapper.GetLogger(string.Format(text, (usedDevice != null) ? new AudioDeviceKind?(usedDevice.Kind) : null));
				if (logger != null)
				{
					logger.LogError("Error: The VolumeMapper for SessionId: {0} is null.", new object[] { this.SessionId });
				}
			}
			if (this._hidHeadset == null)
			{
				AudioDevice audioDevice = this.UsedDevice as AudioDevice;
				if (audioDevice != null && audioDevice.HIDDevice != null)
				{
					this._hidHeadset = audioDevice.HIDDevice;
					this._hidHeadset.MuteTrigered += this.HIDHeadset_MuteTrigered;
					this._hidHeadset.HookSwitched += this.HIDHeadset_HookSwitched;
				}
			}
		}

		private void HIDHeadset_HookSwitched(object sender, bool e)
		{
			if (KStreamSessionState.KSessionStarted == this.KState && !e && !e)
			{
				if (this._hidHeadset.OffHookStatus == 0)
				{
					HIDHeadset hidHeadset = this._hidHeadset;
					if (hidHeadset == null)
					{
						return;
					}
					hidHeadset.setOffHook();
					return;
				}
				else
				{
					this._hidHeadset.setOnHook();
				}
			}
		}

		public void HIDHeadset_MuteTrigered(object sender, bool e)
		{
			if (this._hidHeadset != null)
			{
				bool flag = e;
				VolumeMapper volumeMapper = this._volumeMapper;
				if (volumeMapper != null)
				{
					volumeMapper.SetPartyMuteState(ref flag);
				}
				this._hidHeadset.setMute(flag);
			}
		}

		public bool SetStreamStarted(bool initialNC)
		{
			if (KStreamSessionState.KSessionStarted == this.KState)
			{
				return false;
			}
			this.SessionStarted();
			VolumeMapper volumeMapper = this._volumeMapper;
			if (volumeMapper != null)
			{
				volumeMapper.StartMapping();
			}
			this.KState = KStreamSessionState.KSessionStarted;
			this.SetNCState(initialNC);
			this._startTime = DateTime.UtcNow;
			HIDHeadset hidHeadset = this._hidHeadset;
			if (hidHeadset != null)
			{
				hidHeadset.setOffHook();
			}
			return true;
		}

		public bool SetStreamStoped()
		{
			if (KStreamSessionState.KSessionStarted == this.KState)
			{
				DateTime utcNow = DateTime.UtcNow;
				this.KState = KStreamSessionState.KSessionStoped;
				if (this._startTime != DateTime.MinValue && (utcNow - this._startTime).TotalSeconds > KAudioSession.s_ActivityThreshold)
				{
					this.StreamDuration += utcNow - this._startTime;
				}
				this._startTime = DateTime.MinValue;
				this.SetNCState(this._ncState);
				this.reportAnalytics();
				HIDHeadset hidHeadset = this._hidHeadset;
				if (hidHeadset != null)
				{
					hidHeadset.setOnHook();
				}
				VolumeMapper volumeMapper = this._volumeMapper;
				if (volumeMapper != null)
				{
					volumeMapper.StopMapping();
				}
				return true;
			}
			return false;
		}

		public void SetNCState(bool state)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (state && KStreamSessionState.KSessionStarted == this.KState)
			{
				this._ncOnTime = utcNow;
			}
			else
			{
				if (this._ncOnTime != DateTime.MinValue && (utcNow - this._ncOnTime).TotalSeconds > KAudioSession.s_ActivityThreshold)
				{
					this.CleanedDuration += utcNow - this._ncOnTime;
				}
				this._ncOnTime = DateTime.MinValue;
			}
			this._ncState = state;
		}

		private void reportAnalytics()
		{
			AudioDeviceKind kind = this.UsedDevice.Kind;
			uint num = 0U;
			if (this.StreamDuration.TotalSeconds > KAudioSession.s_ActivityThreshold)
			{
				num = Convert.ToUInt32(this.StreamDuration.TotalSeconds);
				this.StreamDuration = TimeSpan.Zero;
				this._startTime = DateTime.MinValue;
			}
			uint num2 = Convert.ToUInt32(this.CleanedDuration.TotalSeconds);
			this.CleanedDuration = TimeSpan.Zero;
			this._ncOnTime = DateTime.MinValue;
			if (num > 0U)
			{
				WaveFormatExtensible defaultWaveFormat = this.UsedDevice.DefaultWaveFormat;
				string text = defaultWaveFormat.ToFormatedString();
				text = text + ",spStats:" + this._lastStats;
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.StreamStatsEvent(kind == AudioDeviceKind.Speaker, this.UsedDevice.DisplayName, num, num2, defaultWaveFormat.nSamplesPerSec, text, this._sdkModelName));
			}
		}

		internal void SetLastStats(string msg)
		{
			this._lastStats = msg;
		}

		public static uint s_ActivityThreshold = 5U;

		private bool _disposed;

		private DateTime _startTime;

		private DateTime _ncOnTime;

		private bool _ncState;

		private VolumeMapper _volumeMapper;

		private IStreamActivityHolder _activityHolder;

		private IAudioDevice _krispDev;

		private string _sdkModelName;

		private string _lastStats;

		private HIDHeadset _hidHeadset;
	}
}
