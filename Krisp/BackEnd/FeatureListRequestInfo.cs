using System;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class FeatureListRequestInfo : RequestInfo
	{
		public FeatureListRequestInfo()
		{
			base.http_method = 0;
			base.endpoint = "/feature/" + InstallationID.ID;
		}
	}
}
