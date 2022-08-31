using System;

namespace Krisp.Models
{
	public class AccountManagerStateChangedEventArgs : ActionResultArgs
	{
		public AccountManagerState State { get; private set; }

		public AccountManagerErrorCode ReasonCode { get; private set; }

		public AccountManagerStateChangedEventArgs(AccountManagerState state)
		{
			this.State = state;
		}

		public AccountManagerStateChangedEventArgs(AccountManagerState state, AccountManagerErrorCode reasonCode)
		{
			this.State = state;
			this.ReasonCode = reasonCode;
		}
	}
}
