using System;

namespace Krisp.Models
{
	public interface INotification
	{
		string Title { get; }

		string Text { get; }

		Action Handler { get; }
	}
}
