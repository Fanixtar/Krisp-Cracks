using System;
using Krisp.Models;
using Krisp.Properties;

namespace Krisp.Core.Internals
{
	internal class VolumeMappingConfig
	{
		public VolumeMappingConfig(AudioDeviceKind kind)
		{
			if (kind == AudioDeviceKind.Speaker)
			{
				this.MappingMode = VolumeMappingMode.AsIs;
				return;
			}
			this.LockUpVolume = Settings.Default.LockUpVolumeForMic > 0;
		}

		public readonly float VolumeLockMaxConst = 0.98f;

		public readonly float VolumeLockMinHighConst = 0.95f;

		public readonly float VolumeLockMinLowConst = 0.85f;

		public readonly VolumeMappingMode MappingMode = VolumeMappingMode.DoNothing;

		public bool LockUpVolume;
	}
}
