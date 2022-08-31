using System;
using System.Threading;
using System.Timers;

namespace Krisp.AppHelper
{
	public class TimerHelper : System.Timers.Timer
	{
		public new event EventHandler<TimerHelperElapsedEventArgs> Elapsed;

		public TimerHelper()
		{
			base.Elapsed += this.TimerHelper_Elapsed;
		}

		public TimerHelper(double interval)
			: base(interval)
		{
			base.Elapsed += this.TimerHelper_Elapsed;
		}

		private void TimerHelper_Elapsed(object sender, ElapsedEventArgs e)
		{
			EventHandler<TimerHelperElapsedEventArgs> elapsed = this.Elapsed;
			if (elapsed != null)
			{
				try
				{
					elapsed(sender, new TimerHelperElapsedEventArgs());
				}
				catch (Exception ex)
				{
					Exception ex3;
					Exception ex2 = ex3;
					Exception ex = ex2;
					LogWrapper.GetLogger("TimerHelper").LogInfo("Timer handler catch an exception: {0}", new object[] { ex.Message });
					ThreadPool.QueueUserWorkItem(delegate(object _)
					{
						throw ex;
					});
				}
			}
		}
	}
}
