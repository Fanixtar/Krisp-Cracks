using System;

namespace Krisp.Models
{
	public enum AccountManagerErrorCode
	{
		UNINITIALIZED,
		JWT_TEAM_NOT_FOUND,
		NOT_EMPTY_SEAT,
		DEVICE_BLOCKED
	}
}
