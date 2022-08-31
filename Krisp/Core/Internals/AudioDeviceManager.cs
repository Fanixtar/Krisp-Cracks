using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.Services;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Interops;
using Shared.Interops.Extensions;
using Shared.Interops.IMMDeviceAPI;
using WUApiLib;

namespace Krisp.Core.Internals
{
	public sealed class AudioDeviceManager : AudioDeviceLoader, IAudioDeviceManager, IDiagnosticsBase, IMMNotificationClient
	{
		public event EventHandler<bool> DevicesLoaded;

		public event EventHandler<KrispAudioDevice> FoundKrisp;

		public event EventHandler<IAudioDevice> DefaultChanged;

		public string InstanceName
		{
			get
			{
				return string.Format("{0}DeviceManager", this._kind);
			}
		}

		public IAudioDeviceCollection Devices
		{
			get
			{
				return this._devices;
			}
		}

		public IAudioDevice Default
		{
			get
			{
				return this._default;
			}
			private set
			{
				if (this._default != value)
				{
					this._default = value;
					Dispatcher dispatcher = this._dispatcher;
					if (dispatcher != null)
					{
						dispatcher.Invoke(delegate()
						{
							EventHandler<IAudioDevice> defaultChanged = this.DefaultChanged;
							if (defaultChanged == null)
							{
								return;
							}
							defaultChanged(this, this.Default);
						});
					}
					base.FireTrigger(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged);
				}
			}
		}

		public IAudioDevice PreviewDefault { get; private set; }

		public string KrispDevId
		{
			get
			{
				KrispAudioDevice krispDev = this._krispDev;
				if (krispDev == null)
				{
					return null;
				}
				return krispDev.Id;
			}
		}

		private EDataFlow dataFlow
		{
			get
			{
				if (this._kind != AudioDeviceKind.Speaker)
				{
					return EDataFlow.eCapture;
				}
				return EDataFlow.eRender;
			}
		}

		private void ChangeStFlag(ADMStateFlags st = ADMStateFlags.HealtyState)
		{
			this._lastSTFlag = st;
			Dispatcher dispatcher = this._dispatcher;
			if (dispatcher == null)
			{
				return;
			}
			dispatcher.Invoke(delegate()
			{
				IKrispControlStatus krispControlStatus = this._krispControlStatus;
				if (krispControlStatus == null)
				{
					return;
				}
				krispControlStatus.ChangeStFlag(this._lastSTFlag);
			});
		}

		public AudioDeviceManager(AudioDeviceKind kind, IKrispControlStatus krispControlStatus, bool? forceToDef = null)
			: base(forceToDef)
		{
			this._kind = kind;
			this._krispDev = null;
			this._logger = LogWrapper.GetLogger(string.Format("AudioDeviceManager ({0})", this._kind));
			this._dispatcher = Dispatcher.CurrentDispatcher;
			this._krispControlStatus = krispControlStatus;
			this._devices = new AudioDeviceCollection();
			this._enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
			AudioDeviceKind kind2 = this._kind;
			bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
			bool flag = true;
			this.EmptyDefItem = new AudioDevice(kind2, (forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null));
			this._devices.SetDefault(this.EmptyDefItem);
			this._messageNotifierService = ServiceContainer.Instance.GetService<IMessageNotifierService>();
			this._logger.LogDebug("Created");
		}

