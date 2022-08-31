using System;

namespace Krisp.BackEnd
{
	public class MinutesBalance
	{
		public NCBalance nc_out { get; set; }

		public MinutesBalance()
		{
		}

		public MinutesBalance(NCBalance ncBalance)
		{
			this.nc_out = ncBalance;
		}
	}
}
