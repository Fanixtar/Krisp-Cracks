using System;
using Krisp.Models;
using Krisp.Services;
using Krisp.SysTray.Notifications;

namespace Krisp.App.SysTray
{
	public class MessageNotifierService : IMessageNotifierService
	{
		public MessageNotifierService(MessageNotifierService.MessageNotifier notifyer)
		{
			this._notifyMessage = notifyer;
		}

		public void NotifyMessage(string text)
		{
			MessageNotifierService.MessageNotifier notifyMessage = this._notifyMessage;
			if (notifyMessage == null)
			{
				return;
			}
			notifyMessage(new DefaultNotification(text));
		}

		public void NotifyMessage(INotification notification)
		{
			MessageNotifierService.MessageNotifier notifyMessage = this._notifyMessage;
			if (notifyMessage == null)
			{
				return;
			}
			notifyMessage(notification);
		}

		private readonly MessageNotifierService.MessageNotifier _notifyMessage;

		public delegate void MessageNotifier(INotification notification);
	}
}
