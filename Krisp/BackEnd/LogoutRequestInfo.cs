using System;

namespace Krisp.BackEnd
{
	public class LogoutRequestInfo : RequestInfo
	{
		public LogoutRequestInfo(string appToken)
		{
			base.http_method = 0;
			base.endpoint = "/auth/logout";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
		}
	}
}
