using System;

namespace Krisp.BackEnd
{
	public class CreateAppTokenRequestInfo : RequestInfo
	{
		public CreateAppTokenRequestInfo(string jwt)
		{
			base.http_method = 1;
			base.endpoint = "/auth/token/device";
			base.headers = new Headers
			{
				JWT = jwt
			};
		}
	}
}
