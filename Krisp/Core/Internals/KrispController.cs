using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.Properties;
using Shared.Analytics;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class KrispController : ControllerStateMachine, IKrispController, IDiagnosticsBase, IDisposable
	{
		public event EventHandler DeviceManagerLoaded;

		public event NotifyCollectionChangedEventHandler DeviceCollectionChanged;

		public event EventHandler<IAudioDevice> WorkingDeviceChanged;

		public string InstanceName
		{
			get
			{
				return string.Format("{0}Controller", this._kind);
			}
		}

		public IAudioDeviceManager DeviceManager
		{
			get
			{
				return this._devManager;
			}
		}

		public bool? ForceKrispAsSystemDefault
		{
			get
			{
				return this._ForceKrispAsSystemDefault;
			}
		}

		public IAudioDevice WorkingDevice { get; private set; }

		public IAudioDeviceCollection AllDevices
		{
			get
			{
				AudioDeviceManager devManager = this._devManager;
				if (devManager == null)
				{
					return null;
				}
				return devManager.Devices;
			}
		}

		private bool isSystemDefaultAsSelected { get; set; }

		private IAudioDevice SelectedDevice
		{
			get
			{
				return this.WorkingDevice;
			}
			set
			{
				this.WorkingDevice = value;
				IAudioDevice audioDevice = ((this.WorkingDevice == null || this.WorkingDevice.TreatAsSystemDefault) ? this._devManager.EmptyDefItem : this.WorkingDevice);
				string text = "SelDev_Set: TreatAsSystemDefault: {0}, selDev: {1}";
				object[] array = new object[2];
				int num = 0;
				IAudioDevice workingDevice = this.WorkingDevice;
				array[num] = ((workingDevice != null) ? new bool?(workingDevice.TreatAsSystemDefault) : null);
				array[1] = ((audioDevice != null) ? audioDevice.DisplayName : null);
				Trace.TraceInformation(text, array);
				this.SelectedDeviceChanged(this, audioDevice);
			}
		}

		private IAudioDevice PreviewSelection { get; set; }

		public KrispController(AudioDeviceKind kind, bool? forceToDef = null)
			: base(forceToDef)
		{
			this._kind = kind;
			this._logger = LogWrapper.GetLogger(string.Format("KrispController ({0})", this._kind));
			this._dispatcher = Dispatcher.CurrentDispatcher;
			this._krispControlStatus = DataModelFactory.CreateKrispControlStatus(kind);
			this._devManager = new AudioDeviceManager(this._kind, this._krispControlStatus, forceToDef);
			AudioDeviceManager devManager = this._devManager;
			devManager.StateChanged = (EventHandler<AudioDeviceLoader.LoaderState>)Delegate.Combine(devManager.StateChanged, new EventHandler<AudioDeviceLoader.LoaderState>(this.OnDeviceManagerStateChanged));
			this._devManager.DefaultChanged += this.OnDefaultDeviceChanged;
			this._devManager.FoundKrisp += this.OnFoundKrispDevice;
			this._devManager.DevicesLoaded += this.OnDevicesLoaded;
			this._devManager.LoadDevices();
			this._audioEngine = new AudioEngine(this._kind);
			this._audioEngine.StateChanged += this.OnEngineStateChanged;
			if (this._kind == AudioDeviceKind.Speaker)
			{
				this._lastChoosedDeviceId = Settings.Default.LastSelectedSpeaker;
			}
			else
			{
				this._lastChoosedDeviceId = Settings.Default.LastSelectedMic;
			}
			this._devManager.Devices.CollectionChanged += this.OnDeviceCollectionChanged;
			bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
			bool flag = false;
			if (!((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null)))
			{
				this.isSystemDefaultAsSelected = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				this._krispControlStatus = null;
				if (this._watcherHolder != null)
				{
					this._watcherHolder.SessionConnected -= this.AudioSessionConnected;
					this._watcherHolder.SessionDisconnected -= this.AudioSessionDisconnected;
				}
				if (this._devManager != null)
				{
					this._devManager.UnregisterEventNotifications();
					this._devManager.DefaultChanged -= this.OnDefaultDeviceChanged;
					this._devManager.DevicesLoaded -= this.OnDevicesLoaded;
					AudioDeviceManager devManager = this._devManager;
					devManager.StateChanged = (EventHandler<AudioDeviceLoader.LoaderState>)Delegate.Remove(devManager.StateChanged, new EventHandler<AudioDeviceLoader.LoaderState>(this.OnDeviceManagerStateChanged));
					this._devManager.FoundKrisp -= this.OnFoundKrispDevice;
					this._devManager.Dispose();
				}
				if (this._audioEngine != null)
				{
					this._audioEngine.StateChanged -= this.OnEngineStateChanged;
					this._audioEngine.Dispose();
				}
			}
			this._disposed = true;
			base.Dispose(disposing);
		}

		private void OnFoundKrispDevice(object sender, KrispAudioDevice e)
		{
			if (e != null)
			{
				AudioEngine audioEngine = this._audioEngine;
				if (audioEngine != null)
				{
					audioEngine.FoundKrispDevice(e);
				}
			}
			this.ScanStates(null);
		}

		private void OnDevicesLoaded(object sender, bool bSuccess)
		{
			if (bSuccess)
			{
				this._devManagerLoaded = true;
				this.DevManagerLoaded();
				EventHandler deviceManagerLoaded = this.DeviceManagerLoaded;
				if (deviceManagerLoaded != null)
				{
					deviceManagerLoaded(this, null);
				}
				AudioEngine audioEngine = this._audioEngine;
				if (audioEngine != null)
				{
					audioEngine.SetDevicesLoaded();
				}
				this.ScanStates(this.SelectedDevice);
			}
		}

		public void StartProcessing()
		{
			if (this._devManagerLoaded)
			{
				this.ScanStates(this.SelectedDevice);
				return;
			}
			this.ScanStates(null);
		}

		private void DevManagerLoaded()
		{
			if (!string.IsNullOrWhiteSpace(this._lastChoosedDeviceId))
			{
				IAudioDevice audioDevice = this.AllDevices.FirstOrDefault((IAudioDevice dev) => dev.Id == this._lastChoosedDeviceId);
				if (audioDevice == null)
				{
					this.SelectedDevice = this.AllDevices.FirstOrDefault((IAudioDevice dev) => dev.Id == this._devManager.EmptyDefItem.Id);
					return;
				}
				this.SelectedDevice = audioDevice;
				if (this.PreviewSelection == null)
				{
					this.PreviewSelection = audioDevice;
					return;
				}
			}
			else
			{
				IAudioDevice audioDevice2 = this.AllDevices.FirstOrDefault((IAudioDevice dev) => dev.Id == this._devManager.EmptyDefItem.Id);
				this.SelectedDevice = audioDevice2;
			}
		}

		public void SetKrispDeviceWatcherHolder(IWatcherHandlerHolder wh)
		{
			this._watcherHolder = wh;
			this._devManager.SetKrispDeviceWatcherHolder(wh);
			this._devManager.SetKrispDeviceWatcherHolder(wh);
			if (this._watcherHolder != null && this._kind == AudioDeviceKind.Microphone)
			{
				this._watcherHolder.SessionConnected += this.AudioSessionConnected;
				this._watcherHolder.SessionDisconnected += this.AudioSessionDisconnected;
			}
		}

		private void AudioSessionConnected(object sender, IAppInfo e)
		{
		}

		private void AudioSessionDisconnected(object sender, IAppInfo e)
		{
		}

		public void OnNCSwitch(object sender, bool state)
		{
			AudioEngine audioEngine = this._audioEngine;
			if (audioEngine == null)
			{
				return;
			}
			audioEngine.NCSwitch(state);
		}

		public void OnNCModeChange(object sender, SPFeature feature)
		{
			AudioEngine audioEngine = this._audioEngine;
			if (audioEngine == null)
			{
				return;
			}
			audioEngine.NCModeChange(feature);
		}

		public void StoreSelectedDeviceID(IAudioDevice dev)
		{
			if (dev == null)
			{
				return;
			}
			try
			{
				if (this._kind == AudioDeviceKind.Microphone)
				{
					Settings.Default.LastSelectedMic = dev.Id;
					Settings.Default.Save();
				}
				else if (this._kind == AudioDeviceKind.Speaker)
				{
					Settings.Default.LastSelectedSpeaker = dev.Id;
					Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error on StoreSelectedDeviceID. {0}", new object[] { ex.Message });
			}
			Logger logger = this._logger;
			string text = "({0}): Selected device changed. oldValue: {1} - {2} , newValue: {3} - {4}";
			object[] array = new object[5];
			array[0] = this._kind;
			int num = 1;
			IAudioDevice previewSelection = this.PreviewSelection;
			array[num] = ((previewSelection != null) ? previewSelection.DisplayName : null);
			int num2 = 2;
			IAudioDevice previewSelection2 = this.PreviewSelection;
			array[num2] = ((previewSelection2 != null) ? previewSelection2.Id : null);
			array[3] = dev.DisplayName;
			array[4] = dev.Id;
			logger.LogInfo(text, array);
			this.PreviewSelection = dev;
		}

		private void OnDeviceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler deviceCollectionChanged = this.DeviceCollectionChanged;
			if (deviceCollectionChanged != null)
			{
				deviceCollectionChanged(sender, e);
			}
			if (!this._devManagerLoaded)
			{
				return;
			}
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
			{
				IAudioDevice devToAdd = (IAudioDevice)e.NewItems[0];
				if (devToAdd != null)
				{
					string text = ((this.PreviewSelection != null) ? this.PreviewSelection.Id : this._lastChoosedDeviceId);
					if (string.Compare(devToAdd.Id, text) == 0)
					{
						IAudioDevice audioDevice = this.AllDevices.FirstOrDefault((IAudioDevice d) => d.Id == devToAdd.Id);
						if (audioDevice != null)
						{
							Trace.TraceWarning("({0}) DevicesCollection_Add: found _lastChoosedDevice. switch the device: {1}.", new object[] { this._kind, audioDevice.Id });
							this.SelectedDevice = audioDevice;
							return;
						}
					}
				}
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				IAudioDevice audioDevice2 = (IAudioDevice)e.OldItems[0];
				if (audioDevice2 != null)
				{
					Trace.TraceInformation("({0}) DevicesCollection_Remove {2}: Removed device: {1}.", new object[] { this._kind, audioDevice2.Id, sender });
					string id = audioDevice2.Id;
					IAudioDevice selectedDevice = this.SelectedDevice;
					if (id == ((selectedDevice != null) ? selectedDevice.Id : null))
					{
						if (audioDevice2.TreatAsSystemDefault || this.isSystemDefaultAsSelected)
						{
							IAudioDevice audioDevice3 = null;
							if (this.canBeSelected(this.PreviewSelection, out audioDevice3))
							{
								this._logger.LogDebug("({0}) DevicesCollection_Remove: PreviewSelection switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
								Trace.TraceWarning("({0}) DevicesCollection_Remove: PreviewSelection switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
							}
							else if (this._devManager.Default != null && this.canBeSelected(this._devManager.GetDefaultDevice(ERole.eMultimedia), out audioDevice3))
							{
								this._logger.LogDebug("({0}) DevicesCollection_Remove: Default switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
								Trace.TraceWarning("({0}) DevicesCollection_Remove: Default switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
							}
							else if (this._devManager.PreviewDefault != null && this.canBeSelected(this._devManager.PreviewDefault, out audioDevice3))
							{
								this._logger.LogDebug("({0}) DevicesCollection_Remove: PreviewDefault switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
								Trace.TraceWarning("({0}) DevicesCollection_Remove: PreviewDefault switch the device: {1}.", new object[] { this._kind, audioDevice3.Id });
							}
							if (audioDevice3 != null)
							{
								this.SelectedDevice = audioDevice3;
								return;
							}
							IAudioDevice filteredFirstDevice = this.getFilteredFirstDevice();
							if (filteredFirstDevice != null)
							{
								this._logger.LogWarning("({0}) DevicesCollection_Remove: failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice.Id });
								Trace.TraceError("({0}) --- DevicesCollection_Remove: failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice.Id });
								this.SelectedDevice = filteredFirstDevice;
								return;
							}
							this._logger.LogError("({0}) DevicesCollection_Remove:{1}. The DeviceList is empty.", new object[] { this._kind, audioDevice2.Id });
							return;
						}
						else
						{
							bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
							bool flag = true;
							if ((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null))
							{
								AudioDeviceManager devManager = this._devManager;
								this.SelectedDevice = ((devManager != null) ? devManager.EmptyDefItem : null);
								return;
							}
							IAudioDevice filteredFirstDevice2 = this.getFilteredFirstDevice();
							if (filteredFirstDevice2 != null)
							{
								this._logger.LogWarning("({0}) DevicesCollection_Remove: --NotDef-- failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice2.Id });
								Trace.TraceError("({0}) DevicesCollection_Remove (remove): --NotDef-- failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice2.Id });
								this.SelectedDevice = filteredFirstDevice2;
								return;
							}
						}
					}
					else if (audioDevice2.TreatAsSystemDefault)
					{
						IAudioDevice audioDevice4 = null;
						string text2 = "PreviewSelection";
						if (this.PreviewSelection != null)
						{
							audioDevice4 = this.AllDevices.FirstOrDefault((IAudioDevice d) => d.Id == this.PreviewSelection.Id);
						}
						if (audioDevice4 == null && this._devManager.PreviewDefault != null)
						{
							audioDevice4 = this.AllDevices.FirstOrDefault((IAudioDevice d) => d.Id == this._devManager.PreviewDefault.Id);
							text2 = "PreviewDefault";
						}
						if (audioDevice4 != null)
						{
							this._logger.LogDebug("({0}) __Devices_CollectionChanged (remove): {2} switch the device: {1}.", new object[] { this._kind, audioDevice4.Id, text2 });
							Trace.TraceWarning("({0}) __Devices_CollectionChanged (remove): {2} switch the device: {1}.", new object[] { this._kind, audioDevice4.Id, text2 });
							this.SelectedDevice = audioDevice4;
							return;
						}
						IAudioDevice filteredFirstDevice3 = this.getFilteredFirstDevice();
						if (filteredFirstDevice3 != null)
						{
							this._logger.LogWarning("({0}) __Devices_CollectionChanged (remove): --PreviewDefault-- failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice3.Id });
							Trace.TraceError("({0}) __Devices_CollectionChanged (remove): --PreviewDefault-- failover switch the device: {1}.", new object[] { this._kind, filteredFirstDevice3.Id });
							this.SelectedDevice = filteredFirstDevice3;
						}
					}
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
			case NotifyCollectionChangedAction.Move:
			case NotifyCollectionChangedAction.Reset:
				break;
			default:
				return;
			}
		}

		protected IAudioDevice getFilteredFirstDevice()
		{
			IAudioDevice audioDevice;
			if (this._kind == AudioDeviceKind.Microphone)
			{
				if (this.AllDevices.Count<IAudioDevice>() > 1)
				{
					audioDevice = this.AllDevices.FirstOrDefault((IAudioDevice s) => !s.DeviceDescription.Contains("Stereo Mix"));
				}
				else
				{
					audioDevice = this.AllDevices.FirstOrDefault<IAudioDevice>();
				}
			}
			else if (this.AllDevices.Count<IAudioDevice>() > 1)
			{
				audioDevice = this.AllDevices.FirstOrDefault((IAudioDevice s) => s.EnumeratorName != "HDAUDIO");
			}
			else
			{
				audioDevice = this.AllDevices.FirstOrDefault<IAudioDevice>();
			}
			if (audioDevice == null)
			{
				this._logger.LogWarning("({0}) FilteredDevice returns null.", new object[] { this._kind });
				Trace.TraceError("({0}) FilteredDevice returns null.", new object[] { this._kind });
			}
			return audioDevice;
		}

		public void OnDefaultDeviceChanged(object sender, IAudioDevice e)
		{
			if (this._audioEngine != null && !this._audioEngine.DevicesLoaded)
			{
				return;
			}
			bool? flag = this._ForceKrispAsSystemDefault;
			bool flag2 = false;
			if ((flag.GetValueOrDefault() == flag2) & (flag != null))
			{
				if (e != null && this.isSystemDefaultAsSelected && string.Compare(e.Id, this._devManager.KrispDevId) != 0)
				{
					this.ScanStates(e);
					return;
				}
			}
			else
			{
				flag = this._ForceKrispAsSystemDefault;
				flag2 = true;
				if ((flag.GetValueOrDefault() == flag2) & (flag != null))
				{
					if (e != null && string.Compare(e.Id, this._devManager.KrispDevId) != 0)
					{
						this._devManager.ChangeSystemDefault();
						if (this.isSystemDefaultAsSelected)
						{
							this.ScanStates(e);
							return;
						}
					}
				}
				else
				{
					if (string.Compare((e != null) ? e.Id : null, this._devManager.KrispDevId) == 0)
					{
						this._devManager.Devices.RemoveDefault(this);
					}
					else
					{
						this._devManager.Devices.SetDefault(this._devManager.EmptyDefItem);
					}
					if (!this.isSystemDefaultAsSelected)
					{
						if (e == null)
						{
							return;
						}
						string id = e.Id;
						IAudioDevice selectedDevice = this.SelectedDevice;
						if (!(id == ((selectedDevice != null) ? selectedDevice.Id : null)))
						{
							return;
						}
					}
					this.ScanStates(null);
				}
			}
		}

		private bool canBeSelected(IAudioDevice dev, out IAudioDevice found)
		{
			found = null;
			if (dev != null)
			{
				found = this.AllDevices.FirstOrDefault((IAudioDevice d) => d.Id == dev.Id);
			}
			return found != null;
		}

		private IAudioDevice retrieveAutomaticaly()
		{
			AudioDeviceManager devManager = this._devManager;
			IAudioDevice audioDevice = ((devManager != null) ? devManager.GetDefaultDevice(ERole.eMultimedia) : null);
			string text = ((audioDevice != null) ? audioDevice.Id : null);
			AudioDeviceManager devManager2 = this._devManager;
			if (text == ((devManager2 != null) ? devManager2.KrispDevId : null))
			{
				AudioDeviceManager devManager3 = this._devManager;
				IAudioDevice audioDevice2;
				if (this.canBeSelected((devManager3 != null) ? devManager3.PreviewDefault : null, out audioDevice2))
				{
					audioDevice = audioDevice2;
				}
				else if (this.PreviewSelection != null)
				{
					audioDevice2 = this.AllDevices.FirstOrDefault((IAudioDevice d) => d.Id == this.PreviewSelection.Id);
				}
				else
				{
					audioDevice = this.AllDevices.FirstOrDefault<IAudioDevice>();
				}
			}
			string text2 = ((audioDevice != null) ? audioDevice.Id : null);
			AudioDeviceManager devManager4 = this._devManager;
			if (text2 == ((devManager4 != null) ? devManager4.KrispDevId : null))
			{
				AudioDeviceManager devManager5 = this._devManager;
				IAudioDevice audioDevice3;
				if (this.canBeSelected((devManager5 != null) ? devManager5.PreviewDefault : null, out audioDevice3))
				{
					audioDevice = audioDevice3;
				}
			}
			return audioDevice;
		}

		public void SelectedDeviceChanged(object sender, IAudioDevice dev)
		{
			this._logger.LogDebug("ChangeSelectedDevice: " + dev.DisplayName);
			IAudioDevice audioDevice = dev;
			if (audioDevice.TreatAsSystemDefault)
			{
				bool? forceKrispAsSystemDefault = this.ForceKrispAsSystemDefault;
				bool flag = true;
				if ((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null))
				{
					audioDevice = this.retrieveAutomaticaly();
				}
				else
				{
					AudioDeviceManager devManager = this._devManager;
					audioDevice = ((devManager != null) ? devManager.Default : null);
				}
				if (audioDevice == null)
				{
					AudioDeviceManager devManager2 = this._devManager;
					audioDevice = ((devManager2 != null) ? devManager2.GetDefaultDevice(ERole.eMultimedia) : null);
				}
				string text = ((audioDevice != null) ? audioDevice.Id : null);
				AudioDeviceManager devManager3 = this._devManager;
				if (text == ((devManager3 != null) ? devManager3.KrispDevId : null))
				{
					audioDevice = this.retrieveAutomaticaly();
				}
			}
			this.isSystemDefaultAsSelected = dev.TreatAsSystemDefault;
			this.WorkingDevice = audioDevice;
			if (this.SelectedDevice != null && this.ScanStates(this.SelectedDevice))
			{
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.DeviceSelectedEvent(this._kind == AudioDeviceKind.Speaker, audioDevice.DisplayName, this.isSystemDefaultAsSelected));
				this._logger.LogInfo("Change Selected Device: " + dev.DisplayName);
			}
			if (sender == this)
			{
				EventHandler<IAudioDevice> workingDeviceChanged = this.WorkingDeviceChanged;
				if (workingDeviceChanged == null)
				{
					return;
				}
				workingDeviceChanged(this, dev);
			}
		}

		public HRESULT ScanStates(IAudioDevice selDev = null)
		{
			HRESULT hresult = 1;
			if (base.Machine.IsInState(ControllerStateMachine.ControllerState.ControllerUnhealtyState))
			{
				this.NotifyError(ADMStateFlags.UnRecoverable);
				return hresult;
			}
			if (this._audioEngine.DevicesLoaded)
			{
				if (this._devManager.IsInState(AudioDeviceLoader.LoaderState.Healty))
				{
					if (selDev == null)
					{
						if (this.isSystemDefaultAsSelected)
						{
							this.WorkingDevice = this._devManager.EmptyDefItem;
						}
						selDev = this._devManager.Default;
					}
					this._devManager.Devices.TryFind((selDev != null) ? selDev.Id : null, out selDev);
					if (selDev != null)
					{
						hresult = this.StartSession(selDev);
					}
					else if (this.isSystemDefaultAsSelected && this._devManager.Default != null && this._devManager.Default.Id != this._devManager.KrispDevId)
					{
						this.WorkingDevice = this._devManager.EmptyDefItem;
						hresult = this.StartSession(this._devManager.Default);
					}
					else if (this._devManager.Devices.Count<IAudioDevice>() == 0)
					{
						this._devManager.FireTrigger(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected);
					}
				}
				else if (this._devManager.IsInState(AudioDeviceLoader.LoaderState.NoDevice))
				{
					this._devManager.FireTrigger(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected);
				}
				else if (this._devManager.IsInState(AudioDeviceLoader.LoaderState.MissingKrispDevice))
				{
					this._devManager.FireTrigger(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice);
				}
			}
			else
			{
				this._logger.LogDebug("Devices are not loaded yet.");
			}
			return hresult;
		}

		private HRESULT StartSession(IAudioDevice selDev)
		{
			1;
			HRESULT hresult = this._audioEngine.StartSession(selDev, false);
			if (this.isSystemDefaultAsSelected)
			{
				AudioDeviceManager devManager = this._devManager;
				((AudioDevice)((devManager != null) ? devManager.EmptyDefItem : null)).TryToChangeDisplayName(selDev.DisplayName);
				return hresult;
			}
			AudioDeviceManager devManager2 = this._devManager;
			((AudioDevice)((devManager2 != null) ? devManager2.EmptyDefItem : null)).TryToChangeDisplayName(null);
			return hresult;
		}

		public void NotifyError(ADMStateFlags stf)
		{
			this._krispControlStatus.ChangeStFlag(stf);
			base.Machine.Fire(ControllerStateMachine.ControllerTrigger.UnhealtyStateError);
		}

		private void OnEngineStateChanged(object sender, AudioEngineStateMachine.EngineState e)
		{
			if (e == AudioEngineStateMachine.EngineState.SPSessionError)
			{
				this._logger.LogWarning("!Engine's State Changed: {0}", new object[] { e });
				return;
			}
			this._logger.LogDebug("OnEngineStateChanged: {0}", new object[] { e });
		}

		private void OnDeviceManagerStateChanged(object sender, AudioDeviceLoader.LoaderState e)
		{
			this._logger.LogDebug("OnDeviceManagerStateChanged: {0}", new object[] { e });
			this.TraceMachine("before");
			if (e != AudioDeviceLoader.LoaderState.Healty)
			{
				if (e == AudioDeviceLoader.LoaderState.KrispDefault)
				{
					if (base.Machine.IsInState(ControllerStateMachine.ControllerState.ControllerHealtyState) && this.DeviceManager.Default != null)
					{
						IAudioDevice selectedDevice = this.SelectedDevice;
						KAudioSession currentSession = this._audioEngine.CurrentSession;
						if (selectedDevice == ((currentSession != null) ? currentSession.UsedDevice : null))
						{
							Logger logger = this._logger;
							string text = "Don't stop the CurrentSession {0}";
							KAudioSession currentSession2 = this._audioEngine.CurrentSession;
							logger.LogDebug(string.Format(text, (currentSession2 != null) ? new uint?(currentSession2.SessionId) : null));
						}
					}
					base.Machine.Fire(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault);
				}
				else
				{
					this._audioEngine.StopSession(0U);
					base.Machine.Fire(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged);
				}
				this._logger.LogWarning("DeviceManager's StateChanged: {0}", new object[] { e });
			}
			else
			{
				base.Machine.Fire(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged);
				this.ScanStates(null);
			}
			this.TraceMachine("after");
		}

		public int GetStreamActivityLevel()
		{
			AudioEngine audioEngine = this._audioEngine;
			return ((audioEngine != null) ? new int?(audioEngine.GetStreamActivityLevel()) : null).Value;
		}

		protected override bool isAEStateHealty()
		{
			return this._audioEngine.Machine.IsInState(AudioEngineStateMachine.EngineState.SPSessionStarted);
		}

		protected override bool isDLStateHealty()
		{
			return this._devManager.IsInState(AudioDeviceLoader.LoaderState.Healty);
		}

		protected override void onControllerInKrispDefault()
		{
			bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
			bool flag = false;
			if ((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null))
			{
				this._devManager.ChangeSystemDefault();
			}
		}

		protected override void onControllerInHealtyState()
		{
			this._logger.LogDebug("onControllerInHealtyState");
			this.TraceMachine("");
		}

		protected override void onControllerInErrorState()
		{
			this._logger.LogDebug("onControllerInErrorState");
			this.TraceMachine("");
		}

		protected override void onControllerInUnhealtyState()
		{
			this._logger.LogDebug("onControllerInUnhealtyState");
			this.TraceMachine("");
		}

		private void TraceMachine(string prefix = "")
		{
			this._logger.LogDebug("{0} KAPC : {1}", new object[]
			{
				prefix,
				base.Machine.ToString()
			});
		}

		public void DumpDiagnosticInfo(StringBuilder sb, int indent)
		{
			this.beginLineSeparator(sb, 2);
			sb.AppendLine(string.Format("{0}ForceKrispAsSystemDefault: {1}", "".PadLeft(indent), this.ForceKrispAsSystemDefault));
			sb.AppendLine(string.Format("{0}isSystemDefaultAsSelected: {1}", "".PadLeft(indent), this.isSystemDefaultAsSelected));
			sb.AppendLine("".PadLeft(indent) + "Selected Device (Krisp working Device):");
			IAudioDevice selectedDevice = this.SelectedDevice;
			if (selectedDevice != null)
			{
				selectedDevice.DumpDiagnosticInfo(sb, indent + 4);
			}
			sb.AppendLine();
			AudioDeviceManager devManager = this._devManager;
			if (devManager != null)
			{
				devManager.DumpDiagnosticInfo(sb, indent + 4);
			}
			sb.AppendLine();
			AudioEngine audioEngine = this._audioEngine;
			if (audioEngine != null)
			{
				audioEngine.DumpDiagnosticInfo(sb, indent + 4);
			}
			this.endLineSeparator(sb, 2);
		}

		private IWatcherHandlerHolder _watcherHolder;

		private Logger _logger;

		private bool _disposed;

		private readonly Dispatcher _dispatcher;

		private readonly AudioDeviceKind _kind;

		private readonly AudioDeviceManager _devManager;

		private readonly AudioEngine _audioEngine;

		private IKrispControlStatus _krispControlStatus;

		private string _lastChoosedDeviceId;

		private bool _devManagerLoaded;
	}
}
