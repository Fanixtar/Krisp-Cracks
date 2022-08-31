using System;
using System.Net;

namespace Krisp.BackEnd
{
	public class KrispSDKResponse<T>
	{
		public ResponseCode code { get; set; }

		public string error_code { get; set; }

		public string message { get; set; }

		public HttpStatusCode http_code { get; set; }

		public string req_id { get; set; }

		public T data { get; set; }
	}
}
