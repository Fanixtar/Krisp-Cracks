﻿using System;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Properties;
using Shared.Analytics;

namespace Krisp.UI.ViewModels
{
	public class OnboardingSetupViewModel : BindableBase
	{
		public OnboardingSetupViewModel(string appName)
		{
			if (appName == "Other App")
			{
				this.AppName = "OtherApp";
				this.Header = TranslationSourceViewModel.Instance["SetupKrispForYourApp"];
			}
			else
			{
				this.AppName = appName;
				this.Header = string.Format(TranslationSourceViewModel.Instance["SetupKrispFor"], appName);
			}
			this.ContactSupportVisible = AccountManager.ShowContactSupport();
		}

		private Logger Logger { get; } = LogWrapper.GetLogger("Onboarding");

		public string AppName { get; set; }

		public string Header { get; set; }

		public TimeSpan Position { get; set; }

		public TimeSpan VideoLength { get; private set; }

		public bool ContactSupportVisible
		{
			get
			{
				return this._contactSupportVisible;
			}
			set
			{
				if (this._contactSupportVisible != value)
				{
					this._contactSupportVisible = value;
					base.RaisePropertyChanged("ContactSupportVisible");
				}
			}
		}

		public string VideoSource
		{
			get
			{
				try
				{
					return Settings.Default[this.AppName + "VideoUrl"] as string;
				}
				catch
				{
					this.Logger.LogError("{0}: Couldn't find this application name in the list. Default video url will be used", new object[] { this.AppName });
				}
				return Settings.Default.OtherAppVideoUrl;
			}
		}

		public void SendVideoAnalytics()
		{
			uint num;
			try
			{
				num = Convert.ToUInt32(this.Position.Seconds);
			}
			catch
			{
				num = 0U;
			}
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.OnboardingVideo(this.AppName, num));
		}

		private bool _contactSupportVisible;
	}
}