		public void LoadDevices()
		{
			Task.Factory.StartNew(delegate()
			{
				try
				{
					this.ChangeStFlag(ADMStateFlags.NoDeviceDetected);
					this.TraceMachine();
					this.QueryDefaultDevice();
					this._enumerator.RegisterEndpointNotificationCallback(this);
					this._epNotificationRegistered = true;
					IMMDeviceCollection immdeviceCollection = this._enumerator.EnumAudioEndpoints(this.dataFlow, DeviceState.ACTIVE | DeviceState.DISABLED | DeviceState.UNPLUGGED);
					uint count = immdeviceCollection.GetCount();
					for (uint num = 0U; num < count; num += 1U)
					{
						try
						{
							IMMDevice immdevice = immdeviceCollection.Item(num);
							if (immdevice.GetState() == DeviceState.ACTIVE)
							{
								this.AddDevice(immdevice);
							}
							else if (this._krispDev == null)
							{
								this.findKrispDevice(immdevice);
							}
						}
						catch (Exception ex)
						{
							this._logger.LogError("Error on DeviceEnumeration: {0}", new object[] { ex });
						}
					}
					if (this._krispDev == null)
					{
						int num2 = this.retrieveWUSysState();
						this._logger.LogError(string.Format("Missing the KrispDevice. preRestartRequired: {0}", num2));
						base.FireTrigger(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice);
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.KrispDeviceMissingEvent(this._kind == AudioDeviceKind.Speaker));
					}
					this.QueryDefaultDevice();
					this.PreviewDefault = this.Default;
					Dispatcher dispatcher = this._dispatcher;
					if (dispatcher != null)
					{
						dispatcher.Invoke(delegate()
						{
							EventHandler<KrispAudioDevice> foundKrisp = this.FoundKrisp;
							if (foundKrisp != null)
							{
								foundKrisp(this, this._krispDev);
							}
							this._deviceManagerLoaded = true;
							EventHandler<bool> devicesLoaded = this.DevicesLoaded;
							if (devicesLoaded == null)
							{
								return;
							}
							devicesLoaded(this, true);
						});
					}
				}
				catch (Exception ex2)
				{
					this.TraceLine(ex2);
					Dispatcher dispatcher2 = this._dispatcher;
					if (dispatcher2 != null)
					{
						dispatcher2.Invoke(delegate()
						{
							EventHandler<bool> devicesLoaded = this.DevicesLoaded;
							if (devicesLoaded == null)
							{
								return;
							}
							devicesLoaded(this, false);
						});
					}
				}
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				this._devices.RemoveAll();
				this._krispControlStatus = null;
				this._deviceManagerLoaded = false;
				if (this._krispWatcher != null)
				{
					this._krispWatcher.Dispose();
					this._krispWatcher = null;
				}
				KrispAudioDevice krispDev = this._krispDev;
				if (krispDev != null)
				{
					krispDev.Dispose();
				}
				this._krispDev = null;
			}
			if (this._epNotificationRegistered)
			{
				this._epNotificationRegistered = false;
				this._enumerator.UnregisterEndpointNotificationCallback(this);
			}
			this._enumerator = null;
			this._disposed = true;
			base.Dispose(disposing);
		}

		private bool IsInHealtyState()
		{
			return base.IsInState(AudioDeviceLoader.LoaderState.Healty);
		}

		protected override void errorKrispDevice_Missing()
		{
			this.ChangeStFlag(ADMStateFlags.KrispDeviceNotDetected);
		}

		protected override void errorKrispDevice_Disabled()
		{
			this.ChangeStFlag(ADMStateFlags.KrispDevice_Disabled);
		}

		protected override void errorKrispDefaultNotPermited()
		{
			this.ChangeStFlag(ADMStateFlags.KrispDefaultNotPermited);
			this._lastSTFlag = ADMStateFlags.KrispDefaultNotPermited;
			this._logger.LogWarning("Ignored the notification for KrispDefaultNotPermited.");
		}

		protected override void errorNoDeviceDetected()
		{
			this.ChangeStFlag(ADMStateFlags.NoDeviceDetected);
		}

		protected override bool devicesIsEmpty()
		{
			return this._devices.IsEmpty;
		}

		protected override bool inHealty()
		{
			bool flag = true;
			if (this.Default == null)
			{
				this.QueryDefaultDevice();
				if (this.Default == null)
				{
					this._logger.LogDebug("The Default is null.");
					flag = false;
				}
			}
			this.ChangeStFlag(ADMStateFlags.HealtyState);
			return flag;
		}

