using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Interops;

namespace Krisp.UI.ViewModels
{
	public class KrispControllerViewModel : BindableBase
	{
		public event EventHandler<bool> NCSwitchEvent;

		public event EventHandler<IAppInfo> StreamStarted;

		public event EventHandler<IAppInfo> StreamStopped;

		public ObservableCollection<DeviceViewModel> AllDevices { get; private set; }

		public bool NCSwitch
		{
			get
			{
				return this._ncSwitch && !this._forceDisabled;
			}
			set
			{
				if (this._ncSwitch != value)
				{
					if (this._devManagerReady)
					{
						try
						{
							Settings.Default[string.Format("{0}NCState", this._kind)] = value;
							Settings.Default.Save();
						}
						catch (Exception ex)
						{
							this._logger.LogError("Error on storing SPSwitch state. {0}", new object[] { ex.Message });
						}
					}
					this._ncSwitch = value;
					base.RaisePropertyChanged("NCSwitch");
					base.RaisePropertyChanged("NCToggleState");
					base.RaisePropertyChanged("RoomEchoState");
					Mediator.Instance.NotifyColleagues<DeviceKindValueArg>("NCSwitched", new DeviceKindValueArg
					{
						kind = this._kind,
						Value = this._ncSwitch
					});
					EventHandler<bool> ncswitchEvent = this.NCSwitchEvent;
					if (ncswitchEvent == null)
					{
						return;
					}
					ncswitchEvent(this, this._ncSwitch && !this._forceDisabled);
				}
			}
		}

		public bool RoomEchoSwitch
		{
			get
			{
				return this._roomEchoSwitch;
			}
			set
			{
				if (this._roomEchoSwitch != value)
				{
					this._roomEchoSwitch = value;
					try
					{
						Settings.Default[string.Format("{0}RoomModeOn", this._kind)] = value;
						Settings.Default.Save();
					}
					catch (Exception ex)
					{
						this._logger.LogError("Error on storing REcState. {0}", new object[] { ex.Message });
					}
					base.RaisePropertyChanged("RoomEchoState");
				}
				IKrispController krispController = this._krispController;
				if (krispController == null)
				{
					return;
				}
				krispController.OnNCModeChange(this, this._roomEchoSwitch ? SPFeature.Feature_Dereverb : SPFeature.Feature_NoiseClean);
			}
		}

		public double ActivityLevel
		{
			get
			{
				return this._activityLevel;
			}
			set
			{
				this._activityLevel = value;
				base.RaisePropertyChanged("ActivityLevel");
			}
		}

		public DeviceViewModel SelectedDeviceItem
		{
			get
			{
				return this.SelectedDevice;
			}
			set
			{
				if (this.SelectedDevice != value && value != null)
				{
					this.SelectedDevice = value;
					IKrispController krispController = this._krispController;
					if (krispController == null)
					{
						return;
					}
					DeviceViewModel selectedDevice = this.SelectedDevice;
					krispController.StoreSelectedDeviceID((selectedDevice != null) ? selectedDevice.AudioDevice : null);
				}
			}
		}

		public bool HasActiveStream
		{
			get
			{
				return this._hasActiveStream;
			}
			set
			{
				if (value != this._hasActiveStream)
				{
					this._hasActiveStream = value;
					base.RaisePropertyChanged("HasActiveStream");
					Mediator.Instance.NotifyColleagues<DeviceKindValueArg>("ActiveStreamChanged", new DeviceKindValueArg
					{
						kind = this._kind,
						Value = value
					});
				}
			}
		}

		public ControlStatusViewModel ControllerStatusViewModel { get; set; }

		public bool DeviceComboIsEnabled
		{
			get
			{
				return this.ControllerStatusViewModel.LastStatus == null || !this.ControllerStatusViewModel.LastStatus.Flags.HasFlag(StatusMessageFlags.DisabledDeviceCombo);
			}
		}

		public bool RoomEchoAvailable
		{
			get
			{
				return this._roomEchoAvailable;
			}
			set
			{
				if (value != this._roomEchoAvailable)
				{
					this._roomEchoAvailable = value;
					base.RaisePropertyChanged("RoomEchoAvailable");
				}
			}
		}

