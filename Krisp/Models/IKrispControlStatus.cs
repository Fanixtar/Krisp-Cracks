using System;

namespace Krisp.Models
{
	public interface IKrispControlStatus
	{
		event EventHandler<ADMStateFlags> StateChanged;

		event EventHandler<bool> StreamActivityChanged;

		event EventHandler<StatusMessage> NotifyStatusMessage;

		void ChangeStFlag(ADMStateFlags st);

		void NotifyMessage(StatusMessage msg);

		void SetStreamActivityStatus(bool status);
	}
}
