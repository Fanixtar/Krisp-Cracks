using System;
using RestSharp;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class FeatureListRequestInfo : RequestInfo
	{
		public FeatureListRequestInfo()
		{
			base.http_method = Method.GET;
			base.endpoint = "/feature/" + InstallationID.ID;
		}
	}
}
