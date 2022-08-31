using System;
using System.ComponentModel;
using Shared.Interops;

namespace Krisp.Models
{
	public interface IAudioDevice : INotifyPropertyChanged, IDiagnosticsBase
	{
		string Id { get; }

		string DisplayName { get; }

		string IconPath { get; }

		string EnumeratorName { get; }

		string InterfaceName { get; }

		string DeviceDescription { get; }

		bool TreatAsSystemDefault { get; }

		AudioDeviceKind Kind { get; }

		WaveFormatExtensible DefaultWaveFormat { get; }
	}
}
