using System;
using RestSharp;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class ReportProblemRequestInfo : RequestInfo
	{
		public ReportProblemRequestInfo(string appToken, string description, bool hasSystemInfo, bool hasAudio)
		{
			base.http_method = Method.POST;
			base.endpoint = "/report/problem/url";
			base.headers = new Headers
			{
				Authorization = "Bearer " + appToken
			};
			ReportInfoEx reportInfoEx;
			if (!(description == ""))
			{
				(reportInfoEx = new ReportInfoEx()).description = description;
			}
			else
			{
				reportInfoEx = new ReportInfo();
			}
			ReportInfo reportInfo = reportInfoEx;
			reportInfo.os = "win";
			reportInfo.installation_id = InstallationID.ID;
			reportInfo.version = EnvHelper.KrispVersion.ToString();
			reportInfo.has_system_info = (hasSystemInfo ? 1 : 0);
			reportInfo.has_audio = (hasAudio ? 1 : 0);
			base.body = reportInfo;
		}
	}
}
