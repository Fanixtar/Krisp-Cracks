using System;
using Krisp.Core;
using Krisp.Core.Internals;

namespace Krisp.Models
{
	public class DataModelFactory
	{
		public static KrispActivityNotificationClient CreateKrispActivityNotificationClient()
		{
			object obj = DataModelFactory.s_kLocker;
			lock (obj)
			{
				if (DataModelFactory.s_KrispActivityClient == null)
				{
					DataModelFactory.s_KrispActivityClient = new KrispActivityNotificationClient();
				}
			}
			return DataModelFactory.s_KrispActivityClient;
		}

		internal static void DestroyKrispActivityNotificationClient()
		{
			if (DataModelFactory.s_KrispActivityClient != null)
			{
				DataModelFactory.s_KrispActivityClient.Dispose();
				DataModelFactory.s_KrispActivityClient = null;
			}
		}

		public static IAppCore CreateAppCore()
		{
			if (DataModelFactory.s_appCore == null)
			{
				DataModelFactory.s_appCore = new AppCore();
			}
			return DataModelFactory.s_appCore;
		}

		public static void DestroyAppCore()
		{
			if (DataModelFactory.s_appCore != null)
			{
				DataModelFactory.s_appCore.Dispose();
				DataModelFactory.s_appCore = null;
			}
		}

		public static IKrispController KrispController(AudioDeviceKind kind)
		{
			if (kind == AudioDeviceKind.Speaker)
			{
				IAppCore appCore = DataModelFactory.s_appCore;
				if (appCore == null)
				{
					return null;
				}
				return appCore.SpeakerController;
			}
			else
			{
				IAppCore appCore2 = DataModelFactory.s_appCore;
				if (appCore2 == null)
				{
					return null;
				}
				return appCore2.MicController;
			}
		}

		public static IKrispControlStatus CreateKrispControlStatus(AudioDeviceKind kind)
		{
			if (kind == AudioDeviceKind.Speaker)
			{
				if (DataModelFactory.s_InboundStreamControlStatus == null)
				{
					DataModelFactory.s_InboundStreamControlStatus = new KrispControlStatus(kind);
				}
				return DataModelFactory.s_InboundStreamControlStatus;
			}
			if (DataModelFactory.s_OutboundStreamControlStatus == null)
			{
				DataModelFactory.s_OutboundStreamControlStatus = new KrispControlStatus(kind);
			}
			return DataModelFactory.s_OutboundStreamControlStatus;
		}

		public static IStreamProcessor SPInstance
		{
			get
			{
				return StreamProcessor.Instance;
			}
		}

		private static IAppCore s_appCore;

		private static IKrispControlStatus s_InboundStreamControlStatus;

		private static IKrispControlStatus s_OutboundStreamControlStatus;

		private static KrispActivityNotificationClient s_KrispActivityClient;

		private static object s_kLocker = new object();
	}
}
