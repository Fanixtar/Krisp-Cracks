using System;
using System.Windows.Input;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Properties;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Interops;

namespace Krisp.UI.ViewModels
{
	public class PreferencesViewModel : BindableBase
	{
		public PreferencesViewModel()
		{
			try
			{
				this._echoCancellationSwitch = Settings.Default.EchoCancellationState;
				this._lockUpMicVolume = Settings.Default.LockUpVolumeForMic > 0;
			}
			catch
			{
			}
		}

		public bool EchoCancellationSwitch
		{
			get
			{
				return this._echoCancellationSwitch;
			}
			set
			{
				if (this._echoCancellationSwitch != value)
				{
					this._echoCancellationSwitch = value;
					base.RaisePropertyChanged("EchoCancellationSwitch");
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.PreferencesEchoCancellation(value));
					StreamProcessor.Instance.SetFeatureState(EnStreamDirection.Microphone, SPFeature.Feature_Echo, value);
					try
					{
						Settings.Default.EchoCancellationState = value;
						Settings.Default.Save();
					}
					catch (Exception ex)
					{
						this._logger.LogError("Error on storing EchoCancellation state. {0}", new object[] { ex.Message });
					}
				}
			}
		}

		public bool LockUpMicVolume
		{
			get
			{
				return this._lockUpMicVolume;
			}
			set
			{
				if (this._lockUpMicVolume != value)
				{
					this._lockUpMicVolume = value;
					base.RaisePropertyChanged("LockUpMicVolume");
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.PreferencesLockMicLevel(value));
					try
					{
						Settings.Default.LockUpVolumeForMic = (value ? 98 : 0);
						Settings.Default.Save();
					}
					catch (Exception ex)
					{
						this._logger.LogError("Error on storing LockUpMicVolume state. {0}", new object[] { ex.Message });
					}
				}
			}
		}

		public ICommand RestoreDefaultsCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._appSelectedCommand) == null)
				{
					relayCommand = (this._appSelectedCommand = new RelayCommand(delegate(object param)
					{
						this._logger.LogInfo("Advanced preferences restored to defaults");
						this.EchoCancellationSwitch = false;
						this.LockUpMicVolume = true;
					}));
				}
				return relayCommand;
			}
		}

		public static PreferencesViewModel Instance
		{
			get
			{
				if (PreferencesViewModel._instance == null)
				{
					PreferencesViewModel._instance = new PreferencesViewModel();
				}
				return PreferencesViewModel._instance;
			}
		}

		private bool _echoCancellationSwitch;

		private bool _lockUpMicVolume;

		private RelayCommand _appSelectedCommand;

		private Logger _logger = LogWrapper.GetLogger("PreferencesViewModel");

		private static PreferencesViewModel _instance;
	}
}
