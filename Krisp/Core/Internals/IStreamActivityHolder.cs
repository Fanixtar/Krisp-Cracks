using System;

namespace Krisp.Core.Internals
{
	public interface IStreamActivityHolder
	{
		event EventHandler<StreamActivityState> StreamActivityChanged;

		void RegisterActivityNotifications();

		void UnRegisterActivityNotifications();
	}
}
