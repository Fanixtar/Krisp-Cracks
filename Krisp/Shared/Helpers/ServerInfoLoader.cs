using System;

namespace Shared.Helpers
{
	public class ServerInfoLoader
	{
		public ServerInfo KrispSDKInfo { get; private set; }

		public ServerInfo AnalyticInfo { get; private set; }

		public ServerInfo FrontendInfo { get; private set; }

		private ServerInfoLoader()
		{
			RunModeChecker.RunMode mode = RunModeChecker.Mode;
			if (mode <= RunModeChecker.RunMode.NgRok)
			{
				if (mode == RunModeChecker.RunMode.Local)
				{
					this.KrispSDKInfo = new ServerInfo
					{
						url = "http://localhost:4567/v2/",
						stoken = "Basic YXBwOk14V3BNY2NOVVk0MkZSTnpSdlhxSlhFVDVnN1dnWXpDQ2pGOEc="
					};
					this.AnalyticInfo = new ServerInfo
					{
						url = "https://stage.analytics.krisp.ai/v1/",
						stoken = "Bearer YW5hbHl0aWNzOkpzU3VRdWxrc2pzZGZTSmR5cGtq="
					};
					this.FrontendInfo = new ServerInfo
					{
						url = "http://localhost:3000/"
					};
					return;
				}
				if (mode == RunModeChecker.RunMode.NgRok)
				{
					this.KrispSDKInfo = new ServerInfo
					{
						url = "https://krisp.ngrok.io/v2/",
						stoken = "Basic YXBwOk14V3BNY2NOVVk0MkZSTnpSdlhxSlhFVDVnN1dnWXpDQ2pGOEc="
					};
					this.AnalyticInfo = new ServerInfo
					{
						url = "https://stage.analytics.krisp.ai/v1/",
						stoken = "Bearer YW5hbHl0aWNzOkpzU3VRdWxrc2pzZGZTSmR5cGtq="
					};
					this.FrontendInfo = new ServerInfo
					{
						url = "https://krispapp.ngrok.io/"
					};
					return;
				}
			}
			else
			{
				if (mode == RunModeChecker.RunMode.Development)
				{
					this.KrispSDKInfo = new ServerInfo
					{
						url = "https://dev.api.krisp.ai/v2/",
						stoken = "Basic YXBwOk14V3BNY2NOVVk0MkZSTnpSdlhxSlhFVDVnN1dnWXpDQ2pGOEc="
					};
					this.AnalyticInfo = new ServerInfo
					{
						url = "https://stage.analytics.krisp.ai/v1/",
						stoken = "Bearer YW5hbHl0aWNzOkpzU3VRdWxrc2pzZGZTSmR5cGtq="
					};
					this.FrontendInfo = new ServerInfo
					{
						url = "https://dev.account.krisp.ai/"
					};
					return;
				}
				if (mode == RunModeChecker.RunMode.Staging)
				{
					this.KrispSDKInfo = new ServerInfo
					{
						url = "https://stage.api.krisp.ai/v2/",
						stoken = "Basic YXBwOk14V3BNY2NOVVk0MkZSTnpSdlhxSlhFVDVnN1dnWXpDQ2pGOEc="
					};
					this.AnalyticInfo = new ServerInfo
					{
						url = "https://stage.analytics.krisp.ai/v1/",
						stoken = "Bearer YW5hbHl0aWNzOkpzU3VRdWxrc2pzZGZTSmR5cGtq="
					};
					this.FrontendInfo = new ServerInfo
					{
						url = "https://stage.account.krisp.ai/"
					};
					return;
				}
			}
			this.KrispSDKInfo = new ServerInfo
			{
				url = "https://api.krisp.ai/v2/",
				stoken = "Basic YXBwOk14V3BNY2NOVVk0MkZSTnpSdlhxSlhFVDVnN1dnWXpDQ2pGOEc="
			};
			this.AnalyticInfo = new ServerInfo
			{
				url = "https://analytics.krisp.ai/v1/",
				stoken = "Bearer YW5hbHl0aWNzOkpzU3VRdWxrc2pzZGZTSmR5cGtq="
			};
			this.FrontendInfo = new ServerInfo
			{
				url = "https://account.krisp.ai/"
			};
		}

		public static ServerInfoLoader Instance
		{
			get
			{
				if (ServerInfoLoader.instance == null)
				{
					ServerInfoLoader.instance = new ServerInfoLoader();
				}
				return ServerInfoLoader.instance;
			}
		}

		private static ServerInfoLoader instance;
	}
}
