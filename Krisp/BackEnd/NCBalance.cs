using System;
using System.Runtime.CompilerServices;

namespace Krisp.BackEnd
{
	public class NCBalance
	{
		public uint range_id { get; set; }

		public uint range_ends { get; set; }

		public uint balance
		{
			get
			{
				return 6048000U;
			}
			[CompilerGenerated]
			set
			{
				this.<balance>k__BackingField = value;
			}
		}

		public NCBalance.NCTemplate template { get; set; }

		public class NCTemplate
		{
			public uint balance { get; set; }

			public uint range { get; set; }
		}
	}
}
