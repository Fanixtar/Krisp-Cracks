using System;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public sealed class KrispSDKFactory
	{
		public static IKrispSDK Instance
		{
			get
			{
				if (KrispSDKFactory.krispSDKInstance == null)
				{
					object obj = KrispSDKFactory.lockobj;
					lock (obj)
					{
						if (KrispSDKFactory.krispSDKInstance == null)
						{
							ServerInfo krispSDKInfo = ServerInfoLoader.Instance.KrispSDKInfo;
							KrispSDKFactory.krispSDKInstance = new KrispAwsSDK(krispSDKInfo.url, krispSDKInfo.stoken);
						}
					}
				}
				return KrispSDKFactory.krispSDKInstance;
			}
		}

		private static IKrispSDK krispSDKInstance = null;

		private static readonly object lockobj = new object();
	}
}
