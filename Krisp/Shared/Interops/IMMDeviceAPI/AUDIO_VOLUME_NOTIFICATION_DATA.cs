﻿using System;

namespace Shared.Interops.IMMDeviceAPI
{
	public struct AUDIO_VOLUME_NOTIFICATION_DATA
	{
		public Guid guidEventContext;

		public int bMuted;

		public float fMasterVolume;

		public uint nChannels;

		public IntPtr afChannelVolumes;
	}
}
