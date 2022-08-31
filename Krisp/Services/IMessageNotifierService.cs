using System;
using Krisp.Models;

namespace Krisp.Services
{
	internal interface IMessageNotifierService
	{
		void NotifyMessage(string msg);

		void NotifyMessage(INotification notification);
	}
}
