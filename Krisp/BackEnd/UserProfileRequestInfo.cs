using System;

namespace Krisp.BackEnd
{
	public class UserProfileRequestInfo : RequestInfo
	{
		public UserProfileRequestInfo(string appToken)
		{
			base.http_method = 0;
			base.endpoint = "/user/profile/2/1";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
		}
	}
}
