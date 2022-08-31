using System;
using Krisp.Models;
using RestSharp;

namespace Krisp.BackEnd
{
	public interface IKrispSDK
	{
		ICredentialsPrompt ProxyCredentialsPrompt { get; set; }

		void DoAsyncRequest<T>(RequestInfo requestInfo, Action<KrispSDKResponse<T>, object> callback);

		IRestResponse<KrispSDKResponse<T>> DoRequest<T>(RequestInfo requestInfo);
	}
}
