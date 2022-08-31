using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using HidLibrary;
using Krisp.AppHelper;
using Krisp.Models;
using Shared.Helpers;
using Shared.Interops;
using Shared.Interops.Extensions;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	public class AudioDevice : DisposableBase, IAudioDevice, INotifyPropertyChanged, IDiagnosticsBase
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public string Id
		{
			get
			{
				return this._id;
			}
		}

		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		public string IconPath
		{
			get
			{
				return this._iconPath;
			}
		}

		public string EnumeratorName
		{
			get
			{
				return this._enumeratorName;
			}
		}

		public string InterfaceName
		{
			get
			{
				return this._interfaceName;
			}
		}

		public string DeviceDescription
		{
			get
			{
				return this._deviceDescription;
			}
		}

		public bool TreatAsSystemDefault { get; private set; }

		public AudioDeviceKind Kind { get; }

		public string InstanceName
		{
			get
			{
				return string.Format("{0}Device", this.Kind);
			}
		}

		public WaveFormatExtensible DefaultWaveFormat
		{
			get
			{
				return this._defaultWaveFormat;
			}
		}

		internal HIDHeadset HIDDevice
		{
			get
			{
				return this._hidDevice;
			}
		}

		internal IAudioMeterInformation AudioMeter
		{
			get
			{
				if (this._meter == null)
				{
					this._device.Activate(out this._meter);
				}
				return this._meter;
			}
		}

		public AudioDevice(AudioDeviceKind kind, bool auto)
			: this(kind, null)
		{
			this._bAuto = auto;
			if (this._bAuto)
			{
				this._displayName = DefaultDeviceItem.s_AutoDisplayName;
				this._deviceDescription = DefaultDeviceItem.s_AutoDescription;
			}
		}

		public AudioDevice(AudioDeviceKind kind, IMMDevice device)
		{
			this.Kind = kind;
			this._device = device;
			if (device != null)
			{
				this._id = device.GetId();
			}
			else
			{
				this.TreatAsSystemDefault = true;
				this._id = "Default_Device";
			}
			this._logger.LogDebug("({0}) Create {1}", new object[] { this.Kind, this._id });
			this.ReadProperties();
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing && this._hidDevice != null)
			{
				this._hidDevice.Dispose();
				this._hidDevice = null;
			}
			this._disposed = true;
		}

		private void ReadProperties()
		{
			if (this.TreatAsSystemDefault)
			{
				this._displayName = (this._bAuto ? DefaultDeviceItem.s_AutoDisplayName : DefaultDeviceItem.s_DefaultDisplayName);
				this._iconPath = "";
				this._enumeratorName = "Device_EnumeratorName";
				this._interfaceName = "DeviceInterface_FriendlyName";
				this._deviceDescription = (this._bAuto ? DefaultDeviceItem.s_AutoDescription : DefaultDeviceItem.s_DefaultDescription);
				return;
			}
			try
			{
				IPropertyStore propertyStore = this._device.OpenPropertyStore(STGM.DIRECT);
				this._displayName = propertyStore.GetValue(PropertyKeys.PKEY_Device_FriendlyName);
				this._iconPath = propertyStore.GetValue(PropertyKeys.DEVPKEY_DeviceClass_IconPath);
				this._enumeratorName = propertyStore.GetValue(PropertyKeys.DEVPKEY_Device_EnumeratorName);
				this._interfaceName = propertyStore.GetValue(PropertyKeys.DEVPKEY_DeviceInterface_FriendlyName);
				this._deviceDescription = propertyStore.GetValue(PropertyKeys.DEVPKEY_Device_DeviceDesc);
				this._defaultWaveFormat = propertyStore.GetValue(PropertyKeys.PKEY_AudioEngine_DeviceFormat);
			}
			catch (Exception ex)
			{
				this._logger.LogWarning(ex.Message);
			}
		}

		private void checkForHIDHeadset()
		{
			if (this.Kind != AudioDeviceKind.Microphone || this._enumeratorName != "USB" || this._deviceContainerId.CompareTo(Guid.Empty) == 0)
			{
				return;
			}
			try
			{
				List<string> list = AudioEngineHelper.GetDevicesByContainerID(this._deviceContainerId).FindAll((string f) => f.StartsWith("HID\\VID_"));
				this._logger.LogInfo("({0}) Checking for HIDHeadset. ContainerID: {1}, InterfaceName: {2}, devID: {3}", new object[] { this.Kind, this._deviceContainerId, this._interfaceName, this._id });
				HidDevice hidDevice = null;
				HidEnumerator hidEnumerator = new HidEnumerator();
				foreach (string text in list)
				{
					string text2 = AudioEngineHelper.RegPathToHidPath(text);
					this._logger.LogDebug("checkFor: '{0}' as '{1}'", new object[] { text, text2 });
					IHidDevice device = hidEnumerator.GetDevice(text2);
					this._logger.LogDebug("-- '{0}' --- {1}", new object[] { device.DevicePath, device.Description });
					if (string.Compare(device.Description, "HID-compliant headset", true) == 0)
					{
						hidDevice = device as HidDevice;
						this._logger.LogInfo("Found {0}. devPath: '{1}'", new object[] { device.Description, device.DevicePath });
						break;
					}
				}
				if (hidDevice != null && hidDevice.Attributes.VendorId == 2830)
				{
					this._hidDevice = new JHIDHeadset(hidDevice);
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error on checking for HID headset. ", new object[] { ex });
			}
		}

		public void DevicePropertiesChanged(IMMDevice dev, PROPERTYKEY key)
		{
			this._logger.LogInfo("({0}) AudioDevice DevicePropertiesChanged {1}", new object[] { this.Kind, this._id });
			this._device = dev;
			this.ReadProperties();
			if (PropertyKeys.PKEY_AudioEngine_DeviceFormat.fmtid.Equals(key.fmtid))
			{
				PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
				if (propertyChanged == null)
				{
					return;
				}
				propertyChanged(this, new PropertyChangedEventArgs("DefaultWaveFormat"));
				return;
			}
			else
			{
				PropertyChangedEventHandler propertyChanged2 = this.PropertyChanged;
				if (propertyChanged2 == null)
				{
					return;
				}
				propertyChanged2(this, new PropertyChangedEventArgs("DisplayName"));
				return;
			}
		}

		public void TryToChangeDisplayName(string newName)
		{
			if (this.TreatAsSystemDefault)
			{
				if (string.IsNullOrWhiteSpace(newName))
				{
					this._displayName = (this._bAuto ? DefaultDeviceItem.s_AutoDisplayName : DefaultDeviceItem.s_DefaultDisplayName);
				}
				else if (DefaultDeviceItem.ChangeDisplayNameFn != null)
				{
					this._displayName = DefaultDeviceItem.ChangeDisplayNameFn(this._bAuto, newName);
				}
				PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
				if (propertyChanged == null)
				{
					return;
				}
				propertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
			}
		}

		public void DumpDiagnosticInfo(StringBuilder sb, int indent)
		{
			this.beginLineSeparator(sb, indent);
			try
			{
				sb.AppendLine("".PadLeft(indent) + "Id: " + this.Id);
				sb.AppendLine("".PadLeft(indent) + "DisplayName: " + this.DisplayName);
				sb.AppendLine("".PadLeft(indent) + "EnumeratorName: " + this.EnumeratorName);
				sb.AppendLine("".PadLeft(indent) + "InterfaceName: " + this.InterfaceName);
				sb.AppendLine("".PadLeft(indent) + "DeviceDescription: " + this.DeviceDescription);
				sb.AppendLine(string.Format("{0}TreatAsSystemDefault: {1}", "".PadLeft(indent), this.TreatAsSystemDefault));
				if (!this.TreatAsSystemDefault)
				{
					IAudioEndpointVolume audioEndpointVolume = null;
					IMMDevice device = this._device;
					if (device != null)
					{
						device.Activate(out audioEndpointVolume);
					}
					if (audioEndpointVolume != null)
					{
						float num;
						audioEndpointVolume.GetMasterVolumeLevelScalar(out num);
						sb.AppendLine(string.Format("{0}MasterVolume: {1}", "".PadLeft(indent), num));
						sb.AppendLine(string.Format("{0}VolumeMuted: {1}", "".PadLeft(indent), audioEndpointVolume.GetMute()));
					}
					sb.AppendLine("".PadLeft(indent) + "AudioFormat: " + this._defaultWaveFormat.ToFormatedString());
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine("---------");
				sb.AppendLine(string.Format("DumpDiagnosticInfo: {0}", ex));
				sb.AppendLine("---------");
			}
			this.endLineSeparator(sb, indent);
		}

		private bool _disposed;

		private bool _bAuto;

		private readonly string _id;

		private IMMDevice _device;

		private string _displayName;

		private string _iconPath;

		private string _enumeratorName;

		private string _interfaceName;

		private string _deviceDescription;

		private WaveFormatExtensible _defaultWaveFormat;

		private IAudioMeterInformation _meter;

		private Guid _deviceContainerId = Guid.Empty;

		private HIDHeadset _hidDevice;

		protected Logger _logger = LogWrapper.GetLogger("AudioDevice");
	}
}
