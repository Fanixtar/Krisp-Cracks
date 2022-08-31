using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.AppHelper
{
	internal class NetworkHelper
	{
		public event EventHandler NetworkConnectionRestored;

		public static NetworkHelper Instance
		{
			get
			{
				NetworkHelper networkHelper;
				if ((networkHelper = NetworkHelper._instance) == null)
				{
					networkHelper = (NetworkHelper._instance = new NetworkHelper());
				}
				return networkHelper;
			}
		}

		~NetworkHelper()
		{
			NetworkChange.NetworkAddressChanged -= this.OnNetworkAddressChanged;
		}

		public void StartMonitoring()
		{
			NetworkChange.NetworkAddressChanged += this.OnNetworkAddressChanged;
			this._bChecking = false;
		}

		private void OnNetworkAddressChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this._bChecking && NetworkInterface.GetIsNetworkAvailable())
				{
					this._bChecking = true;
					Task.Run(delegate()
					{
						if (WinINet.InternetCheckConnection(UrlProvider.GetPingConnectionUrl(), 1, 0))
						{
							NetworkChange.NetworkAddressChanged -= this.OnNetworkAddressChanged;
							EventHandler networkConnectionRestored = this.NetworkConnectionRestored;
							if (networkConnectionRestored != null)
							{
								networkConnectionRestored(this, EventArgs.Empty);
							}
						}
						this._bChecking = false;
					});
				}
			}
			catch
			{
			}
		}

		private static NetworkHelper _instance;

		private bool _bChecking;
	}
}