		protected override bool isKrispDefault()
		{
			bool flag = false;
			if (!string.IsNullOrWhiteSpace(this.KrispDevId))
			{
				IAudioDevice @default = this.Default;
				if (this.isKrispDevice((@default != null) ? @default.Id : null))
				{
					flag = true;
				}
			}
			return flag;
		}

		public void SetKrispDeviceWatcherHolder(IWatcherHandlerHolder wh)
		{
			this._watcherHolder = wh;
		}

		public void UnregisterEventNotifications()
		{
			try
			{
				this._logger.LogDebug("UnregisterEventNotifications");
				if (this._epNotificationRegistered)
				{
					this._epNotificationRegistered = false;
					this._enumerator.UnregisterEndpointNotificationCallback(this);
				}
			}
			catch (Exception ex)
			{
				this.TraceLine(ex);
			}
		}

		private void QueryDefaultDevice()
		{
			this._logger.LogDebug("QueryDefaultDevice");
			if (this._deviceManagerLoaded && base.IsInState(AudioDeviceLoader.LoaderState.NoDevice))
			{
				this._logger.LogWarning("The ADM is in NoDevice state.");
				return;
			}
			try
			{
				IAudioDevice defaultDevice = this.GetDefaultDevice(ERole.eMultimedia);
				if (defaultDevice != null)
				{
					IAudioDevice @default = this.Default;
					if (((@default != null) ? @default.Id : null) != defaultDevice.Id)
					{
						if (this.Default != null && !this.isKrispDevice(this.Default.Id))
						{
							this.PreviewDefault = this.Default;
						}
						this.Default = defaultDevice;
						this.TraceMachine();
					}
				}
			}
			catch (Exception ex)
			{
				this.TraceLine(ex);
			}
		}

		public IAudioDevice GetDefaultDevice(ERole eRole = ERole.eMultimedia)
		{
			IMMDevice immdevice = null;
			IAudioDevice audioDevice = null;
			try
			{
				IMMDeviceEnumerator enumerator = this._enumerator;
				immdevice = ((enumerator != null) ? enumerator.GetDefaultAudioEndpoint(this.dataFlow, ERole.eMultimedia) : null);
				if (this.isKrispDevice((immdevice != null) ? immdevice.GetId() : null))
				{
					this._logger.LogDebug("The Krisp device is default!");
					if (this._krispDev == null)
					{
						this._krispDev = new KrispAudioDevice(this._kind, immdevice);
						EventHandler<KrispAudioDevice> foundKrisp = this.FoundKrisp;
						if (foundKrisp != null)
						{
							foundKrisp(this, this._krispDev);
						}
					}
					return this._krispDev;
				}
			}
			catch (Exception ex) when (ex.Is(-2147023728))
			{
				this._logger.LogDebug(ex.Message);
			}
			if (immdevice != null)
			{
				this._devices.TryFind(immdevice.GetId(), out audioDevice);
				if (audioDevice == null)
				{
					this.AddDevice(immdevice);
					this._devices.TryFind(immdevice.GetId(), out audioDevice);
				}
			}
			else
			{
				this._logger.LogDebug("Error: No Default DeviceDetected.");
				this.fireNoDeviceDetected();
			}
			return audioDevice;
		}

