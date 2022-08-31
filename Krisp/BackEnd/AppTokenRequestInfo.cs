using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class AppTokenRequestInfo : RequestInfo
	{
		public AppTokenRequestInfo(string jwt)
		{
			base.http_method = Method.GET;
			base.endpoint = "/auth/token/app";
			base.headers = new Headers
			{
				JWT = jwt
			};
		}
	}
}
