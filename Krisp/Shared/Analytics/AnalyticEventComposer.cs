using System;

namespace Shared.Analytics
{
	public static class AnalyticEventComposer
	{
		public static AnalyticEventEx InstallEvent()
		{
			return new AnalyticEventEx(AnalyticEventComposer.ANALYTIC_EVENT_INSTALL);
		}

		public static AnalyticEventEx UpdateEvent()
		{
			return new AnalyticEventEx(AnalyticEventComposer.ANALYTIC_EVENT_UPDATE);
		}

		public static AnalyticEventEx UninstallEvent()
		{
			return new AnalyticEventEx(AnalyticEventComposer.ANALYTIC_EVENT_UNINSTALL);
		}

		public static AnalyticEvent RepairEvent(bool accept)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_REPAIR)
			{
				label1 = (accept ? "accept" : "cancel")
			};
		}

		public static AnalyticEvent UpdateErrorEvent(string error, string message)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_UPDATE_ERROR)
			{
				label1 = error,
				large_label1 = message
			};
		}

		public static AnalyticEvent AppRunEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_APP_RUN);
		}

		public static AnalyticEvent AppQuitEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_APP_QUIT);
		}

		public static AnalyticEvent DeviceChangeEvent(bool spk, string audiodev, uint sRate)
		{
			return new AnalyticEvent(spk ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_CHANGE : AnalyticEventComposer.ANALYTIC_EVENT_MIC_CHANGE)
			{
				label1 = audiodev,
				value1 = sRate
			};
		}

		public static AnalyticEvent NCSwitchedEvent(bool spk, bool state)
		{
			return new AnalyticEvent(spk ? (state ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_MUTE_ON : AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_MUTE_OFF) : (state ? AnalyticEventComposer.ANALYTIC_EVENT_MIC_MUTE_ON : AnalyticEventComposer.ANALYTIC_EVENT_MIC_MUTE_OFF));
		}

		public static AnalyticEvent NCStateSwitchedEvent(bool spk, bool state)
		{
			return new AnalyticEvent(spk ? (state ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_MUTE_ENABLED : AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_MUTE_DISABLED) : (state ? AnalyticEventComposer.ANALYTIC_EVENT_MIC_MUTE_ENABLED : AnalyticEventComposer.ANALYTIC_EVENT_MIC_MUTE_DISABLED));
		}

		public static AnalyticEvent DeviceSelectedEvent(bool spk, string audiodev, bool def)
		{
			return new AnalyticEvent(spk ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_SELCTED : AnalyticEventComposer.ANALYTIC_EVENT_MIC_SELCTED)
			{
				label1 = audiodev,
				value1 = (def ? 1U : 0U)
			};
		}

		public static AnalyticEvent CheckForUpdate()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_CHECK_FOR_UPDATE);
		}

		public static AnalyticEvent UpdatePopupEvent(string version)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_UPDATE_POPUP)
			{
				label1 = version
			};
		}

		public static AnalyticEvent UpdateAcceptedEvent(string version)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_UPDATE_ACCEPTED)
			{
				label1 = version
			};
		}

		public static AnalyticEvent AboutEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_ABOUT);
		}

		public static AnalyticEvent ReportEvent(string error = null)
		{
			if (error == null)
			{
				return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_REPORT);
			}
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_REPORT)
			{
				label1 = "error",
				large_label1 = ((error.Length > 900) ? error.Substring(0, 900) : error)
			};
		}

		public static AnalyticEvent ReportGetDebugLogsEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_REPORT_GET_DEBUG_LGOS);
		}

		public static AnalyticEvent ChatEvent(bool Onboarding = false)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_CHAT)
			{
				label1 = (Onboarding ? "onboarding" : "")
			};
		}

		public static AnalyticEvent ShareSlackEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_SLACK);
		}

		public static AnalyticEvent ShareDiscordEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_DISCORD);
		}

		public static AnalyticEvent ShareTweetEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TWEET);
		}

		public static AnalyticEvent ShareFacebookEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_FACEBOOK);
		}

		public static AnalyticEvent ShareReferralEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_REFERRAL_SHARE);
		}

		public static AnalyticEvent CreateTeamEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_CREATE_TEAM);
		}

		public static AnalyticEvent StreamStatsEvent(bool spk, string audiodev, uint strm_dur, uint cln_dur, uint sample_rate, string strWfex, string sdkModel)
		{
			return new AnalyticEvent(spk ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_STREAM_STATS : AnalyticEventComposer.ANALYTIC_EVENT_MIC_STREAM_STATS)
			{
				label1 = audiodev,
				label2 = sdkModel,
				value1 = strm_dur,
				value2 = cln_dur,
				value3 = sample_rate,
				large_label1 = strWfex
			};
		}

		public static AnalyticEvent CallEndEvent(bool spk, string appname, uint dur)
		{
			return new AnalyticEvent(spk ? AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_CALL_END : AnalyticEventComposer.ANALYTIC_EVENT_MIC_CALL_END)
			{
				label1 = appname,
				value1 = dur
			};
		}

		public static AnalyticEvent KrispDeviceInactiveEvent(bool spk)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_KRISP_INACTIVE)
			{
				value1 = (spk ? 10U : 11U)
			};
		}

		public static AnalyticEvent KrispDeviceMissingEvent(bool spk)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_KRISP_MISSING)
			{
				value1 = (spk ? 10U : 11U)
			};
		}

		public static AnalyticEvent StreamDisconnectedEvent(bool spk, string audiodev)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_STREAM_DISCONNECT)
			{
				label1 = audiodev,
				value1 = (spk ? 10U : 11U)
			};
		}

		public static AnalyticEvent StreamStartErrorEvent(bool spk, string audiodev)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_STREAM_START_ERROR)
			{
				label1 = audiodev,
				value1 = (spk ? 10U : 11U)
			};
		}

		public static AnalyticEvent UnhealthyEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_UNHEALTY_STATE);
		}

		public static AnalyticEvent DuckingNotifiedEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_DUCKING_NOTIFIED);
		}

		public static AnalyticEvent DuckingClickedEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_DUCKING_CLICKED);
		}

		public static AnalyticEvent FeatureNotApplicable(string fname)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_FEATURE_NA)
			{
				label1 = fname
			};
		}

		public static AnalyticEvent SigninAttemptEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_SIGNIN_ATTEPT);
		}

		public static AnalyticEvent SigninSuccessEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_SIGNIN_SUCCESS);
		}

		public static AnalyticEvent SignoutEvent(bool manual)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_SIGNOUT)
			{
				label1 = (manual ? "manual" : "rejected")
			};
		}

		public static AnalyticEvent TryAgainEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TRY_AGAIN);
		}

		public static AnalyticEvent UnlockEvent(bool trialEnded)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_UNLOCK)
			{
				label1 = (trialEnded ? "expired" : "trial")
			};
		}

		public static AnalyticEvent SigningProblemEvent(string desc)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_SYNCING_PROBLEM)
			{
				label1 = (string.IsNullOrWhiteSpace(desc) ? "unknown" : desc)
			};
		}

		public static AnalyticEvent TestKrispInitEvent(bool isSignedIn)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_INIT)
			{
				label1 = (isSignedIn ? "signedin" : "signedout")
			};
		}

		public static AnalyticEvent TestKrispRecStartEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_REC_START);
		}

		public static AnalyticEvent TestKrispRecErrorEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_REC_ERROR);
		}

		public static AnalyticEvent TestKrispTryAgainEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_TRY_AGAIN);
		}

		public static AnalyticEvent TestKrispRecAgainEvent()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_REC_AGAIN);
		}

		public static AnalyticEvent TestKrispRecEndEvent(string micName, uint duration)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_REC_END)
			{
				label1 = micName,
				value1 = duration
			};
		}

		public static AnalyticEvent TestKrispToggleEvent(bool on)
		{
			return new AnalyticEvent(on ? AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_TOGGLE_ON : AnalyticEventComposer.ANALYTIC_EVENT_TESTKRISP_TOGGLE_OFF);
		}

		public static AnalyticEvent OnboardingStartSetup(bool initial)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_ONBOARDING_START_SETUP)
			{
				label1 = (initial ? "initial" : "menu")
			};
		}

		public static AnalyticEvent OnboardingHelp()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_ONBOARDING_HELP);
		}

		public static AnalyticEvent OnboardingVideo(string appName, uint duration)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_ONBOARDING_VIDEO)
			{
				label1 = appName,
				value1 = duration
			};
		}

		public static AnalyticEvent Preferences()
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES);
		}

		public static AnalyticEvent PreferencesSelectorLanguage(string languageTag)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES_SELECTOR_LANGUAGE)
			{
				label1 = languageTag
			};
		}

		public static AnalyticEvent PreferencesLanguageRelaunchResponse(bool relaunch)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES_RELAUNCH)
			{
				label1 = (relaunch ? "Relaunch" : "Cancel")
			};
		}

		public static AnalyticEvent PreferencesLaunchAtStartup(bool enabled)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES_LAUNCH_AT_STARTUP)
			{
				label1 = (enabled ? "Enable" : "Disable")
			};
		}

		public static AnalyticEvent PreferencesEchoCancellation(bool enabled)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES_ECHO_CANCELLATION)
			{
				label1 = (enabled ? "Enable" : "Disable")
			};
		}

		public static AnalyticEvent PreferencesLockMicLevel(bool enabled)
		{
			return new AnalyticEvent(AnalyticEventComposer.ANALYTIC_EVENT_PREFERENCES_LOCK_MIC_LEVEL)
			{
				label1 = (enabled ? "Enable" : "Disable")
			};
		}

		public static AnalyticEventEx DroppedEvent(string key, uint count)
		{
			return new AnalyticEventEx(AnalyticEventComposer.ANALYTIC_EVENT_DROPED)
			{
				label1 = key,
				value1 = count
			};
		}

		public static readonly string ANALYTIC_EVENT_INSTALL = "install";

		public static readonly string ANALYTIC_EVENT_UPDATE = "update";

		public static readonly string ANALYTIC_EVENT_UNINSTALL = "uninstall";

		public static readonly string ANALYTIC_EVENT_REPAIR = "repair";

		public static readonly string ANALYTIC_EVENT_UPDATE_ERROR = "update_error";

		public static readonly string ANALYTIC_EVENT_APP_RUN = "run";

		public static readonly string ANALYTIC_EVENT_APP_QUIT = "quit";

		public static readonly string ANALYTIC_EVENT_MIC_CHANGE = "mic_change";

		public static readonly string ANALYTIC_EVENT_SPEAKER_CHANGE = "speaker_change";

		public static readonly string ANALYTIC_EVENT_MIC_MUTE_ON = "mic_mute_on";

		public static readonly string ANALYTIC_EVENT_MIC_MUTE_OFF = "mic_mute_off";

		public static readonly string ANALYTIC_EVENT_SPEAKER_MUTE_ON = "speaker_mute_on";

		public static readonly string ANALYTIC_EVENT_SPEAKER_MUTE_OFF = "speaker_mute_off";

		public static readonly string ANALYTIC_EVENT_MIC_MUTE_ENABLED = "mic_mute_enabled";

		public static readonly string ANALYTIC_EVENT_MIC_MUTE_DISABLED = "mic_mute_disabled";

		public static readonly string ANALYTIC_EVENT_SPEAKER_MUTE_ENABLED = "speaker_mute_enabled";

		public static readonly string ANALYTIC_EVENT_SPEAKER_MUTE_DISABLED = "speaker_mute_disabled";

		public static readonly string ANALYTIC_EVENT_MIC_SELCTED = "select_mic_change";

		public static readonly string ANALYTIC_EVENT_SPEAKER_SELCTED = "select_speaker_change";

		public static readonly string ANALYTIC_EVENT_ABOUT = "about";

		public static readonly string ANALYTIC_EVENT_CHAT = "chat";

		public static readonly string ANALYTIC_EVENT_CHECK_FOR_UPDATE = "check_for_update";

		public static readonly string ANALYTIC_EVENT_UPDATE_POPUP = "update_popup";

		public static readonly string ANALYTIC_EVENT_UPDATE_ACCEPTED = "update_menu_final";

		public static readonly string ANALYTIC_EVENT_SLACK = "slack";

		public static readonly string ANALYTIC_EVENT_DISCORD = "discord";

		public static readonly string ANALYTIC_EVENT_TWEET = "tweet";

		public static readonly string ANALYTIC_EVENT_FACEBOOK = "facebook";

		public static readonly string ANALYTIC_EVENT_REFERRAL_SHARE = "referral_share";

		public static readonly string ANALYTIC_EVENT_CREATE_TEAM = "team.create";

		public static readonly string ANALYTIC_EVENT_MIC_CALL_END = "mic_call_end";

		public static readonly string ANALYTIC_EVENT_MIC_STREAM_STATS = "mic_stream_stats";

		public static readonly string ANALYTIC_EVENT_SPEAKER_CALL_END = "speaker_call_end";

		public static readonly string ANALYTIC_EVENT_SPEAKER_STREAM_STATS = "speaker_stream_stats";

		public static readonly string ANALYTIC_EVENT_KRISP_INACTIVE = "krisp_device_inactive";

		public static readonly string ANALYTIC_EVENT_KRISP_MISSING = "krisp_device_missing";

		public static readonly string ANALYTIC_EVENT_STREAM_DISCONNECT = "stream_disconnected";

		public static readonly string ANALYTIC_EVENT_STREAM_START_ERROR = "stream_start_error";

		public static readonly string ANALYTIC_EVENT_UNHEALTY_STATE = "unhealty_state";

		public static readonly string ANALYTIC_EVENT_DUCKING_NOTIFIED = "notification_ducking_shown";

		public static readonly string ANALYTIC_EVENT_DUCKING_CLICKED = "notification_ducking_clicked";

		public static readonly string ANALYTIC_EVENT_FEATURE_NA = "feature_not_applicable";

		public static readonly string ANALYTIC_EVENT_SIGNIN_ATTEPT = "signin_attempt";

		public static readonly string ANALYTIC_EVENT_SIGNIN_SUCCESS = "signin_success";

		public static readonly string ANALYTIC_EVENT_SIGNOUT = "signout";

		public static readonly string ANALYTIC_EVENT_TRY_AGAIN = "try_again";

		public static readonly string ANALYTIC_EVENT_UNLOCK = "unlock";

		public static readonly string ANALYTIC_EVENT_SYNCING_PROBLEM = "problem-with-syncing";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_INIT = "test_krisp.testKrisp";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_REC_START = "test_krisp.recordStart";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_REC_ERROR = "test_krisp.errorWhileRecord";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_TRY_AGAIN = "test_krisp.tryAgain";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_REC_AGAIN = "test_krisp.recordAgain";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_REC_END = "test_krisp.recordEnd";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_TOGGLE_ON = "test_krisp.toggleOn";

		public static readonly string ANALYTIC_EVENT_TESTKRISP_TOGGLE_OFF = "test_krisp.toggleOff";

		public static readonly string ANALYTIC_EVENT_ONBOARDING_START_SETUP = "onboarding.startSetup";

		public static readonly string ANALYTIC_EVENT_ONBOARDING_HELP = "onboarding.help";

		public static readonly string ANALYTIC_EVENT_ONBOARDING_VIDEO = "onboarding.video";

		public static readonly string ANALYTIC_EVENT_PREFERENCES = "preferences.preferences";

		public static readonly string ANALYTIC_EVENT_PREFERENCES_SELECTOR_LANGUAGE = "preferences.selectorLanguage";

		public static readonly string ANALYTIC_EVENT_PREFERENCES_RELAUNCH = "preferences.relaunchResponse";

		public static readonly string ANALYTIC_EVENT_PREFERENCES_LAUNCH_AT_STARTUP = "preferences.launchAtStartup";

		public static readonly string ANALYTIC_EVENT_PREFERENCES_ECHO_CANCELLATION = "preferences.echoCancellation";

		public static readonly string ANALYTIC_EVENT_PREFERENCES_LOCK_MIC_LEVEL = "preferences.lockMicLevel";

		public static readonly string ANALYTIC_EVENT_REPORT = "report";

		public static readonly string ANALYTIC_EVENT_REPORT_GET_DEBUG_LGOS = "report.getDebugLogs";

		public static readonly string ANALYTIC_EVENT_DROPED = "droped";
	}
}