		public bool OpenOfficeAvailable
		{
			get
			{
				return this._openOfficeAvailable;
			}
			set
			{
				if (value != this._openOfficeAvailable)
				{
					this._openOfficeAvailable = value;
					base.RaisePropertyChanged("OpenOfficeAvailable");
					if (!this._openOfficeAvailable && this.OpenOfficeSwitch)
					{
						this.OpenOfficeSwitch = false;
					}
				}
			}
		}

		public bool OpenOfficeSwitch
		{
			get
			{
				return this._openOfficeSwitch;
			}
			set
			{
				if (value != this._openOfficeSwitch)
				{
					this._openOfficeSwitch = value;
					base.RaisePropertyChanged("OpenOfficeSwitch");
					if (this._openOfficeSwitch)
					{
						IKrispController krispController = this._krispController;
						if (krispController == null)
						{
							return;
						}
						krispController.OnNCModeChange(this, SPFeature.Feature_OpenOffice);
						return;
					}
					else if (this.RoomEchoAvailable && this.RoomEchoSwitch)
					{
						this.resetECEnabledState();
						IKrispController krispController2 = this._krispController;
						if (krispController2 == null)
						{
							return;
						}
						krispController2.OnNCModeChange(this, SPFeature.Feature_Dereverb);
						return;
					}
					else
					{
						IKrispController krispController3 = this._krispController;
						if (krispController3 == null)
						{
							return;
						}
						krispController3.OnNCModeChange(this, SPFeature.Feature_NoiseClean);
					}
				}
			}
		}

