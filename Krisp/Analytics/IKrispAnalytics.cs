using System;
using Shared.Analytics;

namespace Krisp.Analytics
{
	public interface IKrispAnalytics
	{
		void Report(AnalyticEvent aEvent);
	}
}
