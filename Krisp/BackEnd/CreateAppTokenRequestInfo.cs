using System;
using RestSharp;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class CreateAppTokenRequestInfo : RequestInfo
	{
		public CreateAppTokenRequestInfo(string jwt)
		{
			base.http_method = Method.POST;
			base.endpoint = "/auth/token/device";
			base.headers = new Headers
			{
				JWT = jwt
			};
			if (DeviceLoginHelper.FQDNBasedInstallID)
			{
				base.body = new DeviceKeyConfig
				{
					installation_id_mech = "fqdn"
				};
			}
		}
	}
}
