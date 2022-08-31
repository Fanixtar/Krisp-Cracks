using System;
using Krisp.Models;

namespace Krisp.Core.Internals
{
	public class KrispControlStatus : IKrispControlStatus
	{
		public event EventHandler<ADMStateFlags> StateChanged;

		public event EventHandler<bool> StreamActivityChanged;

		public event EventHandler<StatusMessage> NotifyStatusMessage;

		public KrispControlStatus(AudioDeviceKind kind)
		{
			this._kind = kind;
		}

		public void ChangeStFlag(ADMStateFlags st = ADMStateFlags.HealtyState)
		{
			this._lastSTFlag = st;
			EventHandler<ADMStateFlags> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, this._lastSTFlag);
		}

		public void NotifyMessage(StatusMessage msg)
		{
			EventHandler<StatusMessage> notifyStatusMessage = this.NotifyStatusMessage;
			if (notifyStatusMessage == null)
			{
				return;
			}
			notifyStatusMessage(this, msg);
		}

		public void SetStreamActivityStatus(bool status)
		{
			EventHandler<bool> streamActivityChanged = this.StreamActivityChanged;
			if (streamActivityChanged == null)
			{
				return;
			}
			streamActivityChanged(this, status);
		}

		private ADMStateFlags _lastSTFlag;

		private AudioDeviceKind _kind;
	}
}
