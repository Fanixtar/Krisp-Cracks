using System;
using System.Net;
using System.Reflection;
using RestSharp;

namespace Krisp.BackEnd
{
	internal static class ExtensionMethods
	{
		public static void ApplyInfo(this RestRequest request, RequestInfo requestInfo)
		{
			if (requestInfo.parameters != null)
			{
				foreach (Parameter parameter in requestInfo.parameters)
				{
					request.AddParameter(parameter);
				}
			}
			if (requestInfo.headers != null)
			{
				foreach (FieldInfo fieldInfo in typeof(Headers).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				{
					object value = fieldInfo.GetValue(requestInfo.headers);
					if (value != null)
					{
						request.AddHeader(fieldInfo.Name, value.ToString());
					}
				}
			}
			if (requestInfo.body != null)
			{
				request.AddJsonBody(requestInfo.body);
			}
		}

		internal static bool IsSuccess(this HttpStatusCode httpStatusCode)
		{
			return httpStatusCode >= HttpStatusCode.OK && httpStatusCode < HttpStatusCode.MultipleChoices;
		}

		internal static bool IsProxyAuthRequired(this IRestResponse resp)
		{
			if (resp.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
			{
				return true;
			}
			if (resp.StatusCode == (HttpStatusCode)0)
			{
				bool flag = resp is HttpWebResponse;
				WebException ex = resp.ErrorException as WebException;
				if (flag && ex != null)
				{
					HttpWebResponse httpWebResponse = ex.Response as HttpWebResponse;
					if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
