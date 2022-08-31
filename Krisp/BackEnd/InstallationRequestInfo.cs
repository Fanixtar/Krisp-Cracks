using System;
using Krisp.AppHelper;
using RestSharp;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class InstallationRequestInfo : RequestInfo
	{
		public InstallationRequestInfo()
		{
			base.http_method = Method.POST;
			base.endpoint = "/installation";
			base.body = new InstallationInfo
			{
				installation_id = InstallationID.ID,
				app_version = EnvHelper.KrispVersion.ToString(),
				app_id = PushNotification.KrispApplicationID,
				push_token = PushNotification.PushToken,
				timezone = SystemInfo.Timezone,
				locale = SystemInfo.Locale,
				os = "win",
				os_version = string.Concat(new string[]
				{
					SystemInfo.WinCaption,
					" ",
					SystemInfo.OsBitness,
					" (",
					SystemInfo.WinReleaseID,
					") ",
					SystemInfo.WinVersion
				}),
				type = SystemInfo.CompName,
				cpu = SystemInfo.CPUModel,
				gpu = SystemInfo.GPUModel,
				device_name = (DeviceLoginHelper.DeviceMode ? (DeviceLoginHelper.FQDNBasedInstallID ? SystemInfo.FQDN : SystemInfo.HostName) : null)
			};
		}
	}
}
