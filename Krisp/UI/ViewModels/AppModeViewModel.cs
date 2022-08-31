using System;
using Krisp.AppHelper;

namespace Krisp.UI.ViewModels
{
	public class AppModeViewModel : BindableBase
	{
		public virtual void Disable()
		{
		}

		public virtual void DeAttachHandlers()
		{
		}

		public virtual void AttachHandlers()
		{
		}

		public string Mode
		{
			get
			{
				return this._mode;
			}
			set
			{
				if (this._mode != value)
				{
					this._mode = value;
					base.RaisePropertyChanged("Mode");
				}
			}
		}

		public bool Disabled { get; set; } = true;

		public Logger Logger
		{
			get
			{
				return LogWrapper.GetLogger("AppModeViewModel");
			}
		}

		private string _mode;
	}
}
