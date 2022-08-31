using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Krisp.AppHelper;
using Krisp.UI.ViewModels;
using Shared.Interops;

namespace Krisp.UI.Views.Windows
{
	public partial class DownloadProgressWindow : Window
	{
		public HttpAsyncDownloadResult Result
		{
			get
			{
				return this._dtcntx.Result;
			}
		}

		public DownloadProgressWindow(string strurl, string filepath)
		{
			this.InitializeComponent();
			this._dtcntx = new DownloadProgressViewModel(strurl, filepath);
			DownloadProgressViewModel dtcntx = this._dtcntx;
			dtcntx.Completed = (EventHandler<bool>)Delegate.Combine(dtcntx.Completed, new EventHandler<bool>(delegate(object s, bool e)
			{
				base.Close();
			}));
			base.DataContext = this._dtcntx;
			base.Closing += delegate(object s, CancelEventArgs e)
			{
				HttpAsyncDownloadResult result = this.Result;
				HRESULT? hresult = ((result != null) ? new HRESULT?(result.HResult) : null);
				int? num = ((hresult != null) ? new int?(hresult.GetValueOrDefault()) : null);
				int num2 = 0;
				if (!((num.GetValueOrDefault() == num2) & (num != null)))
				{
					this._dtcntx._cancellationTokenSource.Cancel();
				}
			};
			this._dtcntx.StartDownload();
		}

		private DownloadProgressViewModel _dtcntx;
	}
}
