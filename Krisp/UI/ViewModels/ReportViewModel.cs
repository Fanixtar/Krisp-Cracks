using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Input;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using RestSharp;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	public class ReportViewModel
	{
		public string Description { get; set; }

		public string Email { get; set; }

		public string EmailLabel { get; set; }

		public string Mode { get; set; }

		public string AdditionalInfo { get; private set; }

		public bool IsLoggedIn
		{
			get
			{
				return this._accMngr.IsLoggedIn;
			}
		}

		public bool IncludeSysInfo { get; set; } = true;

		public bool IncludeRecordings { get; set; } = true;

		public List<string> AdditionalFiles { get; set; }

		public bool UserMode { get; } = !DeviceLoginHelper.DeviceMode;

		public ICommand SendReportCommand
		{
			get
			{
				return new RelayCommand(delegate(object param)
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.ReportEvent());
					new ProgressWindow(() => this.ReportBug()).Show();
				});
			}
		}

		public ReportViewModel(ReportSource source = ReportSource.manual, List<string> additionalFiles = null, string preMessage = null)
		{
			this._accMngr = ServiceContainer.Instance.GetService<IAccountManager>();
			this.EmailLabel = TranslationSourceViewModel.Instance[(!DeviceLoginHelper.DeviceMode) ? "Email" : "Device"];
			UserProfileInfo userProfileInfo = this._accMngr.UserProfileInfo;
			this.Email = ((userProfileInfo != null) ? userProfileInfo.email : null);
			this.Description = preMessage ?? "";
			UserProfileInfo userProfileInfo2 = this._accMngr.UserProfileInfo;
			this.Mode = ((userProfileInfo2 != null) ? userProfileInfo2.mode.name : null);
			this._reportSource = source;
			this.AdditionalFiles = additionalFiles ?? new List<string>();
			this.AdditionalInfo = string.Format("Krisp v{0} running on {1} {2} {3}", new object[]
			{
				EnvHelper.KrispVersion.ToString(),
				Environment.Is64BitOperatingSystem ? "x64" : "x86",
				RunModeChecker.Mode.ToString(),
				InstallationID.ID
			});
		}

		public string GenerateReportFile(string selectedPath = null)
		{
			string text = "";
			string text2 = "";
			bool flag = false;
			int num = 0;
			try
			{
				this._logger.LogInfo("Collecting KrispApp related info");
				string text3 = string.Format("{0}_{1}_v{2}_{3}", new object[]
				{
					DateTime.UtcNow.ToString("yyyy.MM.dd-HH-mm-ss", CultureInfo.CreateSpecificCulture("en-US")),
					InstallationID.ID,
					EnvHelper.KrispVersion.ToString(),
					this._reportSource.ToString()
				});
				text = EnvHelper.KrispAppLocalFolder + "\\" + text3;
				text2 = (selectedPath ?? text) + "\\" + text3 + ".zip";
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("From: " + this.Email);
				stringBuilder.AppendLine(string.Format("Date: {0} (UTC: {1})", DateTime.Now, DateTime.UtcNow));
				stringBuilder.AppendLine("Plan: " + this.Mode);
				stringBuilder.AppendLine(string.Format("IsLoggedIn: {0}", this.IsLoggedIn));
				stringBuilder.AppendLine(string.Format("SysInfoIncluded: {0}", this.IncludeSysInfo));
				stringBuilder.AppendLine(string.Format("RecordingsIncluded: {0}", this.IncludeRecordings && this._reportSource == ReportSource.testnc));
				stringBuilder.AppendLine("AddidionalInfo: " + this.AdditionalInfo);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Description: \r\n=====\r\n" + this.Description + "\r\n=====");
				File.WriteAllText(text + "\\description.txt", stringBuilder.ToString());
				stringBuilder.Clear();
				stringBuilder.AppendLine(string.Format("ReportDate: {0}", DateTime.Now));
				stringBuilder.AppendLine("IsChmo: " + (ReportHelper.IsAdmin() ? "no" : "yes"));
				stringBuilder.AppendLine("IsElevatedUser: " + (ReportHelper.IsElevatedUser() ? "yes" : "no"));
				stringBuilder.AppendLine("DuckingMode: " + AudioEngineHelper.GetDuckingMode().ToString());
				this.AppendKrispControllerInfo(ref stringBuilder, num);
				File.WriteAllText(text + "\\appdata.txt", stringBuilder.ToString());
				stringBuilder.Clear();
				if (this.IncludeSysInfo)
				{
					stringBuilder.AppendLine(string.Format("ReportDate: {0}", DateTime.Now));
					using (Process currentProcess = Process.GetCurrentProcess())
					{
						stringBuilder.AppendLine(string.Format("PrivateMemorySize64: {0}", currentProcess.PrivateMemorySize64 / 1024L));
						stringBuilder.AppendLine(string.Format("GCTotalMemory: {0}", GC.GetTotalMemory(false) / 1024L)).AppendLine();
					}
					stringBuilder.AppendLine("HardwareIdentifier: " + InstallationID.HardwareIdentifier).AppendLine();
					stringBuilder.AppendLine("MicrophoneAccessPrivacySettings: " + AudioEngineHelper.MicrophoneAccessPrivacySettings()).AppendLine();
					ReportHelper.dumpMicrophoneAccessPerApplication(stringBuilder);
					stringBuilder.AppendLine();
					ReportHelper.dumpInstalledAudioDevices(stringBuilder);
					stringBuilder.AppendLine();
					ReportHelper.dumpAppSpecificDefaultsDevices(stringBuilder);
					stringBuilder.AppendLine();
					uint num2 = Settings.Default.DumpEventLogDays;
					num2 = ((num2 > 0U && num2 < 100U) ? num2 : 7U);
					ReportHelper.dumpEventLog(stringBuilder, EventLogEntryType.Information, "Application", DateTime.Now.Subtract(TimeSpan.FromDays(num2)), "Krisp");
					stringBuilder.AppendLine();
					ReportHelper.dumpEventLog(stringBuilder, EventLogEntryType.Information, "System", DateTime.Now.Subtract(TimeSpan.FromDays(num2)), "Krisp");
					stringBuilder.AppendLine();
					ReportHelper.dumpInstalledApps(stringBuilder);
					stringBuilder.AppendLine();
					ReportHelper.GetInstalledServices(stringBuilder);
					File.WriteAllText(text + "\\sysinfo.txt", stringBuilder.ToString());
				}
				this._logger.LogInfo("Reporting ...");
				ReportHelper.CopyLogFiles(EnvHelper.KrispAppLogFolder, text);
				ReportHelper.CopyCacheAndConfig(text);
				if (this._reportSource == ReportSource.testnc && this.IncludeRecordings)
				{
					ReportHelper.moveAdditionalFileCopies(this.AdditionalFiles, text);
				}
				ReportHelper.ZipDirectory(text, text2);
				flag = true;
			}
			catch (Exception ex)
			{
				this._logger.LogError("Exception thrown during bug reporting: {0}", new object[] { ex.Message });
			}
			finally
			{
				try
				{
					if (Directory.Exists(text))
					{
						Directory.Delete(text, true);
					}
				}
				catch (Exception ex2)
				{
					this._logger.LogError("Exception thrown during deletion of '{0}' folder: {1}", new object[] { text, ex2.Message });
				}
			}
			if (!flag)
			{
				return null;
			}
			return text2;
		}

		private bool ReportBug()
		{
			bool flag = false;
			string text = this.GenerateReportFile(EnvHelper.KrispAppLocalFolder);
			try
			{
				string text2 = AccountManager.Instance.ReportProblem(this.Description, this.IncludeSysInfo, this._reportSource == ReportSource.testnc && this.IncludeRecordings);
				if (text2 != null)
				{
					long length = new FileInfo(text).Length;
					FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
					BinaryReader binaryReader = new BinaryReader(fileStream);
					byte[] array = binaryReader.ReadBytes((int)length);
					binaryReader.Close();
					fileStream.Close();
					IRestResponse<KrispSDKResponse<string>> restResponse = KrispSDKFactory.Instance.DoRequest<string>(new UploadDebugInfoRequestInfo(text2, array));
					KrispSDKResponse<string> data = restResponse.Data;
					if (restResponse.StatusCode.IsSuccess())
					{
						this._logger.LogInfo("UploadDebugInfo succeeded.");
						return true;
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Exception thrown during bug reporting: {0}", new object[] { ex.Message });
			}
			finally
			{
				try
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				catch (Exception ex2)
				{
					this._logger.LogError("Exception thrown during deletion of '{0}' file: {1}", new object[] { text, ex2.Message });
				}
			}
			return flag;
		}

		private void AppendKrispControllerInfo(ref StringBuilder sb, int indent)
		{
			IKrispController krispController = DataModelFactory.KrispController(AudioDeviceKind.Speaker);
			sb.AppendLine("Core's InternalInfo +" + DiagExtensions.DIAG_LINE_SEPARATOR + "+");
			if (krispController != null)
			{
				krispController.DumpDiagnosticInfo(sb, indent + 4);
			}
			krispController = DataModelFactory.KrispController(AudioDeviceKind.Microphone);
			if (krispController != null)
			{
				krispController.DumpDiagnosticInfo(sb, indent + 4);
			}
			sb.AppendLine("Core's InternalInfo -" + DiagExtensions.DIAG_LINE_SEPARATOR + "-");
		}

		private IAccountManager _accMngr;

		private Logger _logger = LogWrapper.GetLogger("ReportBug");

		private readonly ReportSource _reportSource;
	}
}
