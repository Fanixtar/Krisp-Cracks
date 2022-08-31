using System;

namespace Shared.Interops
{
	public struct PROPERTYKEY
	{
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			PROPERTYKEY propertykey = (PROPERTYKEY)obj;
			return propertykey.fmtid == this.fmtid && propertykey.pid == this.pid;
		}

		public override int GetHashCode()
		{
			return this.fmtid.GetHashCode() + this.pid.GetHashCode();
		}

		public Guid fmtid;

		public UIntPtr pid;
	}
}
