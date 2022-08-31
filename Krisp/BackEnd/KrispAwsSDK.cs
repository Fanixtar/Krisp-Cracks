using System;
using System.Net;
using System.Threading;
using Krisp.AppHelper;
using Krisp.Models;
using RestSharp;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class KrispAwsSDK : RestClient, IKrispSDK
	{
		public ICredentialsPrompt ProxyCredentialsPrompt { get; set; }

		public KrispAwsSDK(string baseUrl, string stoken)
			: base(baseUrl)
		{
			base.Timeout = BackEndHelper.API_REQUEST_TIMEOUT;
			RestClientExtensions.AddDefaultHeader(this, "KrispAuthorization", stoken);
			RestClientExtensions.AddDefaultHeader(this, "User-Agent", BackEndHelper.GetHTTPUserAgent());
			RestClientExtensions.AddDefaultHeader(this, "KRISP_HEADER_APP", "win-" + EnvHelper.KrispVersion.ToString());
			RestClientExtensions.AddDefaultHeader(this, "KRISP_HEADER_ID", InstallationID.ID);
		}

		public void DoAsyncRequest<T>(RequestInfo requestInfo, Action<KrispSDKResponse<T>, object> callback)
		{
			RestRequest restRequest = new RestRequest(requestInfo.endpoint, requestInfo.http_method);
			restRequest.ApplyInfo(requestInfo);
			base.Proxy = ProxyCache.Instance.GetProxy(this.BaseUrl);
			RestClientExtensions.ExecuteAsync<KrispSDKResponse<T>>(this, restRequest, delegate(IRestResponse<KrispSDKResponse<T>> response)
			{
				try
				{
					Logger logger = LogWrapper.GetLogger("KrispRestClient");
					string text = "Response for {0}, Status: {1}, req_id: {2}";
					object[] array = new object[3];
					array[0] = requestInfo.endpoint;
					array[1] = response.StatusCode;
					int num = 2;
					KrispSDKResponse<T> data = response.Data;
					array[num] = ((data != null) ? data.req_id : null);
					logger.LogDebug(text, array);
					if (response.IsProxyAuthRequired() && this.ProxyCredentialsPrompt != null)
					{
						response = this.retryRequestWithProxy<T>(response) ?? response;
					}
					callback(response.Data, response.StatusCode.IsSuccess() ? null : response);
				}
				catch (Exception ex)
				{
					Exception ex3;
					Exception ex2 = ex3;
					Exception ex = ex2;
					LogWrapper.GetLogger("KrispRestClient").LogError("DoAsyncRequest caught an exception: {0}", new object[] { ex });
					ThreadPool.QueueUserWorkItem(delegate(object _)
					{
						throw ex;
					});
				}
			});
		}

		public IRestResponse<KrispSDKResponse<T>> DoRequest<T>(RequestInfo requestInfo)
		{
			RestRequest restRequest = new RestRequest(requestInfo.endpoint, requestInfo.http_method);
			restRequest.ApplyInfo(requestInfo);
			base.Proxy = ProxyCache.Instance.GetProxy(this.BaseUrl);
			IRestResponse<KrispSDKResponse<T>> restResponse = this.Execute<KrispSDKResponse<T>>(restRequest);
			if (restResponse.IsProxyAuthRequired() && this.ProxyCredentialsPrompt != null)
			{
				restResponse = this.retryRequestWithProxy<T>(restResponse) ?? restResponse;
			}
			return restResponse;
		}

		private IRestResponse<KrispSDKResponse<T>> retryRequestWithProxy<T>(IRestResponse resp)
		{
			if (this.ProxyCredentialsPrompt != null)
			{
				try
				{
					KWebProxy proxy = ProxyCache.Instance.GetProxy(this.BaseUrl);
					if (proxy != null)
					{
						NetworkCredential credential = ProxyCache.Instance.GetCredential(proxy.ProxyAddress, "Basic");
						NetworkCredential networkCredential;
						if (((credential != null) ? credential.SecurePassword : null) != null && credential != null && credential.SecurePassword.Length > 0 && resp.Request.Attempts == 1)
						{
							networkCredential = credential;
						}
						else
						{
							networkCredential = this.PromptForCredentials(credential, this.BaseUrl, proxy.ProxyAddress.Host);
						}
						if (networkCredential == null)
						{
							return null;
						}
						if (base.Proxy == null)
						{
							base.Proxy = ProxyCache.Instance.GetProxy(this.BaseUrl);
						}
						base.Proxy.Credentials = networkCredential;
						IRestResponse<KrispSDKResponse<T>> restResponse = this.Execute<KrispSDKResponse<T>>(resp.Request);
						if (restResponse.StatusCode == HttpStatusCode.OK)
						{
							KWebProxy kwebProxy = base.Proxy as KWebProxy;
							if (kwebProxy != null)
							{
								WebRequest.DefaultWebProxy = kwebProxy;
								kwebProxy.StoreIfValidated = true;
								ProxyCache.Instance.UpdateAndStoreCredential(kwebProxy, networkCredential);
							}
						}
						else if (resp.Request.Attempts < 5 && restResponse.IsProxyAuthRequired())
						{
							return this.retryRequestWithProxy<T>(resp);
						}
						return restResponse;
					}
				}
				catch (Exception ex)
				{
					LogWrapper.GetLogger("KrispRestClient").LogError("retryRequestWithProxy caught an exception: {0}", new object[] { ex });
				}
				return null;
			}
			return null;
		}

		private NetworkCredential PromptForCredentials(NetworkCredential cred, Uri forResource, string realm)
		{
			if (cred == null || this.ProxyCredentialsPrompt == null)
			{
				return null;
			}
			NetworkCredential networkCredential = null;
			try
			{
				IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
				Uri uri = ((defaultWebProxy != null) ? defaultWebProxy.GetProxy(forResource) : null);
				CredentialPromptData credentialPromptData = new CredentialPromptData(cred)
				{
					GenericCredentials = true,
					ShowSaveCheckBox = true,
					SaveChecked = true,
					Message = realm
				};
				if (this.ProxyCredentialsPrompt.Prompt(credentialPromptData) == PromptResult.OK)
				{
					networkCredential = new NetworkCredential(credentialPromptData.Credentials.UserName, credentialPromptData.Credentials.SecurePassword);
					ProxyCache.Instance.UpdateCredential(uri, networkCredential);
				}
			}
			catch (Exception ex)
			{
				LogWrapper.GetLogger("KrispRestClient").LogError("PromptForCredentials caught an exception: {0}", new object[] { ex });
			}
			return networkCredential;
		}
	}
}
