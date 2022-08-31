using System;

namespace Krisp.BackEnd
{
	public enum ResponseCode : uint
	{
		Success,
		AuthenticationError,
		NotFound = 1015U,
		ValidationError,
		NetworkError = 1022U,
		Forbidden,
		InternalServerError = 10000U
	}
}
