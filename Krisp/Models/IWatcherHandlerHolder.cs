using System;

namespace Krisp.Models
{
	public interface IWatcherHandlerHolder
	{
		event EventHandler<IAppInfo> SessionDisconnected;

		event EventHandler<IAppInfo> SessionConnected;

		void DisconnectSession(IAppInfo s);

		void ConnectSession(IAppInfo s);
	}
}
