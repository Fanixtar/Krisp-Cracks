using System;
using System.Collections.Generic;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using Krisp.Services;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class MinutesModeViewModel : AppModeViewModel
	{
		public MinutesBalance MinuteModeProps
		{
			get
			{
				return this._minutesModeProps;
			}
			set
			{
				this._minutesModeProps = value;
				if (this.NCCountingEnabled)
				{
					if (this._minutesModeProps.nc_out.balance <= this._currentNCUsage.used_seconds)
					{
						this.StopCounting();
						this.DisableToggles();
					}
					else
					{
						this.NCEnds = this.MinuteModeProps.nc_out.balance - this._currentNCUsage.used_seconds;
					}
				}
				else
				{
					this.NCEnds = this.MinuteModeProps.nc_out.balance;
					if (this._minutesModeProps.nc_out.balance > 0U)
					{
						this.EnableToggles();
					}
					else
					{
						this.DisableToggles();
					}
				}
				this._nextIndex = 0U;
				this.RangeEnds = this.MinuteModeProps.nc_out.range_ends;
				base.RaisePropertyChanged("RangeEnds");
				base.RaisePropertyChanged("MinuteModeProps");
			}
		}

		public uint NCEnds
		{
			get
			{
				return this._ncEnds;
			}
			private set
			{
				if (value != this._ncEnds)
				{
					this._ncEnds = value;
					base.RaisePropertyChanged("NCEnds");
				}
			}
		}

		public uint RangeEnds { get; private set; }

		public KrispControllerViewModel MicController { get; set; }

		public KrispControllerViewModel SpeakerController { get; set; }

		public bool MicHasNCStream
		{
			get
			{
				return this.MicController.FilteredUsingApps.Count > 0 && this.MicController.NCSwitch;
			}
		}

		public bool SpeakerHasNCStream
		{
			get
			{
				return this.SpeakerController.FilteredUsingApps.Count > 0 && this.SpeakerController.NCSwitch;
			}
		}

		public bool HasNCStream
		{
			get
			{
				return this.MicHasNCStream || this.SpeakerHasNCStream;
			}
		}

		public bool NCCountingEnabled
		{
			get
			{
				return this._ncCountingEnabled;
			}
			private set
			{
				if (this._ncCountingEnabled != value)
				{
					this._ncCountingEnabled = value;
					base.RaisePropertyChanged("NCCountingEnabled");
				}
			}
		}

		private static IAccountManager AcmService
		{
			get
			{
				return ServiceContainer.Instance.GetService<IAccountManager>();
			}
		}

		public MinutesModeViewModel(KrispControllerViewModel micController, KrispControllerViewModel speakerController)
		{
			base.Mode = "minutes";
			this._minutesModeProps = new MinutesBalance();
			this._minutesModeProps.nc_out = new NCBalance();
			this._minutesModeProps.nc_out.template = new NCBalance.NCTemplate();
			MinutesModeViewModel.AcmService.MinutesBalanceChanged += this.OnBalanceReceived;
			this.MicController = micController;
			this.SpeakerController = speakerController;
			this.NCEnds = 0U;
			this.RangeEnds = 0U;
			this._timer = new TimerHelper();
			this._timer.AutoReset = true;
			this._timer.Elapsed += this.OnTimerElapsed;
			this._appList = new HashSet<string>();
		}

		public override void AttachHandlers()
		{
			this.MicController.StreamStarted += this.OnMicStreamStarted;
			this.MicController.StreamStopped += this.OnMicStreamStopped;
			this.SpeakerController.StreamStarted += this.OnSpeakerStreamStarted;
			this.SpeakerController.StreamStopped += this.OnSpeakerStreamStopped;
			this.MicController.NCSwitchEvent += this.OnMicToggleActivityChanged;
			this.SpeakerController.NCSwitchEvent += this.OnSpeakerToggleActivityChanged;
		}

		public override void DeAttachHandlers()
		{
			this.MicController.StreamStarted -= this.OnMicStreamStarted;
			this.SpeakerController.StreamStarted -= this.OnSpeakerStreamStarted;
			this.MicController.StreamStopped -= this.OnMicStreamStopped;
			this.SpeakerController.StreamStopped -= this.OnSpeakerStreamStopped;
			this.MicController.NCSwitchEvent -= this.OnMicToggleActivityChanged;
			this.SpeakerController.NCSwitchEvent -= this.OnSpeakerToggleActivityChanged;
			this.SendActiveUsage(true);
			this._appList.Clear();
		}

		public void Enable(MinutesBalance balance)
		{
			base.Logger.LogInfo("Enabling Free mode.");
			base.Disabled = false;
			this._currentNCUsage.used_seconds = 0U;
			this._minutesUsage = new MinutesUsage();
			this._minutesUsage.nc_out = new List<NCUsage>();
			this.AttachHandlers();
			this.MinuteModeProps = balance;
			this._timer.Interval = new TimeSpan(0, 0, 1).TotalMilliseconds;
			this._timer.Start();
		}

		public override void Disable()
		{
			base.Logger.LogInfo("Disabling Free mode.");
			base.Disabled = true;
			this.EnableToggles();
			this.DeAttachHandlers();
			this._timer.Stop();
		}

		private void OnMicStreamStarted(object s, IAppInfo e)
		{
			if (this.NCCountingEnabled)
			{
				this._appList.Add(e.ExeName);
				return;
			}
			if (!this.NCCountingEnabled && this.MicHasNCStream)
			{
				this.StartCounting();
			}
		}

		private void OnMicStreamStopped(object s, IAppInfo e)
		{
			if (this.NCCountingEnabled && !this.HasNCStream)
			{
				this.StopCounting();
			}
		}

		private void OnSpeakerStreamStarted(object s, IAppInfo e)
		{
			if (this.NCCountingEnabled)
			{
				this._appList.Add(e.ExeName);
				return;
			}
			if (!this.NCCountingEnabled && this.SpeakerHasNCStream)
			{
				this.StartCounting();
			}
		}

		private void OnSpeakerStreamStopped(object s, IAppInfo e)
		{
			if (this.NCCountingEnabled && !this.HasNCStream)
			{
				this.StopCounting();
			}
		}

		private void OnMicToggleActivityChanged(object s, bool e)
		{
			if (e)
			{
				if (!this.NCCountingEnabled && this.MicController.FilteredUsingApps.Count > 0)
				{
					this.StartCounting();
					return;
				}
			}
			else if (this.NCCountingEnabled && !this.SpeakerHasNCStream)
			{
				this.StopCounting();
			}
		}

		private void OnSpeakerToggleActivityChanged(object s, bool e)
		{
			if (e)
			{
				if (!this.NCCountingEnabled && this.SpeakerController.FilteredUsingApps.Count > 0)
				{
					this.StartCounting();
					return;
				}
			}
			else if (this.NCCountingEnabled && !this.MicHasNCStream)
			{
				this.StopCounting();
			}
		}

		private void DisableToggles()
		{
			this.TurnOffToggles();
			this.MicController.NCToggleState = false;
			this.SpeakerController.NCToggleState = false;
		}

		private void EnableToggles()
		{
			this.MicController.NCToggleState = true;
			this.SpeakerController.NCToggleState = true;
		}

		private void TurnOffToggles()
		{
			this.MicController.NCSwitchEvent -= this.OnMicToggleActivityChanged;
			this.SpeakerController.NCSwitchEvent -= this.OnSpeakerToggleActivityChanged;
			this.MicController.NCSwitch = false;
			this.SpeakerController.NCSwitch = false;
			this.MicController.NCSwitchEvent += this.OnMicToggleActivityChanged;
			this.SpeakerController.NCSwitchEvent += this.OnSpeakerToggleActivityChanged;
		}

		public void SendUsage()
		{
			this.setCurrentNCUsage();
			if (this._minutesUsage.nc_out.Count != 0)
			{
				MinutesModeViewModel.AcmService.MinutesBalanceAsync(this._minutesUsage);
				this._minutesUsage.nc_out.Clear();
			}
		}

		public void SendActiveUsage(bool async)
		{
			if (this.NCCountingEnabled)
			{
				this.NCCountingEnabled = false;
				this.setCurrentNCUsage();
				if (this._minutesUsage.nc_out.Count != 0)
				{
					if (async)
					{
						MinutesModeViewModel.AcmService.MinutesBalanceAsync(this._minutesUsage);
					}
					else
					{
						MinutesModeViewModel.AcmService.MinutesBalance(this._minutesUsage);
					}
					this._minutesUsage.nc_out.Clear();
				}
				this._appList.Clear();
			}
		}

		private void OnBalanceReceived(object s, MinutesBalance e)
		{
			this.MinuteModeProps = e;
			this._nextIndex = 0U;
		}

		private void resetNCBalance()
		{
			if (this.MinuteModeProps.nc_out.template == null)
			{
				this.MinuteModeProps.nc_out.balance = 0U;
				this.NCEnds = 0U;
				this.MinuteModeProps.nc_out.range_ends = 0U;
			}
			else
			{
				this.MinuteModeProps.nc_out.balance = this.MinuteModeProps.nc_out.template.balance;
				this.NCEnds = this.MinuteModeProps.nc_out.balance;
				this.MinuteModeProps.nc_out.range_ends = this.MinuteModeProps.nc_out.template.range;
				this.RangeEnds = this.MinuteModeProps.nc_out.range_ends;
			}
			base.RaisePropertyChanged("RangeEnds");
			base.RaisePropertyChanged("MinuteModeProps");
		}

		private void OnTimerElapsed(object s, TimerHelperElapsedEventArgs e)
		{
			uint num;
			if (this.NCCountingEnabled)
			{
				num = this._currentNCUsage.used_seconds + 1U;
				this._currentNCUsage.used_seconds = num;
				if (this._currentNCUsage.used_seconds == 10U)
				{
					MinutesModeViewModel.AcmService.MinutesBalanceAsync(null);
				}
				num = this.NCEnds - 1U;
				this.NCEnds = num;
				if (num <= 0U)
				{
					this.StopCounting();
					this.DisableToggles();
					ServiceContainer.Instance.GetService<IMessageNotifierService>().NotifyMessage(TranslationSourceViewModel.Instance["FreeMinutesExpired"] + DateTime.Now.AddSeconds(this.RangeEnds + 10U).ToString("dd MMM yyyy HH:mm"));
				}
			}
			num = this.RangeEnds - 1U;
			this.RangeEnds = num;
			if (num <= 0U)
			{
				this.EnableToggles();
				if (this.HasNCStream)
				{
					this.setCurrentNCUsage();
					this._currentNCUsage.relative_timestamp = this.MinuteModeProps.nc_out.template.range;
				}
				this._nextIndex += 1U;
				this.resetNCBalance();
				return;
			}
			if (this._nextIndex != 0U && (ulong)this.RangeEnds == (ulong)((long)new Random().Next((int)(this.MinuteModeProps.nc_out.template.range / 100U), (int)(this.MinuteModeProps.nc_out.template.range / 10U))))
			{
				MinutesModeViewModel.AcmService.MinutesBalanceAsync(null);
			}
		}

		private void setCurrentNCUsage()
		{
			if (this._currentNCUsage.used_seconds > 3U)
			{
				this._currentNCUsage.range_id = this.MinuteModeProps.nc_out.range_id;
				this._currentNCUsage.next_index = this._nextIndex;
				this._currentNCUsage.apps = string.Join(", ", this._appList);
				this._minutesUsage.nc_out.Add(this._currentNCUsage);
				base.Logger.LogInfo("NC usage: used: {0}; relative_ts: {1}; rangeID: {2}; next_index: {3}; apps: {4}; nc_remaining: {5}; range_remaining: {6}", new object[]
				{
					this._currentNCUsage.used_seconds,
					this._currentNCUsage.relative_timestamp,
					this._currentNCUsage.range_id,
					this._currentNCUsage.next_index,
					this._currentNCUsage.apps,
					this.NCEnds,
					this.RangeEnds
				});
			}
			else
			{
				this.NCEnds += this._currentNCUsage.used_seconds;
			}
			this._currentNCUsage.used_seconds = 0U;
		}

		private void StartCounting()
		{
			this.NCCountingEnabled = true;
			this._currentNCUsage.relative_timestamp = this.RangeEnds;
			if (this.MicHasNCStream)
			{
				this._appList.UnionWith(this.MicController.FilteredUsingApps);
			}
			if (this.SpeakerHasNCStream)
			{
				this._appList.UnionWith(this.SpeakerController.FilteredUsingApps);
			}
		}

		private void StopCounting()
		{
			this.NCCountingEnabled = false;
			this.SendUsage();
			this._appList.Clear();
		}

		private TimerHelper _timer;

		private uint _ncEnds;

		private NCUsage _currentNCUsage;

		private MinutesUsage _minutesUsage;

		private MinutesBalance _minutesModeProps;

		private uint _nextIndex;

		private HashSet<string> _appList;

		private object _micListSync = new object();

		private object _speakerListSync = new object();

		private bool _ncCountingEnabled;
	}
}
