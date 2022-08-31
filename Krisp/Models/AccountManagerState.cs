using System;

namespace Krisp.Models
{
	public enum AccountManagerState
	{
		Uninitialized = -1,
		LoggedIn,
		LoggingIn,
		LoggedOut,
		NoInternetConnection,
		GeneralError
	}
}
