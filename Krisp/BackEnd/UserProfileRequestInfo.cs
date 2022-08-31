using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class UserProfileRequestInfo : RequestInfo
	{
		public UserProfileRequestInfo(string appToken)
		{
			base.http_method = Method.GET;
			base.endpoint = "/user/profile/2/1";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
		}
	}
}
