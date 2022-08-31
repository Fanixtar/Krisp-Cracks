using System;
using Krisp.Models;

namespace Krisp.SysTray.Notifications
{
	public class DefaultNotification : INotification
	{
		public DefaultNotification(string text)
		{
			this.Title = "Krisp";
			this.Text = text;
			this.Handler = null;
		}

		public string Title { get; private set; }

		public string Text { get; private set; }

		public Action Handler { get; private set; }
	}
}
