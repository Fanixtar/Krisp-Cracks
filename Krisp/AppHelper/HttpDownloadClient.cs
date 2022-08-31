using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Krisp.AppHelper
{
	internal class HttpDownloadClient : IDisposable
	{
		public event HttpDownloadClient.ProgressChangedHandler ProgressChanged;

		public HttpDownloadClient(string downloadUrl, string destinationFilePath, CancellationToken? cancellationToken = null)
		{
			this._downloadUrl = downloadUrl;
			this._destinationFilePath = destinationFilePath;
			this._cancellationToken = cancellationToken;
		}

		public async Task<HttpAsyncDownloadResult> StartDownload()
		{
			HttpClientHandler httpClientHandler = new HttpClientHandler
			{
				Proxy = WebRequest.DefaultWebProxy
			};
			this._httpClient = new HttpClient(httpClientHandler, true)
			{
				Timeout = TimeSpan.FromDays(1.0)
			};
			try
			{
				using (HttpResponseMessage response = await this._httpClient.GetAsync(this._downloadUrl, HttpCompletionOption.ResponseHeadersRead, this._cancellationToken.Value))
				{
					await this.ProcessContentStream(response);
				}
				HttpResponseMessage response = null;
			}
			catch (Exception ex)
			{
				return new HttpAsyncDownloadResult(ex);
			}
			return new HttpAsyncDownloadResult(0);
		}

		private async Task<HttpAsyncDownloadResult> ProcessContentStream(HttpResponseMessage response)
		{
			response.EnsureSuccessStatusCode();
			long? totalDownloadSize = response.Content.Headers.ContentLength;
			long totalBytesRead = 0L;
			long readCount = 0L;
			byte[] buffer = new byte[8192];
			bool isMoreToRead = true;
			using (Stream contentStream = await response.Content.ReadAsStreamAsync())
			{
				using (FileStream fileStream = new FileStream(this._destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
				{
					do
					{
						int bytesRead;
						if (this._cancellationToken != null)
						{
							bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, this._cancellationToken.Value);
						}
						else
						{
							bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
						}
						if (bytesRead == 0)
						{
							isMoreToRead = false;
						}
						else
						{
							await fileStream.WriteAsync(buffer, 0, bytesRead);
							totalBytesRead += (long)bytesRead;
							readCount += 1L;
							if (readCount % 10L == 0L)
							{
								this.TriggerProgressChanged(totalDownloadSize, totalBytesRead);
							}
						}
					}
					while (isMoreToRead && (this._cancellationToken == null || !this._cancellationToken.Value.IsCancellationRequested));
				}
				FileStream fileStream = null;
			}
			Stream contentStream = null;
			if (!this._cancellationToken.Value.IsCancellationRequested)
			{
				this.TriggerProgressChanged(totalDownloadSize, totalBytesRead);
			}
			return new HttpAsyncDownloadResult(0);
		}

		private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
		{
			if (this.ProgressChanged == null)
			{
				return;
			}
			double? num = null;
			if (totalDownloadSize != null)
			{
				num = new double?(Math.Round((double)totalBytesRead / (double)totalDownloadSize.Value * 100.0, 2));
			}
			this.ProgressChanged(totalDownloadSize, totalBytesRead, num);
		}

		public void Dispose()
		{
			HttpClient httpClient = this._httpClient;
			if (httpClient == null)
			{
				return;
			}
			httpClient.Dispose();
		}

		private readonly string _downloadUrl;

		private readonly string _destinationFilePath;

		private readonly CancellationToken? _cancellationToken;

		private HttpClient _httpClient;

		public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);
	}
}
