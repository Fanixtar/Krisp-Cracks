using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.Properties;
using MVVMFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Shared.Analytics;
using Shared.Interops;
using Shared.Interops.Extensions;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.TestKrisp.ViewModels
{
	public class PlayerViewModel : DeviceViewModel
	{
		public bool MuteNoise
		{
			get
			{
				return this._muteNoise;
			}
			set
			{
				if (value != this._muteNoise)
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispToggleEvent(value));
					this.Logger.LogInfo("NC switch turned {0}", new object[] { value ? "ON" : "OFF" });
					this._muteNoise = value;
					this._beforeNCWave.Volume = (this.MuteNoise ? 0f : 1f);
					this._afterNCWave.Volume = (this.MuteNoise ? 1f : 0f);
					base.RaisePropertyChanged("MuteNoise");
				}
			}
		}

		public int PlayerPosition
		{
			get
			{
				return this._playerPosition;
			}
			set
			{
				if (value != this._playerPosition)
				{
					this._playerPosition = value;
					base.RaisePropertyChanged("PlayerPosition");
				}
			}
		}

		public bool ReportProblemEnabled
		{
			get
			{
				return this._reportProblemEnabled;
			}
			set
			{
				if (this._reportProblemEnabled != value)
				{
					this._reportProblemEnabled = value;
					base.RaisePropertyChanged("ReportProblemEnabled");
				}
			}
		}

		private Logger Logger { get; } = LogWrapper.GetLogger("TestNoiseCancellation");

		public event EventHandler<string> Error;

		public PlayerViewModel()
		{
			this._timer = new DispatcherTimer();
			this._timer.Tick += this.TimerTick;
			this._timer.Interval = TimeSpan.FromSeconds(1.0);
			this.ReportProblemEnabled = AccountManager.ShowReportProblem();
		}

		public void Init(string beforeNCPath, string afterNCPath)
		{
			bool flag = false;
			this._beforeNCWave = new AudioFileReader(beforeNCPath);
			this._afterNCWave = new AudioFileReader(afterNCPath);
			this._beforeNCWave.Volume = (this.MuteNoise ? 0f : 1f);
			this._afterNCWave.Volume = (this.MuteNoise ? 1f : 0f);
			this._mixingSampleProvider = new MixingSampleProvider(new List<ISampleProvider> { this._beforeNCWave, this._afterNCWave });
			try
			{
				this._Player = new WaveOutEvent
				{
					DeviceNumber = this.DetermineDeviceNumber(),
					Volume = 1f
				};
				this._Player.Init(this._mixingSampleProvider, false);
			}
			catch (Exception ex)
			{
				this.Logger.LogError("TestKrisp playback device initialization failed. ({0})", new object[] { ex.Message });
				WaveOutEvent player = this._Player;
				if (player != null)
				{
					player.Dispose();
				}
				if (WaveOut.DeviceCount == 0)
				{
					EventHandler<string> error = this.Error;
					if (error != null)
					{
						error(this, "No playback device found.");
					}
					return;
				}
				flag = true;
			}
			if (flag)
			{
				try
				{
					this._Player = new WaveOutEvent
					{
						DeviceNumber = -1,
						Volume = 1f
					};
					this._Player.Init(this._mixingSampleProvider, false);
				}
				catch (Exception ex2)
				{
					this.Logger.LogError("TestKrisp playback device initialization failed. ({0})", new object[] { ex2.Message });
					WaveOutEvent player2 = this._Player;
					if (player2 != null)
					{
						player2.Dispose();
					}
					EventHandler<string> error2 = this.Error;
					if (error2 != null)
					{
						error2(this, "No playback device found.");
					}
					return;
				}
			}
			this._Player.PlaybackStopped += this.PlaybackStopped;
			this.MuteNoise = false;
			this.Play();
		}

		private void TimerTick(object sender, EventArgs e)
		{
			if (WaveOut.DeviceCount != 0)
			{
				int num = this.PlayerPosition + 1;
				this.PlayerPosition = num;
				return;
			}
			EventHandler<string> error = this.Error;
			if (error == null)
			{
				return;
			}
			error(this, "Playback device was lost.");
		}

		private void Play()
		{
			this.Logger.LogInfo("Playing the recording.");
			this._timer.Start();
			this._Player.Play();
		}

		public override void Destroy()
		{
			object lockObj = PlayerViewModel._lockObj;
			lock (lockObj)
			{
				DispatcherTimer timer = this._timer;
				if (timer != null)
				{
					timer.Stop();
				}
				if (this._Player != null)
				{
					this._Player.PlaybackStopped -= this.PlaybackStopped;
					this._Player.Stop();
					this._Player.Dispose();
				}
				AudioFileReader beforeNCWave = this._beforeNCWave;
				if (beforeNCWave != null)
				{
					beforeNCWave.Dispose();
				}
				AudioFileReader afterNCWave = this._afterNCWave;
				if (afterNCWave != null)
				{
					afterNCWave.Dispose();
				}
				this._destroyed = true;
			}
		}

		private void PlaybackStopped(object sender, StoppedEventArgs e)
		{
			try
			{
				object lockObj = PlayerViewModel._lockObj;
				lock (lockObj)
				{
					if (e.Exception == null)
					{
						if (!this._destroyed)
						{
							this._timer.Stop();
							this.PlayerPosition = 0;
							this._beforeNCWave.Position = 0L;
							this._mixingSampleProvider.AddMixerInput(this._beforeNCWave);
							this._afterNCWave.Position = 0L;
							this._mixingSampleProvider.AddMixerInput(this._afterNCWave);
							this.Play();
						}
					}
					else
					{
						EventHandler<string> error = this.Error;
						if (error != null)
						{
							error(this, "Player stopped: unexpected error.");
						}
						this.Logger.LogWarning("PlaybackStopped event received with exception: {0}", new object[] { e.Exception });
					}
				}
			}
			catch (Exception ex)
			{
				this.Logger.LogError("PlaybackStopped event failed: {0}", new object[] { ex.Message });
			}
		}

		private int DetermineDeviceNumber()
		{
			if (!ServiceContainer.Instance.GetService<IAccountManager>().IsLoggedIn)
			{
				return -1;
			}
			string lastSelectedSpeaker = Settings.Default.LastSelectedSpeaker;
			if (lastSelectedSpeaker == "Default_Device")
			{
				return -1;
			}
			try
			{
				string text = ((IMMDeviceEnumerator)new MMDeviceEnumerator()).GetDevice(lastSelectedSpeaker).OpenPropertyStore(STGM.DIRECT).GetValue(PropertyKeys.PKEY_Device_FriendlyName);
				if (text.Length > 31)
				{
					text = text.Substring(0, 31);
				}
				for (int i = -1; i < WaveOut.DeviceCount; i++)
				{
					if (WaveOut.GetCapabilities(i).ProductName == text)
					{
						return i;
					}
				}
			}
			catch (Exception ex) when (ex.Is(-2147023728))
			{
				this.Logger.LogWarning("Couldn't determine device. Exception: {0}", new object[] { ex.Message });
				return -1;
			}
			return -1;
		}

		private bool _reportProblemEnabled = true;

		private bool _muteNoise;

		private readonly DispatcherTimer _timer;

		private int _playerPosition;

		private static readonly object _lockObj = new object();

		private MixingSampleProvider _mixingSampleProvider;

		private AudioFileReader _beforeNCWave;

		private AudioFileReader _afterNCWave;

		private WaveOutEvent _Player;

		private bool _destroyed;
	}
}
