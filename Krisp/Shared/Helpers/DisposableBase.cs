using System;

namespace Shared.Helpers
{
	public class DisposableBase : IDisposable
	{
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			this._disposed = true;
		}

		~DisposableBase()
		{
			this.Dispose(false);
		}

		private bool _disposed;
	}
}
