using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.UI.ViewModels;
using Krisp.UI.Views;
using Krisp.UI.Views.Windows;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.TestKrisp.ViewModels
{
	public class TestKrispViewModel : BindableBase
	{
		public DeviceViewModel CurrentDevice
		{
			get
			{
				return this._currentDevice;
			}
			private set
			{
				if (this._currentDevice != value)
				{
					this._currentDevice = value;
					base.RaisePropertyChanged("CurrentDevice");
				}
			}
		}

		public TestKrispViewModel()
		{
			this._logger.LogDebug("Initializing Test Krisp");
			this.RemoveRecordings(true);
			Directory.CreateDirectory(TestKrispViewModel._path);
			DataModelFactory.SPInstance.SPInitialize();
			RecorderViewModel recorderViewModel = new RecorderViewModel(this._sourceSoundPath, this._beforeNCSoundPath, this._afterNCSoundPath);
			recorderViewModel.RecordCompleted += this.Recorded;
			recorderViewModel.Error += this.Error;
			this.CurrentDevice = recorderViewModel;
		}

		private void Error(object sender, string e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				DeviceViewModel currentDevice = this.CurrentDevice;
				if (currentDevice != null)
				{
					currentDevice.Destroy();
				}
				this.CurrentDevice = new ErrorViewModel();
			});
		}

		~TestKrispViewModel()
		{
		}

		public ICommand RecordAgainCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._recordAgainCommand) == null)
				{
					relayCommand = (this._recordAgainCommand = new RelayCommand(delegate(object param)
					{
						this._logger.LogInfo("Record again.");
						this.Reset();
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispRecAgainEvent());
					}));
				}
				return relayCommand;
			}
		}

		public ICommand ReportAnAudioCommand
		{
			get
			{
				return new RelayCommand(delegate(object param)
				{
					if (AccountManager.Instance.IsLoggedIn)
					{
						ReportBugWindow.Show(new ReportViewModel(ReportSource.testnc, this.GetListOfCopiedFiles(), null));
						return;
					}
					ReportProblemManually.Show(new ReportViewModel(ReportSource.testnc, this.GetListOfCopiedFiles(), null));
				});
			}
		}

		public ICommand TryAgainCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._tryAgainCommand) == null)
				{
					relayCommand = (this._tryAgainCommand = new RelayCommand(delegate(object param)
					{
						this.Reset();
						this._logger.LogInfo("Try again.");
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.TestKrispTryAgainEvent());
					}));
				}
				return relayCommand;
			}
		}

		private List<string> GetListOfCopiedFiles()
		{
			List<string> list = new List<string>();
			string krispAppLogFolder = EnvHelper.KrispAppLogFolder;
			if (Directory.Exists(krispAppLogFolder))
			{
				if (File.Exists(this._sourceSoundPath))
				{
					string text = Path.Combine(krispAppLogFolder, Path.GetFileName(this._sourceSoundPath));
					try
					{
						File.Copy(this._sourceSoundPath, text, true);
						list.Add(text);
					}
					catch
					{
					}
				}
				if (File.Exists(this._beforeNCSoundPath))
				{
					string text2 = Path.Combine(krispAppLogFolder, Path.GetFileName(this._beforeNCSoundPath));
					try
					{
						File.Copy(this._beforeNCSoundPath, text2, true);
						list.Add(text2);
					}
					catch
					{
					}
				}
				if (File.Exists(this._afterNCSoundPath))
				{
					string text3 = Path.Combine(krispAppLogFolder, Path.GetFileName(this._afterNCSoundPath));
					try
					{
						File.Copy(this._afterNCSoundPath, text3, true);
						list.Add(text3);
					}
					catch
					{
					}
				}
			}
			return list;
		}

		private void Recorded(object sender, EventArgs e)
		{
			if (sender is RecorderViewModel)
			{
				(sender as RecorderViewModel).RecordCompleted -= this.Recorded;
				this.CurrentDevice.Destroy();
				PlayerViewModel playerViewModel = new PlayerViewModel();
				playerViewModel.Error += this.Error;
				playerViewModel.Init(this._beforeNCSoundPath, this._afterNCSoundPath);
				this.CurrentDevice = playerViewModel;
			}
		}

		private void RemoveRecordings(bool contentsOnly = false)
		{
			try
			{
				if (Directory.Exists(TestKrispViewModel._path))
				{
					if (contentsOnly)
					{
						FileInfo[] files = new DirectoryInfo(TestKrispViewModel._path).GetFiles();
						for (int i = 0; i < files.Length; i++)
						{
							files[i].Delete();
						}
					}
					else
					{
						Directory.Delete(TestKrispViewModel._path, true);
					}
				}
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Recording deletion failed. Exception: {0}", new object[] { ex.Message });
			}
		}

		private void Reset()
		{
			this._logger.LogDebug("Resetting currentDevice to recorder");
			this.CurrentDevice.Destroy();
			this.RemoveRecordings(true);
			RecorderViewModel recorderViewModel = new RecorderViewModel(this._sourceSoundPath, this._beforeNCSoundPath, this._afterNCSoundPath);
			recorderViewModel.RecordCompleted += this.Recorded;
			recorderViewModel.Error += this.Error;
			this.CurrentDevice = recorderViewModel;
		}

		public void Cleanup()
		{
			this._logger.LogDebug("Cleanup of TestKrisp");
			this.CurrentDevice.Destroy();
			DataModelFactory.SPInstance.SPRelease();
			this.RemoveRecordings(false);
		}

		private DeviceViewModel _currentDevice;

		private static readonly string _path = EnvHelper.TestKrispRecordingsFolder;

		private readonly string _sourceSoundPath = Path.Combine(TestKrispViewModel._path, DateTime.Now.ToString("yyyyddMHHmmss") + "_source.wav");

		private readonly string _beforeNCSoundPath = Path.Combine(TestKrispViewModel._path, DateTime.Now.ToString("yyyyddMHHmmss") + "_before_nc.wav");

		private readonly string _afterNCSoundPath = Path.Combine(TestKrispViewModel._path, DateTime.Now.ToString("yyyyddMHHmmss") + "_after_nc.wav");

		private RelayCommand _recordAgainCommand;

		private RelayCommand _tryAgainCommand;

		private readonly Logger _logger = LogWrapper.GetLogger("TestNoiseCancellation");

		private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
	}
}
