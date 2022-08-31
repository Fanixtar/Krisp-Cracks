using System;

namespace Shared.Helpers
{
	public static class UrlProvider
	{
		public static string GetContactSupportUrl(string languageTag)
		{
			return string.Format("{0}resource/chat?user_id={1}&locale={2}", ServerInfoLoader.Instance.KrispSDKInfo.url, InstallationID.ID, languageTag);
		}

		public static string GetLearnMoreUrl(string languageTag)
		{
			return string.Format("{0}resource/learn_more?user_id={1}&locale={2}", ServerInfoLoader.Instance.KrispSDKInfo.url, InstallationID.ID, languageTag);
		}

		public static string GetPrivacyPolicyUrl()
		{
			return "https://krisp.ai/privacy-policy/";
		}

		public static string GetTermsOfUseUrl()
		{
			return "https://krisp.ai/terms-of-use/";
		}

		public static string GetHelpdeskUrl()
		{
			return "https://help.krisp.ai";
		}

		public static string GetPingConnectionUrl()
		{
			return "https://api.krisp.ai/v2/";
		}
	}
}
