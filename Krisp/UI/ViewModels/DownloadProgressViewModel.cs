using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Krisp.AppHelper;

namespace Krisp.UI.ViewModels
{
	public class DownloadProgressViewModel : BindableBase
	{
		public DownloadProgressViewModel(string surl, string filepath)
		{
			this.Result = new HttpAsyncDownloadResult(1);
			this._dispatcher = Application.Current.Dispatcher;
			this.CurrentProgress = 0.0;
			this._cancellationTokenSource = new CancellationTokenSource();
			this._client = new HttpDownloadClient(surl, filepath, new CancellationToken?(this._cancellationTokenSource.Token));
			this._client.ProgressChanged += delegate(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
			{
				this.CurrentProgress = progressPercentage.Value;
			};
		}

		public void StartDownload()
		{
			Task.Run(delegate()
			{
				this.Result = this._client.StartDownload().Result;
				this._client.Dispose();
				Dispatcher dispatcher = this._dispatcher;
				if (dispatcher == null)
				{
					return;
				}
				dispatcher.Invoke(delegate()
				{
					EventHandler<bool> completed = this.Completed;
					if (completed == null)
					{
						return;
					}
					completed(this, true);
				});
			});
		}

		public double CurrentProgress
		{
			get
			{
				return this._currentProgress;
			}
			private set
			{
				if (this._currentProgress != value)
				{
					this._currentProgress = value;
					Dispatcher dispatcher = this._dispatcher;
					if (dispatcher == null)
					{
						return;
					}
					dispatcher.Invoke(delegate()
					{
						base.RaisePropertyChanged("CurrentProgress");
					});
				}
			}
		}

		public EventHandler<bool> Completed;

		private double _currentProgress;

		private HttpDownloadClient _client;

		public CancellationTokenSource _cancellationTokenSource;

		public HttpAsyncDownloadResult Result;

		private Dispatcher _dispatcher;
	}
}
