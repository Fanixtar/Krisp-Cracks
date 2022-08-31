using System;
using System.Windows.Threading;
using Krisp.Models;

namespace Krisp.UI.ViewModels
{
	public class WatcherHandlerHolder : IWatcherHandlerHolder
	{
		public event EventHandler<IAppInfo> SessionDisconnected;

		public event EventHandler<IAppInfo> SessionConnected;

		public WatcherHandlerHolder()
		{
			this._dispatcher = Dispatcher.CurrentDispatcher;
		}

		public void ConnectSession(IAppInfo s)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				EventHandler<IAppInfo> sessionConnected = this.SessionConnected;
				if (sessionConnected == null)
				{
					return;
				}
				sessionConnected(this, s);
			});
		}

		public void DisconnectSession(IAppInfo s)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				EventHandler<IAppInfo> sessionDisconnected = this.SessionDisconnected;
				if (sessionDisconnected == null)
				{
					return;
				}
				sessionDisconnected(this, s);
			});
		}

		private Dispatcher _dispatcher;
	}
}