		public RoomEchoCancelationState RoomEchoState
		{
			get
			{
				if (!this._ncSwitch && this._roomEchoState != RoomEchoCancelationState.NotAvailable)
				{
					return RoomEchoCancelationState.Disabled;
				}
				return this._roomEchoState;
			}
			set
			{
				this._roomEchoState = value;
				this._dispatcher.Invoke(delegate()
				{
					base.RaisePropertyChanged("RoomEchoState");
				});
				if (this._roomEchoSwitch && this._roomEchoState == RoomEchoCancelationState.NotAvailable && this._kind == AudioDeviceKind.Microphone)
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.FeatureNotApplicable("room_echo_out"));
				}
			}
		}

		public bool NCToggleState
		{
			get
			{
				bool flag = false;
				if (this._toggleState)
				{
					flag = this.ControllerStatusViewModel.LastStatus == null || !this.ControllerStatusViewModel.LastStatus.Flags.HasFlag(StatusMessageFlags.DisabledToggle);
				}
				return flag && !this._forceDisabled;
			}
			set
			{
				if (this._toggleState != value)
				{
					this._toggleState = value;
					base.RaisePropertyChanged("NCToggleState");
				}
			}
		}

		public string DeviceTypeName
		{
			get
			{
				return this._kind.PresentationName();
			}
		}

		public ObservableCollection<string> UsingApps { get; private set; }

		public List<string> FilteredUsingApps { get; private set; }

		private IAudioDeviceManager _deviceManager
		{
			get
			{
				IKrispController krispController = this._krispController;
				if (krispController == null)
				{
					return null;
				}
				return krispController.DeviceManager;
			}
		}

		private IKrispController _krispController
		{
			get
			{
				IKrispController krispController = null;
				WeakReference<IKrispController> weakKrispController = this._weakKrispController;
				if (weakKrispController != null)
				{
					weakKrispController.TryGetTarget(out krispController);
				}
				return krispController;
			}
		}

		private DeviceViewModel SelectedDevice
		{
			get
			{
				return this._selectedDevice;
			}
			set
			{
				if (value != null && this._selectedDevice != value)
				{
					Logger logger = this._logger;
					string text = "({0}): SelectedDeviceChanged. oldValue: {1} - {2} , newValue: {3} - {4}";
					object[] array = new object[5];
					array[0] = this._kind;
					int num = 1;
					DeviceViewModel selectedDevice = this._selectedDevice;
					array[num] = ((selectedDevice != null) ? selectedDevice.DisplayName : null);
					int num2 = 2;
					DeviceViewModel selectedDevice2 = this._selectedDevice;
					array[num2] = ((selectedDevice2 != null) ? selectedDevice2.Id : null);
					array[3] = value.DisplayName;
					array[4] = value.Id;
					logger.LogInfo(text, array);
					this._selectedDevice = value;
					this._dispatcher.Invoke(delegate()
					{
						base.RaisePropertyChanged("SelectedDeviceItem");
						IKrispController krispController = this._krispController;
						if (krispController == null)
						{
							return;
						}
						krispController.SelectedDeviceChanged(this, this._selectedDevice.AudioDevice);
					});
					this.resetECEnabledState();
				}
			}
		}

		private void resetECEnabledState()
		{
			if (!this._roomEchoAvailable)
			{
				return;
			}
			DeviceViewModel selectedDevice = this._selectedDevice;
			IAudioDevice audioDevice = ((selectedDevice != null) ? selectedDevice.AudioDevice : null);
			if (audioDevice != null && audioDevice.TreatAsSystemDefault)
			{
				IAudioDeviceManager deviceManager = this._deviceManager;
				audioDevice = ((deviceManager != null) ? deviceManager.Default : null);
			}
			if (audioDevice == null)
			{
				this.RoomEchoState = RoomEchoCancelationState.Disabled;
				return;
			}
			this.RoomEchoState = (DataModelFactory.SPInstance.IsSPFeatureAvailable((EnStreamDirection)this._kind, SPFeature.Feature_Dereverb, audioDevice.DefaultWaveFormat.nSamplesPerSec) ? RoomEchoCancelationState.Available : RoomEchoCancelationState.NotAvailable);
		}

		private void OnWorkingDeviceChanged(object sender, IAudioDevice e)
		{
			DeviceViewModel deviceViewModel = null;
			if (this.canBeSelected(e, out deviceViewModel))
			{
				this._selectedDevice = deviceViewModel;
				this._dispatcher.Invoke(delegate()
				{
					base.RaisePropertyChanged("SelectedDeviceItem");
				});
				this.resetECEnabledState();
			}
		}

		private IAudioDevice PreviewSelection { get; set; }

		private KrispControllerViewModel()
		{
		}

		public KrispControllerViewModel(AudioDeviceKind kind)
		{
			this._logger = LogWrapper.GetLogger("DeviceCollectionVM");
			this._kind = kind;
			this._dispatcher = Dispatcher.CurrentDispatcher;
			this.AllDevices = new ObservableCollection<DeviceViewModel>();
			this.ControllerStatusViewModel = new ControlStatusViewModel();
			this._krispControlStatus = DataModelFactory.CreateKrispControlStatus(kind);
			this._roomEchoAvailable = this._kind == AudioDeviceKind.Microphone;
			this.UsingApps = new ObservableCollection<string>();
			this.FilteredUsingApps = new List<string>();
			this._activityTimer = new TimerHelper();
			this._activityTimer.Interval = 100.0;
			this._activityTimer.AutoReset = true;
			this._activityTimer.Elapsed += delegate(object s, TimerHelperElapsedEventArgs eventArgs)
			{
				if (this._krispController != null)
				{
					this.ActivityLevel = Math.Min(4.3 * (double)this._krispController.GetStreamActivityLevel(), 43.0);
				}
			};
		}

		~KrispControllerViewModel()
		{
			if (this._krispController != null)
			{
				this._krispController.DeviceManagerLoaded -= this.DeviceManager_Loaded;
				this._krispController.DeviceCollectionChanged -= this.OnDeviceCollectionChanged;
			}
			this._krispControlStatus.StateChanged -= this.KrispControlStatus_StateChanged;
		}

		public void AttachHandlers()
		{
			if (!this._handlersAttached)
			{
				this._weakKrispController = new WeakReference<IKrispController>(DataModelFactory.KrispController(this._kind));
				if (this._krispController != null)
				{
					this._krispController.DeviceManagerLoaded += this.DeviceManager_Loaded;
					this._krispController.DeviceCollectionChanged += this.OnDeviceCollectionChanged;
					this._krispController.WorkingDeviceChanged += this.OnWorkingDeviceChanged;
					this.NCSwitchEvent += this._krispController.OnNCSwitch;
					this.OnDeviceCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					this._handlersAttached = true;
					if (this._watcherHolder == null)
					{
						this._watcherHolder = new WatcherHandlerHolder();
					}
					this._watcherHolder.SessionConnected += this._watcherHolder_SessionConnected;
					this._watcherHolder.SessionDisconnected += this._watcherHolder_SessionDisconnected;
					this._krispController.SetKrispDeviceWatcherHolder(this._watcherHolder);
					AccountManager.Instance.InternetConnectionChangeDetected += this.InternetConnectionChangeDetected;
					this.RoomEchoSwitch = false;
					return;
				}
				this._logger.LogError(string.Format("Krisp{0}Controller is null!!!", this._kind));
			}
		}

		public void DeAttachHandlers()
		{
			if (this._handlersAttached)
			{
				this._krispControlStatus.StateChanged -= this.KrispControlStatus_StateChanged;
				if (this._krispController != null)
				{
					this._devManagerReady = false;
					if (this._watcherHolder != null)
					{
						this._watcherHolder.SessionConnected -= this._watcherHolder_SessionConnected;
						this._watcherHolder.SessionDisconnected -= this._watcherHolder_SessionDisconnected;
					}
					this._krispController.WorkingDeviceChanged -= this.OnWorkingDeviceChanged;
					this._krispController.DeviceManagerLoaded -= this.DeviceManager_Loaded;
					this._krispController.DeviceCollectionChanged -= this.OnDeviceCollectionChanged;
					this.NCSwitchEvent -= this._krispController.OnNCSwitch;
					this.UsingApps.Clear();
					this.FilteredUsingApps.Clear();
				}
				this._handlersAttached = false;
			}
		}

		private void InternetConnectionChangeDetected(object sender, bool e)
		{
			this._forceDisabled = !e;
			base.RaisePropertyChanged("NCToggleState");
			base.RaisePropertyChanged("NCSwitch");
			EventHandler<bool> ncswitchEvent = this.NCSwitchEvent;
			if (ncswitchEvent == null)
			{
				return;
			}
			ncswitchEvent(this, this._ncSwitch && !this._forceDisabled);
		}

		private void _watcherHolder_SessionConnected(object sender, IAppInfo e)
		{
			object obj = this._usingAppsLock;
			lock (obj)
			{
				this.UsingApps.Add(string.IsNullOrWhiteSpace(e.Description) ? e.ExeName : e.Description);
				base.RaisePropertyChanged("UsingApps");
			}
			if (AppSettingsHelper.Instance == null || !AppSettingsHelper.Instance.IgnoreList.Contains(e.ExeName))
			{
				obj = this._filteredUsingAppsLock;
				lock (obj)
				{
					this.FilteredUsingApps.Add(e.ExeName);
				}
				EventHandler<IAppInfo> streamStarted = this.StreamStarted;
				if (streamStarted == null)
				{
					return;
				}
				streamStarted(this, e);
			}
		}

		private void _watcherHolder_SessionDisconnected(object sender, IAppInfo e)
		{
			object obj = this._usingAppsLock;
			lock (obj)
			{
				this.UsingApps.Remove(string.IsNullOrWhiteSpace(e.Description) ? e.ExeName : e.Description);
				base.RaisePropertyChanged("UsingApps");
			}
			if (AppSettingsHelper.Instance == null || !AppSettingsHelper.Instance.IgnoreList.Contains(e.ExeName))
			{
				obj = this._filteredUsingAppsLock;
				lock (obj)
				{
					this.FilteredUsingApps.Remove(e.ExeName);
				}
				EventHandler<IAppInfo> streamStopped = this.StreamStopped;
				if (streamStopped == null)
				{
					return;
				}
				streamStopped(this, e);
			}
		}

		private void KrispControlStatus_StateChanged(object sender, ADMStateFlags evnt)
		{
			StatusMessage statusMessage = null;
			if (evnt != ADMStateFlags.HealtyState)
			{
				statusMessage = new StatusMessage();
				statusMessage.Message = evnt.ToStrFromStringTable(true);
				statusMessage.Flags |= StatusMessageFlags.DisabledToggle;
				if (evnt.IsUiUnrecoverable())
				{
					statusMessage.Flags |= StatusMessageFlags.DisabledDeviceCombo;
				}
			}
			if (evnt == ADMStateFlags.KrispDevice_Disabled && this._deviceManager != null)
			{
				this.ControllerStatusViewModel.ApplyStatus(statusMessage, delegate
				{
					this._deviceManager.SetKrispDeviceStatus(true);
				});
			}
			else
			{
				this.ControllerStatusViewModel.ApplyStatus(statusMessage, evnt.HasRepairHandler());
			}
			base.RaisePropertyChanged("NCToggleState");
			base.RaisePropertyChanged("DeviceComboIsEnabled");
		}

		private void OnStreamActivityChanged(object sender, bool e)
		{
			this.HasActiveStream = e;
			if (e)
			{
				this._activityTimer.Start();
				return;
			}
			this._activityTimer.Stop();
			this.ActivityLevel = 0.0;
		}

		private void DeviceManager_Loaded(object sender, EventArgs e)
		{
			this._krispControlStatus.StateChanged += this.KrispControlStatus_StateChanged;
			this._krispControlStatus.StreamActivityChanged += this.OnStreamActivityChanged;
			this._devManagerReady = true;
		}

		private void AddDevice(IAudioDevice device)
		{
			DeviceViewModel deviceViewModel = new DeviceViewModel(device);
			this.AllDevices.Add(deviceViewModel);
			deviceViewModel.PropertyChanged += this.OnDevicePropertyChanged;
		}

		private void OnDevicePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!this._roomEchoAvailable)
			{
				return;
			}
			DeviceViewModel deviceViewModel = sender as DeviceViewModel;
			if (deviceViewModel != null)
			{
				if (((e != null) ? e.PropertyName : null) == "DefaultWaveFormat")
				{
					if (deviceViewModel == this.SelectedDevice)
					{
						this.resetECEnabledState();
						return;
					}
					if (this.SelectedDevice.AudioDevice.TreatAsSystemDefault && !deviceViewModel.AudioDevice.TreatAsSystemDefault && deviceViewModel.AudioDevice == this._deviceManager.Default)
					{
						this.resetECEnabledState();
						return;
					}
				}
				else if (deviceViewModel.Id == this.SelectedDevice.Id || deviceViewModel.AudioDevice == this._deviceManager.Default)
				{
					this.resetECEnabledState();
				}
			}
		}

		private bool canBeSelected(IAudioDevice dev, out DeviceViewModel found)
		{
			found = null;
			if (dev != null)
			{
				found = this.AllDevices.FirstOrDefault((DeviceViewModel d) => d.Id == dev.Id);
			}
			return found != null;
		}

		private void OnDeviceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
			{
				IAudioDevice devToAdd = (IAudioDevice)e.NewItems[0];
				if (devToAdd != null)
				{
					DeviceViewModel deviceViewModel = this.AllDevices.FirstOrDefault((DeviceViewModel itm) => string.Compare(itm.Id, devToAdd.Id) == 0);
					if (deviceViewModel != null)
					{
						this._logger.LogWarning("DevicesCollectionChanged: the device: {0} already exist in list.", new object[] { deviceViewModel.Id });
						return;
					}
					if (devToAdd.TreatAsSystemDefault)
					{
						this.AllDevices.Insert(0, new DeviceViewModel(devToAdd));
						return;
					}
					this.AddDevice(devToAdd);
					return;
				}
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				string removed = ((IAudioDevice)e.OldItems[0]).Id;
				DeviceViewModel deviceViewModel2 = this.AllDevices.FirstOrDefault((DeviceViewModel d) => d.Id == removed);
				if (deviceViewModel2 != null)
				{
					this.AllDevices.Remove(deviceViewModel2);
					return;
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
			case NotifyCollectionChangedAction.Move:
				break;
			case NotifyCollectionChangedAction.Reset:
				this.AllDevices.Clear();
				if (this._deviceManager != null)
				{
					foreach (IAudioDevice audioDevice in this._deviceManager.Devices)
					{
						this.AddDevice(audioDevice);
					}
				}
				break;
			default:
				return;
			}
		}

		private AudioDeviceKind _kind;

		private DeviceViewModel _selectedDevice;

		private bool _ncSwitch;

		private bool _roomEchoSwitch;

		private RoomEchoCancelationState _roomEchoState;

		private bool _roomEchoAvailable;

		private bool _openOfficeAvailable;

		private bool _openOfficeSwitch;

		private double _activityLevel;

		private TimerHelper _activityTimer;

		private Logger _logger;

		private object _usingAppsLock = new object();

		private object _filteredUsingAppsLock = new object();

		private bool _devManagerReady;

		private Dispatcher _dispatcher;

		private IKrispControlStatus _krispControlStatus;

		private bool _handlersAttached;

		private WatcherHandlerHolder _watcherHolder;

		private bool _toggleState = true;

		private bool _forceDisabled;

		private WeakReference<IKrispController> _weakKrispController;

		private bool _hasActiveStream;
	}
}
