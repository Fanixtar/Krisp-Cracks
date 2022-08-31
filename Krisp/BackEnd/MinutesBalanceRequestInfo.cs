using System;

namespace Krisp.BackEnd
{
	public class MinutesBalanceRequestInfo : RequestInfo
	{
		public MinutesBalanceRequestInfo(string appToken, MinutesUsage minutesUsage)
		{
			base.http_method = ((minutesUsage == null) ? 0 : 1);
			base.endpoint = "/user/minutes/balance";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
			base.body = minutesUsage;
		}
	}
}
