using System;

namespace Krisp.BackEnd
{
	public class AppTokenRequestInfo : RequestInfo
	{
		public AppTokenRequestInfo(string jwt)
		{
			base.http_method = 0;
			base.endpoint = "/auth/token/app";
			base.headers = new Headers
			{
				JWT = jwt
			};
		}
	}
}
