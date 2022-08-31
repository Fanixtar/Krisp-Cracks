using System;
using System.Collections.Specialized;
using Shared.Interops;

namespace Krisp.Models
{
	public interface IKrispController : IDiagnosticsBase, IDisposable
	{
		event EventHandler DeviceManagerLoaded;

		event NotifyCollectionChangedEventHandler DeviceCollectionChanged;

		event EventHandler<IAudioDevice> WorkingDeviceChanged;

		IAudioDeviceManager DeviceManager { get; }

		IAudioDeviceCollection AllDevices { get; }

		bool? ForceKrispAsSystemDefault { get; }

		IAudioDevice WorkingDevice { get; }

		void SelectedDeviceChanged(object sender, IAudioDevice audioDevice);

		void StoreSelectedDeviceID(IAudioDevice dev);

		void OnNCSwitch(object sender, bool e);

		void OnNCModeChange(object sender, SPFeature e);

		HRESULT ScanStates(IAudioDevice selDev = null);

		void NotifyError(ADMStateFlags stf);

		int GetStreamActivityLevel();

		void SetKrispDeviceWatcherHolder(IWatcherHandlerHolder wh);

		void StartProcessing();
	}
}
