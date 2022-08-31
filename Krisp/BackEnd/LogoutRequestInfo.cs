using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class LogoutRequestInfo : RequestInfo
	{
		public LogoutRequestInfo(string appToken)
		{
			base.http_method = Method.GET;
			base.endpoint = "/auth/logout";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
		}
	}
}
