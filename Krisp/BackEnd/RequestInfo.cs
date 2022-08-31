using System;
using RestSharp;

namespace Krisp.BackEnd
{
	public class RequestInfo
	{
		protected RequestInfo()
		{
		}

		public Method http_method { get; protected set; }

		public string endpoint { get; protected set; }

		public Headers headers { get; protected set; }

		public object body { get; protected set; }

		public Parameter[] parameters { get; protected set; }
	}
}
