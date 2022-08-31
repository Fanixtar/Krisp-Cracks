using System;

namespace Shared.Interops
{
	public static class KrispDevicePublic
	{
		public static readonly string GUID_DEVINTF_KRISP = "1F9B2A19-C266-4E0F-9A32-214A285C24D1";

		public static readonly uint BASE = 20480U;

		public static readonly uint DEVICE_SPEAKER = 1U;

		public static readonly uint DEVICE_MIC = 2U;

		public static readonly uint DEVICE_PRESENCE = 8U;

		public static readonly uint WAVE_FORMAT_EVENT = 61440U;

		public static readonly uint NOTIFY_START = 1U;

		public static readonly uint NOTIFY_DISCONNECT = 2U;

		public static readonly uint IOCTL_KRISP_GET_FORMAT = Kernel32.CTL_CODE(Kernel32.FILE_DEVICE_UNKNOWN, KrispDevicePublic.BASE + 1U, Kernel32.METHOD_BUFFERED, Kernel32.FILE_READ_ACCESS);

		public static readonly uint IOCTL_KRISP_PRESENT = Kernel32.CTL_CODE(Kernel32.FILE_DEVICE_UNKNOWN, KrispDevicePublic.BASE + 3U, Kernel32.METHOD_BUFFERED, Kernel32.FILE_WRITE_ACCESS);

		public static readonly uint IOCTL_KRISP_NOTIFY = Kernel32.CTL_CODE(Kernel32.FILE_DEVICE_UNKNOWN, KrispDevicePublic.BASE + 4U, Kernel32.METHOD_BUFFERED, Kernel32.FILE_READ_ACCESS);

		public static readonly uint IOCTL_KRISP_VOLUME_TABLE = Kernel32.CTL_CODE(Kernel32.FILE_DEVICE_UNKNOWN, KrispDevicePublic.BASE + 5U, Kernel32.METHOD_BUFFERED, Kernel32.FILE_WRITE_ACCESS);
	}
}
