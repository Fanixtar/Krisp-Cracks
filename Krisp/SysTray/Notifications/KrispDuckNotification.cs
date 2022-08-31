using System;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.UI.ViewModels;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.SysTray.Notifications
{
	public class KrispDuckNotification : INotification
	{
		public KrispDuckNotification()
		{
			this.Title = TranslationSourceViewModel.Instance["DuckNotificationTitle"];
			this.Text = TranslationSourceViewModel.Instance["DuckNotificationText"];
			this.Handler = delegate()
			{
				LogWrapper.GetLogger("Notification").LogInfo("Ducking notification clicked");
				if (AudioEngineHelper.IsDuckingDisabled() && !AudioEngineHelper.SetDuckingMode(AudioEngineHelper.DuckingMode.Reduce_the_volume_by_80))
				{
					LogWrapper.GetLogger("Notification").LogWarning("Fail to set the ducking mode");
				}
				AudioEngineHelper.LaunchSystemSoundSettings(3);
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.DuckingClickedEvent());
			};
		}

		public string Title { get; private set; }

		public string Text { get; private set; }

		public Action Handler { get; private set; }
	}
}
