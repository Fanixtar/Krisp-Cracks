using System;
using Krisp.UI.ViewModels;

namespace Krisp.BackEnd
{
	public class VersionRequestInfo : RequestInfo
	{
		public VersionRequestInfo(bool latest)
		{
			base.http_method = 0;
			base.endpoint = "/version/1/win";
			base.endpoint = base.endpoint + "?locale=" + TranslationSourceViewModel.Instance.SelectedCulture.Name;
			if (latest)
			{
				base.endpoint += "&type=last";
			}
		}
	}
}
