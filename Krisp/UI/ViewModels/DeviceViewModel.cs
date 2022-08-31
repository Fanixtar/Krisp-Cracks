using System;
using System.ComponentModel;
using Krisp.Models;
using Shared.Interops;

namespace Krisp.UI.ViewModels
{
	public class DeviceViewModel : BindableBase
	{
		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		public string EnumeratorName
		{
			get
			{
				return this._device.EnumeratorName;
			}
		}

		public string DeviceDescription
		{
			get
			{
				return this._device.DeviceDescription;
			}
		}

		public WaveFormatExtensible DefaultWaveFormat
		{
			get
			{
				return this._device.DefaultWaveFormat;
			}
		}

		public IAudioDevice AudioDevice
		{
			get
			{
				return this._device;
			}
		}

		public string Id
		{
			get
			{
				return this._device.Id;
			}
		}

		protected DeviceViewModel()
		{
		}

		public DeviceViewModel(IAudioDevice device)
		{
			this._device = device;
			this._displayName = device.DisplayName.Replace('\n', ' ');
			this._device.PropertyChanged += this.Device_PropertyChanged;
		}

		~DeviceViewModel()
		{
			this._device.PropertyChanged -= this.Device_PropertyChanged;
		}

		private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "DisplayName")
			{
				this._displayName = this._device.DisplayName.Replace('\n', ' ');
				base.RaisePropertyChanged("DisplayName");
				return;
			}
			if (e.PropertyName == "DefaultWaveFormat")
			{
				base.RaisePropertyChanged("DefaultWaveFormat");
			}
		}

		private readonly IAudioDevice _device;

		private string _displayName;
	}
}
