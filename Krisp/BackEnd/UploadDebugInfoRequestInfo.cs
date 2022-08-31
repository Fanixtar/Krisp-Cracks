using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class UploadDebugInfoRequestInfo : RequestInfo
	{
		public UploadDebugInfoRequestInfo(string url, byte[] documentBytes)
		{
			base.http_method = Method.PUT;
			base.endpoint = url;
			base.parameters = new Parameter[]
			{
				new Parameter("application/zip", documentBytes, ParameterType.RequestBody)
			};
		}
	}
}
