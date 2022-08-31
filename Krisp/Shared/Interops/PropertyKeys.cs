using System;

namespace Shared.Interops
{
	public static class PropertyKeys
	{
		public static PROPERTYKEY PKEY_ItemNameDisplay = new PROPERTYKEY
		{
			fmtid = new Guid("{B725F130-47EF-101A-A5F1-02608C9EEBAC}"),
			pid = new UIntPtr(10U)
		};

		public static PROPERTYKEY PKEY_AppUserModel_Background = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{86D40B4D-9069-443C-819A-2A54090DCCEC}"),
			pid = new UIntPtr(4U)
		};

		public static PROPERTYKEY PKEY_AppUserModel_PackageInstallPath = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"),
			pid = new UIntPtr(15U)
		};

		public static PROPERTYKEY PKEY_Tile_SmallLogoPath = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{86D40B4D-9069-443C-819A-2A54090DCCEC}"),
			pid = new UIntPtr(2U)
		};

		public static PROPERTYKEY PKEY_AppUserModel_PackageFullName = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"),
			pid = new UIntPtr(21U)
		};

		public static PROPERTYKEY PKEY_Device_FriendlyName = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0xa45c254e, 0xdf1c, 0x4efd, {0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0}}"),
			pid = new UIntPtr(14U)
		};

		public static PROPERTYKEY PKEY_AudioEndPoint_Interface = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{a45c254e-df1c-4efd-8020-67d146a850e0}"),
			pid = new UIntPtr(2U)
		};

		public static PROPERTYKEY PKEY_AudioEndpoint_PhysicalSpeakers = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0x1da5d803, 0xd492, 0x4edd, {0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e}}"),
			pid = new UIntPtr(3U)
		};

		public static PROPERTYKEY DEVPKEY_Device_DeviceDesc = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0xa45c254e, 0xdf1c, 0x4efd, {0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0}}"),
			pid = new UIntPtr(2U)
		};

		public static PROPERTYKEY DEVPKEY_Device_EnumeratorName = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0xa45c254e, 0xdf1c, 0x4efd, {0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0}}"),
			pid = new UIntPtr(24U)
		};

		public static PROPERTYKEY DEVPKEY_DeviceClass_IconPath = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0x259abffc, 0x50a7, 0x47ce, {0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66}}"),
			pid = new UIntPtr(12U)
		};

		public static PROPERTYKEY DEVPKEY_DeviceInterface_FriendlyName = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0x026e516e, 0xb814, 0x414b, {0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22}}"),
			pid = new UIntPtr(2U)
		};

		public static PROPERTYKEY PKEY_DeviceInterface_Enabled = new PROPERTYKEY
		{
			fmtid = Guid.Parse("{0x026e516e, 0xb814, 0x414b, {0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22}}"),
			pid = new UIntPtr(3U)
		};

		public static PROPERTYKEY PKEY_AudioEngine_DeviceFormat = new PROPERTYKEY
		{
			fmtid = new Guid(4053730893U, 2092, 20007, 188, 115, 104, 130, 161, 187, 142, 76),
			pid = new UIntPtr(0U)
		};

		public static PROPERTYKEY PKEY_Devices_ContainerId = new PROPERTYKEY
		{
			fmtid = new Guid("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C"),
			pid = new UIntPtr(2U)
		};
	}
}
