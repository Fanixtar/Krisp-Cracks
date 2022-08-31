using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.UI.ViewModels;
using Shared.Analytics;

namespace Krisp.UI.Views.Windows
{
	public partial class ShareWindow : Window
	{
		public ShareWindow(UserProfileInfo userProfileInfo)
		{
			this.InitializeComponent();
			this._currentCopiedInfo = this.ReferalCopiedInfo;
			this._hideCopiedInfoTimer = new TimerHelper();
			this._hideCopiedInfoTimer.Interval = 2000.0;
			this._hideCopiedInfoTimer.AutoReset = false;
			this._hideCopiedInfoTimer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs eventArgs)
			{
				base.Dispatcher.Invoke(delegate()
				{
					this._currentCopiedInfo.Visibility = Visibility.Hidden;
				});
			};
			if (userProfileInfo.team == null)
			{
				this._discordShareUrl = userProfileInfo.ref_string;
				this._slackShareUrl = userProfileInfo.ref_string;
				this._twitterShareUrl = "https://twitter.com/intent/tweet?url=" + userProfileInfo.ref_string + "&text=Wow,%20just%20tried%20%40krispHQ.%20Works%20amazing.%20Maybe%20a%20new%20must-have%20tool%20for%20remote%20teams%3F%20%23letsfightnoisetogether";
				this._facebookShareUrl = "https://www.facebook.com/sharer?u=" + userProfileInfo.ref_string;
				this.TeamMessage.Text = string.Format(this._teamShareText, this._discordKeyword, userProfileInfo.ref_string);
				this.TeamMessage.Tag = ShareWindow.ShareChannel.Discord;
				this.PersonalReferalShareFrame.Visibility = Visibility.Visible;
				this.ReferalLink.Text = userProfileInfo.ref_string;
				return;
			}
			this._discordShareUrl = "https://krisp.ai/?utm_source=inapp&utm_medium=discord";
			this._slackShareUrl = "https://krisp.ai/?utm_source=inapp&utm_medium=slack";
			this._twitterShareUrl = "https://twitter.com/intent/tweet?url=https%3A%2F%2Fkrisp.ai%2F%3Futm_source%3Dinapp%26utm_medium%3Dtwitter&text=Wow,%20just%20tried%20%40krispHQ.%20Works%20amazing.%20Maybe%20a%20new%20must-have%20tool%20for%20remote%20teams%3F%20%23letsfightnoisetogether";
			this._facebookShareUrl = "https://www.facebook.com/sharer?u=https%3A%2F%2Fkrisp.ai%2F%3Futm_source%3Dinapp%26utm_medium%3Dfacebook";
			this.TeamMessage.Text = string.Format(this._teamShareText, this._discordKeyword, this._discordShareUrl);
			this.PersonalReferalShareFrame.Visibility = Visibility.Collapsed;
		}

		private void ShareOnTwitterPressed(object sender, MouseButtonEventArgs e)
		{
			Helpers.OpenUrl(this._twitterShareUrl);
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.ShareTweetEvent());
		}

		private void ShareOnFacebookPressed(object sender, MouseButtonEventArgs e)
		{
			Helpers.OpenUrl(this._facebookShareUrl);
			AnalyticsFactory.Instance.Report(AnalyticEventComposer.ShareFacebookEvent());
		}

		private void CopyReferralLink(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Clipboard.SetText(this.ReferalLink.Text);
				this._currentCopiedInfo.Visibility = Visibility.Hidden;
				this._hideCopiedInfoTimer.Stop();
				this._currentCopiedInfo = this.ReferalCopiedInfo;
				this._currentCopiedInfo.Visibility = Visibility.Visible;
				this._hideCopiedInfoTimer.Start();
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.ShareReferralEvent());
			}
			catch
			{
			}
		}

		private void CopyTeamMessage(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Clipboard.SetText(this.TeamMessage.Text);
				this._currentCopiedInfo.Visibility = Visibility.Hidden;
				this._hideCopiedInfoTimer.Stop();
				this._currentCopiedInfo = this.TeamMessageCopiedInfo;
				this._currentCopiedInfo.Visibility = Visibility.Visible;
				this._hideCopiedInfoTimer.Start();
				object tag = this.TeamMessage.Tag;
				object obj;
				if (tag != null && (obj = tag) is ShareWindow.ShareChannel)
				{
					ShareWindow.ShareChannel shareChannel = (ShareWindow.ShareChannel)obj;
					int num = (int)shareChannel;
					if (num != 0)
					{
						if (num == 1)
						{
							AnalyticsFactory.Instance.Report(AnalyticEventComposer.ShareSlackEvent());
						}
					}
					else
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.ShareDiscordEvent());
					}
				}
			}
			catch
			{
			}
		}

		private void DiscordSelected(object sender, MouseButtonEventArgs e)
		{
			this.TeamMessage.Text = string.Format(this._teamShareText, this._discordKeyword, this._discordShareUrl);
			this.TeamMessage.Tag = ShareWindow.ShareChannel.Discord;
			this.DiscordBorder.BorderBrush = new SolidColorBrush(this._selectedColor);
			this.SlackBorder.BorderBrush = new SolidColorBrush(this._deselectedColor);
		}

		private void SlackSelected(object sender, MouseButtonEventArgs e)
		{
			this.TeamMessage.Text = string.Format(this._teamShareText, this._slackKeyword, this._slackShareUrl);
			this.TeamMessage.Tag = ShareWindow.ShareChannel.Slack;
			this.DiscordBorder.BorderBrush = new SolidColorBrush(this._deselectedColor);
			this.SlackBorder.BorderBrush = new SolidColorBrush(this._selectedColor);
		}

		private TimerHelper _hideCopiedInfoTimer;

		private UIElement _currentCopiedInfo;

		private string _discordShareUrl;

		private string _slackShareUrl;

		private string _twitterShareUrl;

		private string _facebookShareUrl;

		private string _teamShareText = TranslationSourceViewModel.Instance["ShareMessage2"];

		private string _slackKeyword = "@channel";

		private string _discordKeyword = "@here";

		private Color _selectedColor = (Color)ColorConverter.ConvertFromString("#6371DE");

		private Color _deselectedColor = (Color)ColorConverter.ConvertFromString("#ECECEC");

		private enum ShareChannel
		{
			Discord,
			Slack
		}
	}
}
