using System;
using System.Runtime.InteropServices;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	internal class AudioEndpointVolumeCallback : IAudioEndpointVolumeCallback
	{
		private AudioEndpointVolumeCallback()
		{
		}

		public AudioEndpointVolumeCallback(VolumeMapper mapper, bool isKrisp = false)
		{
			this._mapper = new WeakReference<VolumeMapper>(mapper);
			this._isKrisp = isKrisp;
		}

		public void OnNotify(IntPtr pNotify)
		{
			VolumeMapper volumeMapper;
			if (pNotify != IntPtr.Zero && this._mapper.TryGetTarget(out volumeMapper))
			{
				try
				{
					AUDIO_VOLUME_NOTIFICATION_DATA audio_VOLUME_NOTIFICATION_DATA = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotify);
					volumeMapper.OnNotifyVolumeChanged(audio_VOLUME_NOTIFICATION_DATA, this._isKrisp);
				}
				catch
				{
				}
			}
		}

		private readonly WeakReference<VolumeMapper> _mapper;

		private readonly bool _isKrisp;
	}
}
