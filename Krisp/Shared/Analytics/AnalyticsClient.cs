using System;
using System.Linq;
using RestSharp;
using Shared.Helpers;

namespace Shared.Analytics
{
	public class AnalyticsClient : RestClient
	{
		public AnalyticsClient(string aUrl, string stoken)
			: base(aUrl)
		{
			base.Timeout = BackEndHelper.MSI_ANALYTIC_REQUEST_TIMEOUT;
			RestClientExtensions.AddDefaultHeader(this, "Authorization", stoken);
		}

		public bool ReportSync(AnalyticEventEx aEvent)
		{
			RestRequest restRequest = new RestRequest(AnalyticsClient.store_endpoint, 1);
			restRequest.AddJsonBody(Enumerable.Repeat<AnalyticEventEx>(aEvent, 1));
			return this.Execute(restRequest).IsSuccessful;
		}

		public static bool ReportSingleEventSync(AnalyticEventEx aEvent)
		{
			ServerInfo analyticInfo = ServerInfoLoader.Instance.AnalyticInfo;
			return new AnalyticsClient(analyticInfo.url, analyticInfo.stoken).ReportSync(aEvent);
		}

		private static readonly string store_endpoint = "/store";
	}
}
