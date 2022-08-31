using System;

namespace MVVMFoundation
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class MediatorMessageSinkAttribute : Attribute
	{
		public object MessageKey { get; private set; }

		public MediatorMessageSinkAttribute()
		{
			this.MessageKey = null;
		}

		public MediatorMessageSinkAttribute(string messageKey)
		{
			this.MessageKey = messageKey;
		}

		public MediatorMessageSinkAttribute(Enum messageKey)
		{
			this.MessageKey = Enum.GetName(messageKey.GetType(), messageKey);
		}
	}
}