		public bool ChangeSystemDefault()
		{
			HRESULT hresult = -2147467259;
			try
			{
				bool? flag = this._ForceKrispAsSystemDefault;
				bool flag2 = false;
				if ((flag.GetValueOrDefault() == flag2) & (flag != null))
				{
					IAudioDevice audioDevice = null;
					if (this.PreviewDefault != null && this._devices.TryFind(this.PreviewDefault.Id, out audioDevice))
					{
						hresult = this.setDefaultDevice(this.PreviewDefault, ERole.ERole_enum_count);
						hresult.LogOnNotSuccess(this._logger, "Unable to ReSet SystemDefault device ...");
						return hresult;
					}
					this.PreviewDefault = null;
					if (!this._devices.IsEmpty)
					{
						this.PreviewDefault = this._devices.GetFirstOrNull();
						Logger logger = this._logger;
						string text = "Setting the {0} device as default.";
						object[] array = new object[1];
						int num = 0;
						IAudioDevice previewDefault = this.PreviewDefault;
						array[num] = ((previewDefault != null) ? previewDefault.Id : null);
						logger.LogWarning(text, array);
					}
					if (this.PreviewDefault != null)
					{
						hresult = this.setDefaultDevice(this.PreviewDefault, ERole.ERole_enum_count);
						hresult.LogOnNotSuccess(this._logger, "Unable to ReSet SystemDefault device.");
					}
				}
				else
				{
					flag = this._ForceKrispAsSystemDefault;
					bool flag3 = true;
					if ((flag.GetValueOrDefault() == flag3) & (flag != null))
					{
						if (!this.isKrispDefault())
						{
							this.PreviewDefault = this.Default;
						}
						if (this._krispDev != null)
						{
							hresult = this.setDefaultDevice(this._krispDev, ERole.ERole_enum_count);
						}
					}
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Unable to ReSet SystemDefault device. {0}", new object[] { ex.Message });
			}
			return hresult == 0;
		}

		public HRESULT SetKrispDeviceStatus(bool bEnable)
		{
			if (AudioDeviceManager.s_PolicyConfigClient == null)
			{
				AudioDeviceManager.s_PolicyConfigClient = new AudioPolicyConfigClient();
			}
			HRESULT hresult = AudioDeviceManager.s_PolicyConfigClient.SetEndpointVisibility(this.KrispDevId, bEnable);
			this._logger.LogInfo("SetKrispDeviceStatus result: {0}", new object[] { hresult });
			return hresult;
		}

		private void fireNoDeviceDetected()
		{
			base.FireTrigger(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected);
			this.TraceMachine();
		}

		private void AddDevice(IMMDevice device)
		{
			string deviceId = ((device != null) ? device.GetId() : null);
			IAudioDevice audioDevice;
			if (!this._devices.TryFind(deviceId, out audioDevice))
			{
				try
				{
					if (((IMMEndpoint)device).GetDataFlow() == this.dataFlow)
					{
						if (this.findKrispDevice(device))
						{
							this._logger.LogInfo("Found KrispDevice: {0}", new object[] { device.GetId() });
							if (this._krispWatcher != null)
							{
								this._krispWatcher.Dispose();
								this._krispWatcher = null;
							}
							DeviceState state = device.GetState();
							if (base.IsInState(AudioDeviceLoader.LoaderState.KrispDisabled) || base.IsInState(AudioDeviceLoader.LoaderState.MissingKrispDevice))
							{
								if (state == DeviceState.UNPLUGGED || state.HasFlag(DeviceState.ACTIVE))
								{
									base.FireTrigger(AudioDeviceLoader.LoaderTrigger.KrispEnabled);
									this.TraceMachine();
								}
							}
							else if (state.HasFlag(DeviceState.DISABLED))
							{
								base.FireTrigger(AudioDeviceLoader.LoaderTrigger.KrispDisabled);
								this.TraceMachine();
							}
							else if (state.HasFlag(DeviceState.NOTPRESENT))
							{
								base.FireTrigger(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice);
								this.TraceMachine();
							}
							if (state.HasFlag(DeviceState.ACTIVE))
							{
								this._krispWatcher = new KrispDeviceSessionWatcher(this._kind, device);
								this._krispWatcher.SetWatcherHolder(this._watcherHolder);
								bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
								bool flag = true;
								if ((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null))
								{
									string krispDevId = this.KrispDevId;
									IAudioDevice @default = this.Default;
									if (string.Compare(krispDevId, (@default != null) ? @default.Id : null) != 0)
									{
										this.ChangeSystemDefault();
									}
								}
							}
						}
						else
						{
							AudioDevice newDevice = new AudioDevice(this._kind, device);
							Dispatcher dispatcher = this._dispatcher;
							if (dispatcher != null)
							{
								dispatcher.Invoke(delegate()
								{
									IAudioDevice audioDevice2;
									if (!this._devices.TryFind(deviceId, out audioDevice2))
									{
										this._logger.LogInfo("AddDevice : {0} - {1}", new object[] { newDevice.DisplayName, newDevice.Id });
										this._devices.Add(newDevice);
										if (!this.devicesIsEmpty())
										{
											this.FireTrigger(AudioDeviceLoader.LoaderTrigger.DeviceAdded);
											this.TraceMachine();
											return;
										}
										this._logger.LogDebug("Error. The devicelist is empty.");
										this.fireNoDeviceDetected();
									}
								});
							}
						}
					}
				}
				catch (Exception ex)
				{
					this.TraceLine(ex);
				}
			}
		}

		private HRESULT setDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
		{
			HRESULT hresult = -2147467259;
			if (device == null)
			{
				this._logger.LogDebug("SetDefaultDevice: called with device=null.");
				return hresult;
			}
			try
			{
				this._logger.LogDebug("Try to SetDefaultDevice({0}): {1} - {2}", new object[]
				{
					role,
					(device != null) ? device.DisplayName : null,
					(device != null) ? device.Id : null
				});
				if (AudioDeviceManager.s_PolicyConfigClient == null)
				{
					AudioDeviceManager.s_PolicyConfigClient = new AudioPolicyConfigClient();
				}
				IMMDevice device2 = this._enumerator.GetDevice(device.Id);
				if (device2 == null || !device2.GetState().HasFlag(DeviceState.ACTIVE))
				{
					this._logger.LogDebug("SetDefaultDevice: The device is Inactive. {0} - {1}", new object[] { device.DisplayName, device.Id });
					return hresult;
				}
				if (role != ERole.ERole_enum_count)
				{
					hresult = AudioDeviceManager.s_PolicyConfigClient.SetDefaultEndpoint(device.Id, role);
				}
				else
				{
					hresult = AudioDeviceManager.s_PolicyConfigClient.SetDefaultEndpoint(device.Id, ERole.eMultimedia);
					hresult = AudioDeviceManager.s_PolicyConfigClient.SetDefaultEndpoint(device.Id, ERole.eCommunications);
					if (hresult)
					{
						this.Default = device;
					}
				}
			}
			catch (Exception ex)
			{
				this.TraceLine(ex);
			}
			return hresult;
		}

		void IMMNotificationClient.OnDeviceAdded(string pwstrDeviceId)
		{
			this._logger.LogDebug("called OnDeviceAdded for {0}", new object[] { pwstrDeviceId });
			try
			{
				IMMDevice device = this._enumerator.GetDevice(pwstrDeviceId);
				if (device != null && device.GetState() == DeviceState.ACTIVE)
				{
					IMMEndpoint immendpoint = (IMMEndpoint)device;
					EDataFlow? edataFlow = ((immendpoint != null) ? new EDataFlow?(immendpoint.GetDataFlow()) : null);
					EDataFlow dataFlow = this.dataFlow;
					if ((edataFlow.GetValueOrDefault() == dataFlow) & (edataFlow != null))
					{
						this._logger.LogInfo("OnDeviceAdded {0}", new object[] { device.GetId() });
						this.AddDevice(device);
					}
				}
			}
			catch (Exception ex)
			{
				this.TraceLine(ex);
			}
		}

		private void removeDevice(string pwstrDeviceId)
		{
			try
			{
				IAudioDevice audioDevice;
				if (this._devices.TryFind(pwstrDeviceId, out audioDevice))
				{
					this._devices.Remove(audioDevice);
					this._logger.LogInfo("RemoveDevice {0} - {1}", new object[] { audioDevice.DisplayName, audioDevice.Id });
				}
				if (this.devicesIsEmpty())
				{
					this.fireNoDeviceDetected();
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error on removing device {0}", new object[] { pwstrDeviceId });
				this.TraceLine(ex);
			}
		}

		void IMMNotificationClient.OnDeviceRemoved(string pwstrDeviceId)
		{
			try
			{
				IAudioDevice audioDevice;
				if (this._devices.TryFind(pwstrDeviceId, out audioDevice))
				{
					Dispatcher dispatcher = this._dispatcher;
					if (dispatcher != null)
					{
						dispatcher.InvokeAsync(delegate()
						{
							this.removeDevice(pwstrDeviceId);
						});
					}
				}
			}
			catch (Exception ex)
			{
				this.TraceLine(ex);
			}
		}

		void IMMNotificationClient.OnDefaultDeviceChanged(EDataFlow flow, ERole role, string strDefaultDeviceId)
		{
			if (flow == this.dataFlow)
			{
				try
				{
					if (string.IsNullOrEmpty(strDefaultDeviceId))
					{
						this.Default = null;
					}
					else
					{
						IMMDevice defaultAudioEndpoint = this._enumerator.GetDefaultAudioEndpoint(flow, role);
						string text = ((defaultAudioEndpoint != null) ? defaultAudioEndpoint.GetId() : null);
						Logger logger = this._logger;
						string text2 = "-- OnDefaultDeviceChanged ({0}): strDefaultId: {1}, realDefId: {2} OldDefId: {3}";
						object[] array = new object[4];
						array[0] = role.ToString();
						array[1] = strDefaultDeviceId;
						array[2] = text;
						int num = 3;
						IAudioDevice @default = this.Default;
						array[num] = ((@default != null) ? @default.Id : null);
						logger.LogDebug(text2, array);
						if (strDefaultDeviceId == text)
						{
							IAudioDevice default2 = this.Default;
							if (strDefaultDeviceId == ((default2 != null) ? default2.Id : null))
							{
								this._logger.LogDebug("-- OnDefaultDeviceChanged -- Ignore.");
								return;
							}
						}
						if (role == ERole.eMultimedia)
						{
							if (strDefaultDeviceId != text)
							{
								Logger logger2 = this._logger;
								string text3 = "OnDefaultDeviceChanged ({0}): strDefaultDeviceId: {1}, realDefault: {2}, prevDefault:{3}";
								object[] array2 = new object[4];
								array2[0] = role.ToString();
								array2[1] = strDefaultDeviceId;
								array2[2] = text;
								int num2 = 3;
								IAudioDevice default3 = this.Default;
								array2[num2] = ((default3 != null) ? default3.Id : null);
								logger2.LogWarning(text3, array2);
							}
							bool? flag = this._ForceKrispAsSystemDefault;
							bool flag2 = false;
							if (((flag.GetValueOrDefault() == flag2) & (flag != null)) && this.isKrispDevice(text))
							{
								this._logger.LogWarning("OnDefaultDeviceChanged: The Krisp device cannot be Default Multimedia Device.");
							}
							IAudioDevice default4 = this.Default;
							if (!(((default4 != null) ? default4.Id : null) != text))
							{
								flag = this._ForceKrispAsSystemDefault;
								flag2 = true;
								if (!((flag.GetValueOrDefault() == flag2) & (flag != null)))
								{
									goto IL_235;
								}
							}
							this._dispatcher.InvokeAsync(delegate()
							{
								this.QueryDefaultDevice();
							});
						}
						else if (role == ERole.eConsole && this._ForceKrispAsSystemDefault == null)
						{
							if (strDefaultDeviceId != text)
							{
								Logger logger3 = this._logger;
								string text4 = "OnDefaultDeviceChanged ({0}): strDefaultDeviceId: {1}, realDefault: {2}, prevDefault:{3}";
								object[] array3 = new object[4];
								array3[0] = role.ToString();
								array3[1] = strDefaultDeviceId;
								array3[2] = text;
								int num3 = 3;
								IAudioDevice default5 = this.Default;
								array3[num3] = ((default5 != null) ? default5.Id : null);
								logger3.LogWarning(text4, array3);
							}
							if (this.isKrispDevice(text))
							{
								IAudioDevice default6 = this.Default;
								if (((default6 != null) ? default6.Id : null) != text)
								{
									this._dispatcher.InvokeAsync(delegate()
									{
										this.setDefaultDevice(this._krispDev, ERole.eCommunications);
									});
								}
							}
						}
						IL_235:;
					}
				}
				catch (Exception ex)
				{
					this.TraceLine(ex);
				}
			}
		}

		void IMMNotificationClient.OnDeviceStateChanged(string strDeviceId, DeviceState dwNewState)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				try
				{
					IMMDevice device = this._enumerator.GetDevice(strDeviceId);
					IMMEndpoint immendpoint = (IMMEndpoint)device;
					EDataFlow? edataFlow = ((immendpoint != null) ? new EDataFlow?(immendpoint.GetDataFlow()) : null);
					EDataFlow dataFlow = this.dataFlow;
					if ((edataFlow.GetValueOrDefault() == dataFlow) & (edataFlow != null))
					{
						this._logger.LogDebug("OnDeviceStateChanged {0} {1}", new object[] { strDeviceId, dwNewState });
						DeviceState dwNewState2 = dwNewState;
						switch (dwNewState2)
						{
						case DeviceState.ACTIVE:
							if (this.isKrispDevice(strDeviceId))
							{
								this.FireTrigger(AudioDeviceLoader.LoaderTrigger.KrispEnabled);
								this.TraceMachine();
							}
							this.AddDevice(device);
							goto IL_265;
						case DeviceState.DISABLED:
						case DeviceState.NOTPRESENT:
							if (this._krispDev == null)
							{
								this.findKrispDevice(device);
							}
							if (this.isKrispDevice(strDeviceId))
							{
								if (dwNewState == DeviceState.NOTPRESENT)
								{
									AnalyticsFactory.Instance.Report(AnalyticEventComposer.KrispDeviceMissingEvent(this._kind == AudioDeviceKind.Speaker));
									this.FireTrigger(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice);
									this._logger.LogWarning("{0}: KrispDeviceState: NOTPRESENT (Removed)", new object[] { this.InstanceName });
								}
								else
								{
									AnalyticsFactory.Instance.Report(AnalyticEventComposer.KrispDeviceInactiveEvent(this._kind == AudioDeviceKind.Speaker));
									this.FireTrigger(AudioDeviceLoader.LoaderTrigger.KrispDisabled);
								}
								this.TraceMachine();
							}
							this.removeDevice(strDeviceId);
							goto IL_265;
						case DeviceState.ACTIVE | DeviceState.DISABLED:
							break;
						default:
							if (dwNewState2 == DeviceState.UNPLUGGED)
							{
								if (this._epNotificationRegistered)
								{
									if (this._krispDev == null)
									{
										this.findKrispDevice(device);
									}
									if (this.isKrispDevice(strDeviceId) && device.GetState() == DeviceState.UNPLUGGED)
									{
										this.FireTrigger(AudioDeviceLoader.LoaderTrigger.KrispEnabled);
										this.TraceMachine();
									}
									this.removeDevice(strDeviceId);
									goto IL_265;
								}
								goto IL_265;
							}
							break;
						}
						this._logger.LogDebug("Unknown DEVICE_STATE: {0}", new object[] { dwNewState });
					}
					IL_265:;
				}
				catch (Exception ex)
				{
					this.TraceLine(ex);
				}
			});
		}

