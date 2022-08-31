using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Krisp.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string LastSelectedSpeaker
		{
			get
			{
				return (string)this["LastSelectedSpeaker"];
			}
			set
			{
				this["LastSelectedSpeaker"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string LastSelectedMic
		{
			get
			{
				return (string)this["LastSelectedMic"];
			}
			set
			{
				this["LastSelectedMic"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool SpeakerNCState
		{
			get
			{
				return (bool)this["SpeakerNCState"];
			}
			set
			{
				this["SpeakerNCState"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool MicrophoneNCState
		{
			get
			{
				return (bool)this["MicrophoneNCState"];
			}
			set
			{
				this["MicrophoneNCState"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("360")]
		public uint FeatureRequestPeriod
		{
			get
			{
				return (uint)this["FeatureRequestPeriod"];
			}
			set
			{
				this["FeatureRequestPeriod"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("5")]
		public uint ActivityThreshold
		{
			get
			{
				return (uint)this["ActivityThreshold"];
			}
			set
			{
				this["ActivityThreshold"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool UpgradeRequired
		{
			get
			{
				return (bool)this["UpgradeRequired"];
			}
			set
			{
				this["UpgradeRequired"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool DemoMode
		{
			get
			{
				return (bool)this["DemoMode"];
			}
			set
			{
				this["DemoMode"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("21600000")]
		public double FetchUesrProfileIntervalMilisec
		{
			get
			{
				return (double)this["FetchUesrProfileIntervalMilisec"];
			}
			set
			{
				this["FetchUesrProfileIntervalMilisec"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool DuckingDisabled
		{
			get
			{
				return (bool)this["DuckingDisabled"];
			}
			set
			{
				this["DuckingDisabled"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("2")]
		public uint LogLevel
		{
			get
			{
				return (uint)this["LogLevel"];
			}
			set
			{
				this["LogLevel"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public string KrispSpeakerAsSystemDefault
		{
			get
			{
				return (string)this["KrispSpeakerAsSystemDefault"];
			}
			set
			{
				this["KrispSpeakerAsSystemDefault"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string KrispMicrophoneAsSystemDefault
		{
			get
			{
				return (string)this["KrispMicrophoneAsSystemDefault"];
			}
			set
			{
				this["KrispMicrophoneAsSystemDefault"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string InstallationGuid
		{
			get
			{
				return (string)this["InstallationGuid"];
			}
			set
			{
				this["InstallationGuid"] = value;
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/skype_onboard_win_en_v1.mp4")]
		public string SkypeVideoUrl
		{
			get
			{
				return (string)this["SkypeVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/slack_onboard_win_en_v1.mp4")]
		public string SlackVideoUrl
		{
			get
			{
				return (string)this["SlackVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/hangouts_onboard_win_en_v1.mp4")]
		public string HangoutsVideoUrl
		{
			get
			{
				return (string)this["HangoutsVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/webex_onboard_win_en_v1.mp4")]
		public string WebexVideoUrl
		{
			get
			{
				return (string)this["WebexVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/discord_onboard_win_en_v1.mp4")]
		public string DiscordVideoUrl
		{
			get
			{
				return (string)this["DiscordVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/zoom_onboard_win_en_v1.mp4")]
		public string ZoomVideoUrl
		{
			get
			{
				return (string)this["ZoomVideoUrl"];
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("http://cdn.krisp.ai/win/res/video/onboard/en/general_onboard_win_en_v1.mp4")]
		public string OtherAppVideoUrl
		{
			get
			{
				return (string)this["OtherAppVideoUrl"];
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool DismissTrialEndNotification
		{
			get
			{
				return (bool)this["DismissTrialEndNotification"];
			}
			set
			{
				this["DismissTrialEndNotification"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("7")]
		public uint DumpEventLogDays
		{
			get
			{
				return (uint)this["DumpEventLogDays"];
			}
			set
			{
				this["DumpEventLogDays"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("")]
		public string Language
		{
			get
			{
				return (string)this["Language"];
			}
			set
			{
				this["Language"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool SpeakerRoomModeOn
		{
			get
			{
				return (bool)this["SpeakerRoomModeOn"];
			}
			set
			{
				this["SpeakerRoomModeOn"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool MicrophoneRoomModeOn
		{
			get
			{
				return (bool)this["MicrophoneRoomModeOn"];
			}
			set
			{
				this["MicrophoneRoomModeOn"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool FirstRun
		{
			get
			{
				return (bool)this["FirstRun"];
			}
			set
			{
				this["FirstRun"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("98")]
		public int LockUpVolumeForMic
		{
			get
			{
				return (int)this["LockUpVolumeForMic"];
			}
			set
			{
				this["LockUpVolumeForMic"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("False")]
		public bool EchoCancellationState
		{
			get
			{
				return (bool)this["EchoCancellationState"];
			}
			set
			{
				this["EchoCancellationState"] = value;
			}
		}

		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
