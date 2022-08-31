using System;
using System.Windows.Input;
using Krisp.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class ControlStatusViewModel : BindableBase
	{
		public bool StatusExists
		{
			get
			{
				return this.LastStatus != null;
			}
		}

		public string StatusMessageText
		{
			get
			{
				StatusMessage lastStatus = this.LastStatus;
				if (lastStatus == null)
				{
					return null;
				}
				return lastStatus.Message;
			}
		}

		public Action Handler { get; private set; }

		public StatusMessage LastStatus { get; private set; }

		public ICommand StatusHandlerCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._statusHandlerCommand) == null)
				{
					relayCommand = (this._statusHandlerCommand = new RelayCommand(delegate(object param)
					{
						this.Handler();
					}, (object param) => this.Handler != null));
				}
				return relayCommand;
			}
		}

		public void ApplyStatus(StatusMessage statusMsg, Action handler)
		{
			this.Handler = handler;
			this.LastStatus = statusMsg;
			base.RaisePropertyChanged("StatusMessageText");
			base.RaisePropertyChanged("StatusExists");
			base.RaisePropertyChanged("Handler");
		}

		private RelayCommand _statusHandlerCommand;
	}
}