		void IMMNotificationClient.OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key)
		{
			IAudioDevice audioDevice;
			if (this._devices.TryFind(pwstrDeviceId, out audioDevice) && (PropertyKeys.PKEY_AudioEndPoint_Interface.fmtid.Equals(key.fmtid) || PropertyKeys.PKEY_AudioEngine_DeviceFormat.fmtid.Equals(key.fmtid)))
			{
				try
				{
					((AudioDevice)audioDevice).DevicePropertiesChanged(this._enumerator.GetDevice(audioDevice.Id), key);
				}
				catch (Exception ex)
				{
					this.TraceLine(ex);
				}
			}
		}

		private bool isKrispDevice(string devId)
		{
			return string.Compare(this.KrispDevId, devId) == 0;
		}

		private bool findKrispDevice(IMMDevice dev)
		{
			try
			{
				if (dev != null)
				{
					if (this._krispDev == null)
					{
						string value = dev.OpenPropertyStore(STGM.DIRECT).GetValue(PropertyKeys.DEVPKEY_DeviceInterface_FriendlyName);
						if (value != null && value.CompareTo("Krisp") == 0 && !dev.GetState().HasFlag(DeviceState.NOTPRESENT))
						{
							this._krispDev = new KrispAudioDevice(this._kind, dev);
							EventHandler<KrispAudioDevice> foundKrisp = this.FoundKrisp;
							if (foundKrisp != null)
							{
								foundKrisp(this, this._krispDev);
							}
							return true;
						}
					}
					KrispAudioDevice krispDev = this._krispDev;
					return string.Compare((krispDev != null) ? krispDev.Id : null, dev.GetId()) == 0;
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error in findKrispDevice(). {0}", new object[] { ex });
			}
			return false;
		}

		private int retrieveWUSysState()
		{
			try
			{
				return ((SystemInformation)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("C01B9BA0-BEA7-41BA-B604-D0A36F469133")))).RebootRequired ? 1 : 0;
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to fetch systemInfo. Error: ", new object[] { ex.Message });
			}
			return -1;
		}

		private void TraceLine(Exception ex)
		{
			this._logger.LogError(ex.Message);
		}

		private void TraceMachine()
		{
			this._logger.LogDebug("ADM ({0})", new object[] { base.StateTrace() });
		}

		public void DumpDiagnosticInfo(StringBuilder sb, int indent)
		{
			this.beginLineSeparator(sb, indent);
			try
			{
				sb.AppendLine("".PadLeft(indent) + "KrispDevId: " + this.KrispDevId);
				sb.AppendLine(string.Format("{0}ADM ({1}): {2}", "".PadLeft(indent), this._kind, base.StateTrace()));
				sb.AppendLine();
				sb.AppendLine("".PadLeft(indent) + "Begin WorkingDeviceLists:");
				foreach (IAudioDevice audioDevice in this.Devices)
				{
					if (audioDevice != null)
					{
						audioDevice.DumpDiagnosticInfo(sb, indent + 2);
					}
				}
				sb.AppendLine("".PadLeft(indent) + "End WorkingDeviceLists:");
				sb.AppendLine();
				sb.AppendLine("".PadLeft(indent) + "CurrentDefault:");
				IAudioDevice @default = this.Default;
				if (@default != null)
				{
					@default.DumpDiagnosticInfo(sb, indent + 2);
				}
				sb.AppendLine();
				sb.AppendLine("".PadLeft(indent) + "PreviewDefault:");
				IAudioDevice previewDefault = this.PreviewDefault;
				if (previewDefault != null)
				{
					previewDefault.DumpDiagnosticInfo(sb, indent + 2);
				}
				sb.AppendLine();
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("Error on DumpDiagnosticInfo for AudioDeviceManager: {0}", ex));
				sb.AppendLine("---------");
			}
			this.endLineSeparator(sb, indent);
		}

		public readonly IAudioDevice EmptyDefItem;

		private static AudioPolicyConfigClient s_PolicyConfigClient;

		private IAudioDevice _default;

		private IMMDeviceEnumerator _enumerator;

		private bool _epNotificationRegistered;

		private readonly Dispatcher _dispatcher;

		private readonly AudioDeviceKind _kind;

		private readonly AudioDeviceCollection _devices;

		private KrispAudioDevice _krispDev;

		private ADMStateFlags _lastSTFlag;

		private bool _deviceManagerLoaded;

		private IKrispControlStatus _krispControlStatus;

		private IMessageNotifierService _messageNotifierService;

		private Logger _logger;

		private bool _disposed;

		private KrispDeviceSessionWatcher _krispWatcher;

		private IWatcherHandlerHolder _watcherHolder;
	}
}
