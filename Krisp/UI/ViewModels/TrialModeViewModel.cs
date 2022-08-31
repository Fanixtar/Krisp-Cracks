using System;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class TrialModeViewModel : AppModeViewModel
	{
		public TrialMode_Props Props
		{
			get
			{
				return this._props;
			}
			set
			{
				if (this._props != value)
				{
					this._props = value;
					this.StartStopTimerIfNeeded();
					base.RaisePropertyChanged("TrialEnds");
				}
			}
		}

		public uint TrialEnds
		{
			get
			{
				return this.Props.trial_ends;
			}
		}

		public TrialModeViewModel()
		{
			base.Mode = "trial";
			this._trialEndTimer = new TimerHelper();
			this._trialEndTimer.AutoReset = false;
		}

		public void Enable(TrialMode_Props props)
		{
			base.Logger.LogInfo("Enabling FreePro mode.");
			base.Disabled = false;
			this._trialEndTimer.Elapsed += this.OnRangeTimerElapsed;
			this.Props = props;
			this.StartStopTimerIfNeeded();
		}

		public override void Disable()
		{
			base.Logger.LogInfo("Disabling FreePro mode.");
			base.Disabled = true;
			this._trialEndTimer.Stop();
			this._trialEndTimer.Elapsed -= this.OnRangeTimerElapsed;
		}

		private void OnRangeTimerElapsed(object s, TimerHelperElapsedEventArgs e)
		{
			base.Logger.LogInfo("FreePro timer elapsed... Fetching user profile.");
			ServiceContainer.Instance.GetService<IAccountManager>().FetchUserProfileInfoAsync();
		}

		private void StartStopTimerIfNeeded()
		{
			double totalMilliseconds = new TimeSpan(0, 0, (int)this.TrialEnds).TotalMilliseconds;
			if (this._trialEndTimer.Enabled)
			{
				if (totalMilliseconds > 21600000.0)
				{
					base.Logger.LogInfo("FreePro was extended to {0} seconds. Stopping Timer...", new object[] { this.TrialEnds });
					this._trialEndTimer.Stop();
					return;
				}
			}
			else if (totalMilliseconds < 21600000.0)
			{
				base.Logger.LogInfo("{0} seconds left for FreePro mode. Starting Timer...", new object[] { this.TrialEnds });
				this._trialEndTimer.Interval = totalMilliseconds;
				this._trialEndTimer.Start();
			}
		}

		private TimerHelper _trialEndTimer;

		private TrialMode_Props _props;
	}
}
