using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class MinutesBalanceRequestInfo : RequestInfo
	{
		public MinutesBalanceRequestInfo(string appToken, MinutesUsage minutesUsage)
		{
			base.http_method = ((minutesUsage == null) ? Method.GET : Method.POST);
			base.endpoint = "/user/minutes/balance";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
			base.body = minutesUsage;
		}
	}
}
