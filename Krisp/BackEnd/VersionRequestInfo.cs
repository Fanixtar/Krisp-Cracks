using System;
using Krisp.UI.ViewModels;
using RestSharp;

namespace Krisp.BackEnd
{
	public class VersionRequestInfo : RequestInfo
	{
		public VersionRequestInfo(bool latest)
		{
			base.http_method = Method.GET;
			base.endpoint = "/version/1/win";
			base.endpoint = base.endpoint + "?locale=" + TranslationSourceViewModel.Instance.SelectedCulture.Name;
			if (latest)
			{
				base.endpoint += "&type=last";
			}
		}
	}
}
