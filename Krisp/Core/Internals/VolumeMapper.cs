using System;
using System.Diagnostics;
using Krisp.AppHelper;
using Krisp.Models;
using Shared.Helpers;
using Shared.Interops;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	internal class VolumeMapper : DisposableBase
	{
		public VolumeMapper(IAudioDevice krispDev, IAudioDevice party)
		{
			this._logger = LogWrapper.GetLogger("VolumeMapper");
			this._krisp = krispDev;
			this._party = party;
			this._VMConf = new VolumeMappingConfig(this._krisp.Kind);
			this._lockMin = this._VMConf.VolumeLockMinHighConst;
			this._LockUpVolume = this._VMConf.LockUpVolume;
			this._contextGUID = Guid.NewGuid();
			if (this.RegisterCallBacks())
			{
				this.resetKrispVolume();
			}
		}

		~VolumeMapper()
		{
			this.Dispose(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				this.StopMapping();
			}
			this.UnRegisterControlChangeCallBack();
			this._disposed = true;
			base.Dispose(disposing);
		}

		private bool RegisterCallBacks()
		{
			HRESULT hresult = 1;
			try
			{
				IMMDeviceEnumerator immdeviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
				if (this._cbKrispVolume == null)
				{
					this._cbKrispVolume = new AudioEndpointVolumeCallback(this, true);
				}
				if (this._aeKrispVolume == null)
				{
					try
					{
						hresult = this.RegisterControlChangeCallBack(this._krisp.Id, immdeviceEnumerator, this._cbKrispVolume, ref this._aeKrispVolume);
						HRESULT hresult2 = hresult;
						Logger logger = this._logger;
						string text = "{0} Unable to register krisp's ControlChangeCallBack";
						IAudioDevice krisp = this._krisp;
						hresult2.LogOnNotSuccess(logger, string.Format(text, (krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null));
						if (hresult != 0)
						{
							return false;
						}
					}
					catch (Exception ex)
					{
						Logger logger2 = this._logger;
						string text2 = "{0} Unable to register krisp's ControlChangeCallBack: {1}";
						object[] array = new object[2];
						int num = 0;
						IAudioDevice krisp2 = this._krisp;
						array[num] = ((krisp2 != null) ? new AudioDeviceKind?(krisp2.Kind) : null);
						array[1] = ex.Message;
						logger2.LogError(text2, array);
						return false;
					}
				}
				if (this._cbPartyVolume == null)
				{
					this._cbPartyVolume = new AudioEndpointVolumeCallback(this, false);
				}
				if (this._aePartyVolume == null)
				{
					try
					{
						hresult = this.RegisterControlChangeCallBack(this._party.Id, immdeviceEnumerator, this._cbPartyVolume, ref this._aePartyVolume);
						HRESULT hresult3 = hresult;
						Logger logger3 = this._logger;
						string text3 = "{0} Unable to register party's ControlChangeCallBack";
						IAudioDevice krisp3 = this._krisp;
						hresult3.LogOnNotSuccess(logger3, string.Format(text3, (krisp3 != null) ? new AudioDeviceKind?(krisp3.Kind) : null));
						if (hresult != 0)
						{
							return false;
						}
					}
					catch (Exception ex2)
					{
						Logger logger4 = this._logger;
						string text4 = "{0} Unable to register party's ControlChangeCallBack: {1}";
						object[] array2 = new object[2];
						int num2 = 0;
						IAudioDevice krisp4 = this._krisp;
						array2[num2] = ((krisp4 != null) ? new AudioDeviceKind?(krisp4.Kind) : null);
						array2[1] = ex2.Message;
						logger4.LogError(text4, array2);
						return false;
					}
				}
			}
			catch (Exception ex3)
			{
				Logger logger5 = this._logger;
				string text5 = "{0} Unable to register ControlChangeCallBack: {0}";
				object[] array3 = new object[2];
				int num3 = 0;
				IAudioDevice krisp5 = this._krisp;
				array3[num3] = ((krisp5 != null) ? new AudioDeviceKind?(krisp5.Kind) : null);
				array3[1] = ex3;
				logger5.LogError(text5, array3);
				return false;
			}
			return hresult == 0;
		}

		private HRESULT RegisterControlChangeCallBack(string devId, IMMDeviceEnumerator enumerator, AudioEndpointVolumeCallback cbVolume, ref IAudioEndpointVolume aepVolume)
		{
			HRESULT hresult = -2147467259;
			if (string.IsNullOrWhiteSpace(devId) || cbVolume == null)
			{
				return -2147024809;
			}
			try
			{
				IMMDevice device = enumerator.GetDevice(devId);
				if (device != null)
				{
					if (aepVolume != null)
					{
						goto IL_21D;
					}
					try
					{
						hresult = device.Activate(out aepVolume);
						HRESULT hresult2 = hresult;
						Logger logger = this._logger;
						string text = "{0} Unable to Activate AudioEndpointVolumeInterface.";
						IAudioDevice krisp = this._krisp;
						hresult2.LogOnNotSuccess(logger, string.Format(text, (krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null));
						if (hresult != 0)
						{
							return -2147467259;
						}
						hresult = aepVolume.GetMasterVolumeLevelScalar(out this._mappedVolume);
						HRESULT hresult3 = hresult;
						Logger logger2 = this._logger;
						string text2 = "{0} Unable to get MasterVolumeLevel";
						IAudioDevice krisp2 = this._krisp;
						hresult3.LogOnNotSuccess(logger2, string.Format(text2, (krisp2 != null) ? new AudioDeviceKind?(krisp2.Kind) : null));
						if (hresult == 0)
						{
							bool flag = aepVolume.GetMute() == 1;
							Logger logger3 = this._logger;
							string text3 = "{0} Volume of {1}: {2}, isMuted: {3}";
							object[] array = new object[4];
							int num = 0;
							IAudioDevice krisp3 = this._krisp;
							array[num] = ((krisp3 != null) ? new AudioDeviceKind?(krisp3.Kind) : null);
							array[1] = devId;
							array[2] = this._mappedVolume;
							array[3] = flag;
							logger3.LogInfo(text3, array);
						}
						aepVolume.RegisterControlChangeNotify(cbVolume);
						goto IL_21D;
					}
					catch (Exception ex)
					{
						Logger logger4 = this._logger;
						string text4 = "{0} Unable to register/activate ControlChangeCallBack for device {1}, error: {2}";
						object[] array2 = new object[3];
						int num2 = 0;
						IAudioDevice krisp4 = this._krisp;
						array2[num2] = ((krisp4 != null) ? new AudioDeviceKind?(krisp4.Kind) : null);
						array2[1] = devId;
						array2[2] = ex.Message;
						logger4.LogError(text4, array2);
						return ex.HResult;
					}
				}
				Logger logger5 = this._logger;
				string text5 = "{0} Unable to get Device with devId: {1}.";
				IAudioDevice krisp5 = this._krisp;
				logger5.LogError(string.Format(text5, (krisp5 != null) ? new AudioDeviceKind?(krisp5.Kind) : null, devId));
				IL_21D:;
			}
			catch (Exception ex2)
			{
				Logger logger6 = this._logger;
				string text6 = "{0} Unable to register ControlChangeCallBack for device {1}, Error: {2}";
				object[] array3 = new object[3];
				int num3 = 0;
				IAudioDevice krisp6 = this._krisp;
				array3[num3] = ((krisp6 != null) ? new AudioDeviceKind?(krisp6.Kind) : null);
				array3[1] = devId;
				array3[2] = ex2;
				logger6.LogError(text6, array3);
				return ex2.HResult;
			}
			return hresult;
		}

		internal void StartMapping()
		{
			try
			{
				if (this._krisp.Kind == AudioDeviceKind.Microphone)
				{
					this._LockUpVolume = this._VMConf.LockUpVolume;
				}
				if (this._VMConf.MappingMode == VolumeMappingMode.DoNothing && this._LockUpVolume)
				{
					this.startMonitoring();
					this._aePartyVolume.SetMasterVolumeLevelScalar(this._VMConf.VolumeLockMaxConst, ref this._contextGUID).LogOnHerror(this._logger, "StartMapping: Unable to SetMasterVolumeLevel for PartyDev.");
				}
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to startMapping. Error: " + ex.Message);
			}
		}

		internal void StopMapping()
		{
			try
			{
				TimerHelper peakMonitoringTimer = this._peakMonitoringTimer;
				if (peakMonitoringTimer != null)
				{
					peakMonitoringTimer.Stop();
				}
				if (this._LockUpVolume)
				{
					this._LockUpVolume = false;
					if (this._krisp.Kind == AudioDeviceKind.Microphone)
					{
						IAudioEndpointVolume aePartyVolume = this._aePartyVolume;
						if (aePartyVolume != null)
						{
							aePartyVolume.SetMasterVolumeLevelScalar(this._storedVolume, ref this._contextGUID);
						}
					}
				}
			}
			catch
			{
			}
		}

		public void SetPartyMuteState(ref bool mute)
		{
			IAudioEndpointVolume aePartyVolume = this._aePartyVolume;
			if (aePartyVolume != null)
			{
				aePartyVolume.SetMute(mute ? 1 : 0, ref VolumeMapper.s_muteContextGUID);
			}
			IAudioEndpointVolume aePartyVolume2 = this._aePartyVolume;
			mute = aePartyVolume2 != null && aePartyVolume2.GetMute() == 1;
			Trace.TraceWarning(string.Format("----------- Muted: {0} ---------------", mute));
		}

		private void UnRegisterControlChangeCallBack()
		{
			IAudioEndpointVolume aeKrispVolume = this._aeKrispVolume;
			if (aeKrispVolume != null)
			{
				aeKrispVolume.UnregisterControlChangeNotify(this._cbKrispVolume);
			}
			this._aeKrispVolume = null;
			IAudioEndpointVolume aePartyVolume = this._aePartyVolume;
			if (aePartyVolume != null)
			{
				aePartyVolume.UnregisterControlChangeNotify(this._cbPartyVolume);
			}
			this._aePartyVolume = null;
			this._cbKrispVolume = null;
			this._cbPartyVolume = null;
		}

		private void resetKrispVolume()
		{
			try
			{
				float num = 0.55f;
				HRESULT hresult = this._aePartyVolume.GetMasterVolumeLevelScalar(out num);
				this.calculateVolumeRange();
				if (hresult == 0)
				{
					int mute = this._aePartyVolume.GetMute();
					this._storedVolume = num;
					if (this._VMConf.MappingMode == VolumeMappingMode.DoNothing && this._LockUpVolume)
					{
						hresult = this._aePartyVolume.SetMasterVolumeLevelScalar(this._VMConf.VolumeLockMaxConst, ref this._contextGUID);
						hresult.LogOnHerror(this._logger, "Unable to SetMasterVolumeLevel for PartyDev.");
						if (hresult == 0)
						{
							this._mappedVolume = this._VMConf.VolumeLockMaxConst;
						}
					}
					if (this._krisp.Kind != AudioDeviceKind.Microphone)
					{
						hresult = this._aeKrispVolume.SetMasterVolumeLevelScalar(num, ref this._contextGUID);
						hresult.LogOnHerror(this._logger, "Unable to SetMasterVolumeLevel for KrispDev.");
					}
					this._aeKrispVolume.SetMute(mute, ref this._contextGUID);
					Logger logger = this._logger;
					string text = "{0} volume applied. Volume: {1}, isMuted: {2}";
					object[] array = new object[3];
					int num2 = 0;
					IAudioDevice krisp = this._krisp;
					array[num2] = ((krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null);
					array[1] = num;
					array[2] = Convert.ToBoolean(mute);
					logger.LogInfo(text, array);
				}
				else
				{
					Logger logger2 = this._logger;
					string text2 = "{0} Unable to GetMasterVolumeLevel for PartyDev. Error: {1}";
					object[] array2 = new object[2];
					int num3 = 0;
					IAudioDevice krisp2 = this._krisp;
					array2[num3] = ((krisp2 != null) ? new AudioDeviceKind?(krisp2.Kind) : null);
					array2[1] = hresult;
					logger2.LogWarning(text2, array2);
				}
			}
			catch (Exception ex)
			{
				Logger logger3 = this._logger;
				string text3 = "resetKrispVolume: Unable to reset {1} volume. {0}";
				object[] array3 = new object[2];
				array3[0] = ex.Message;
				int num4 = 1;
				IAudioDevice krisp3 = this._krisp;
				array3[num4] = ((krisp3 != null) ? new AudioDeviceKind?(krisp3.Kind) : null);
				logger3.LogError(text3, array3);
			}
		}

		private void calculateVolumeRange()
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 6f;
			this._lockMin = this._VMConf.VolumeLockMinLowConst;
			this._aeKrispVolume.GetVolumeRange(out num2, out num3, out num4);
			this._aeKrispVolume.GetMasterVolumeLevel(out num);
			Logger logger = this._logger;
			string text = "Krisp {0}'s VLevel in dB: {1}, min: {2}, max: {3}, incr: {4}";
			object[] array = new object[5];
			int num6 = 0;
			IAudioDevice krisp = this._krisp;
			array[num6] = ((krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null);
			array[1] = num;
			array[2] = num2;
			array[3] = num3;
			array[4] = num4;
			logger.LogInfo(string.Format(text, array));
			this._aePartyVolume.GetMasterVolumeLevel(out num);
			this._aePartyVolume.GetVolumeRange(out num2, out num3, out num4);
			if (num3 - num5 > num2)
			{
				this._lockMin = 1f - 1f / Math.Abs(num3 - num2) * Math.Abs(num3 - num5);
			}
			if (this._lockMin == 1f)
			{
				this._lockMin = this._VMConf.VolumeLockMinHighConst;
			}
			else if (this._lockMin < this._VMConf.VolumeLockMinLowConst)
			{
				this._lockMin = this._VMConf.VolumeLockMinLowConst;
			}
			Logger logger2 = this._logger;
			string text2 = "{0}'s VLevel in dB: {1}, min: {2}, max: {3}, incr: {4}, -- lockMin: {5}";
			object[] array2 = new object[6];
			int num7 = 0;
			IAudioDevice party = this._party;
			array2[num7] = ((party != null) ? party.DisplayName : null);
			array2[1] = num;
			array2[2] = num2;
			array2[3] = num3;
			array2[4] = num4;
			array2[5] = this._lockMin;
			logger2.LogInfo(string.Format(text2, array2));
		}

		public void OnNotifyVolumeChanged(AUDIO_VOLUME_NOTIFICATION_DATA data, bool isKrisp)
		{
			try
			{
				float fMasterVolume = data.fMasterVolume;
				HRESULT hresult = 1;
				int bMuted = data.bMuted;
				Logger logger = this._logger;
				string text = "{0} VolumeChanged volume: {1} ";
				IAudioDevice krisp = this._krisp;
				logger.LogInfo(string.Format(text, (krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null, fMasterVolume) + string.Format("({0} : {1} : {2}), isMuted: {3} context: {4}, isKrisp: {5}", new object[]
				{
					this._mappedVolume,
					this._VMConf.LockUpVolume,
					this._lockMin,
					bMuted,
					data.guidEventContext,
					isKrisp
				}));
				if (!this._contextGUID.Equals(data.guidEventContext))
				{
					if (this._VMConf.MappingMode == VolumeMappingMode.DoNothing)
					{
						if (!isKrisp)
						{
							if (this._LockUpVolume)
							{
								Logger logger2 = this._logger;
								string text2 = "{0} ResetVolume to {1}, got scalar: {2}";
								object[] array = new object[3];
								int num = 0;
								IAudioDevice krisp2 = this._krisp;
								array[num] = ((krisp2 != null) ? new AudioDeviceKind?(krisp2.Kind) : null);
								array[1] = this._mappedVolume;
								array[2] = fMasterVolume;
								logger2.LogInfo(text2, array);
								hresult = this._aePartyVolume.SetMasterVolumeLevelScalar(this._mappedVolume, ref this._contextGUID);
								hresult.LogOnNotSuccess(this._logger, "ResetVolume ");
							}
							else
							{
								this._storedVolume = fMasterVolume;
							}
							this._aeKrispVolume.SetMute(bMuted, ref this._contextGUID);
						}
						else
						{
							this._aePartyVolume.SetMute(bMuted, ref this._contextGUID);
						}
					}
					else
					{
						if (isKrisp)
						{
							hresult = this._aePartyVolume.SetMasterVolumeLevelScalar(fMasterVolume, ref this._contextGUID);
							this._aePartyVolume.SetMute(bMuted, ref this._contextGUID);
						}
						else
						{
							hresult = this._aeKrispVolume.SetMasterVolumeLevelScalar(fMasterVolume, ref this._contextGUID);
							this._aeKrispVolume.SetMute(bMuted, ref this._contextGUID);
						}
						if (hresult == 0)
						{
							this._mappedVolume = fMasterVolume;
						}
					}
					Logger logger3 = this._logger;
					string text3 = "{0} VolumeChanged volume: {1} ({5} : {6} : {7}), isMuted: {2} context: {3}, isKrisp: {4}";
					object[] array2 = new object[8];
					int num2 = 0;
					IAudioDevice krisp3 = this._krisp;
					array2[num2] = ((krisp3 != null) ? new AudioDeviceKind?(krisp3.Kind) : null);
					array2[1] = fMasterVolume;
					array2[2] = bMuted;
					array2[3] = data.guidEventContext;
					array2[4] = isKrisp;
					array2[5] = this._mappedVolume;
					array2[6] = this._VMConf.LockUpVolume;
					array2[7] = this._lockMin;
					logger3.LogInfo(text3, array2);
				}
			}
			catch (Exception ex)
			{
				Logger logger4 = this._logger;
				string text4 = "{0} OnNotifyVolumeChanged: Unable to reset party device ({1}) volume. {2}";
				object[] array3 = new object[3];
				int num3 = 0;
				IAudioDevice krisp4 = this._krisp;
				array3[num3] = ((krisp4 != null) ? new AudioDeviceKind?(krisp4.Kind) : null);
				array3[1] = isKrisp;
				array3[2] = ex.Message;
				logger4.LogError(text4, array3);
			}
		}

		private void startMonitoring()
		{
			this._peakMeterData.resetValues();
			if (this._peakMonitoringTimer == null)
			{
				this._peakMonitoringTimer = new TimerHelper();
				this._peakMonitoringTimer.Interval = 100.0;
				this._peakMonitoringTimer.AutoReset = true;
				this._peakMonitoringTimer.Elapsed += delegate(object s, TimerHelperElapsedEventArgs eventArgs)
				{
					try
					{
						float peakValue = ((AudioDevice)this._party).AudioMeter.GetPeakValue();
						this._peakMeterData._absPeak = Math.Max(this._peakMeterData._absPeak, peakValue);
						if (this._peakMeterData._currPeak < peakValue)
						{
							PeakMeterData peakMeterData = this._peakMeterData;
							int cpeakCount = peakMeterData._cpeakCount;
							peakMeterData._cpeakCount = cpeakCount + 1;
							if (cpeakCount > 1)
							{
								Logger logger = this._logger;
								string text = "{0} -- AudioMeter -- PeakValues: {1},  {2},  {3}";
								object[] array = new object[4];
								int num = 0;
								IAudioDevice krisp = this._krisp;
								array[num] = ((krisp != null) ? new AudioDeviceKind?(krisp.Kind) : null);
								array[1] = this._peakMeterData._absPeak;
								array[2] = this._peakMeterData._currPeak;
								array[3] = peakValue;
								logger.LogInfo(string.Format(text, array));
								this._peakMeterData._currPeak = peakValue;
								if (this._peakMeterData._currPeak > this._VMConf.VolumeLockMinHighConst && this._mappedVolume > this._lockMin)
								{
									this._aePartyVolume.VolumeStepDown(ref this._contextGUID);
									this._logger.LogInfo(string.Format("VolumeStepDown. currPeak: {0} ", this._peakMeterData._currPeak));
									float num2;
									if (this._aePartyVolume.GetMasterVolumeLevelScalar(out num2) == 0)
									{
										this._mappedVolume = num2;
										this._peakMeterData.resetValues();
									}
								}
								else if (this._mappedVolume < this._lockMin)
								{
									this._mappedVolume = this._lockMin;
									this._aePartyVolume.SetMasterVolumeLevelScalar(this._mappedVolume, ref this._contextGUID);
								}
							}
						}
					}
					catch (Exception ex)
					{
						this._logger.LogError(string.Format(" -- {0}", ex));
						TimerHelper peakMonitoringTimer = this._peakMonitoringTimer;
						if (peakMonitoringTimer != null)
						{
							peakMonitoringTimer.Stop();
						}
					}
				};
			}
			this._peakMonitoringTimer.Start();
		}

		private static Guid s_muteContextGUID = Guid.Parse("9ddca828-1d85-43a4-bec5-8c3d54cd54f5");

		private Logger _logger;

		private IAudioDevice _krisp;

		private IAudioDevice _party;

		private IAudioEndpointVolume _aeKrispVolume;

		private IAudioEndpointVolume _aePartyVolume;

		private AudioEndpointVolumeCallback _cbKrispVolume;

		private AudioEndpointVolumeCallback _cbPartyVolume;

		private Guid _contextGUID;

		private bool _disposed;

		private float _mappedVolume = 0.55f;

		private float _storedVolume = 0.55f;

		private bool _LockUpVolume = true;

		private TimerHelper _peakMonitoringTimer;

		private float _lockMin;

		private PeakMeterData _peakMeterData = new PeakMeterData();

		private VolumeMappingConfig _VMConf;
	}
}
