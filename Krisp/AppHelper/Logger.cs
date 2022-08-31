using System;
using P7;

namespace Krisp.AppHelper
{
	public class Logger
	{
		internal Logger(Traces p7Tr, string mName, Traces.Level logLevel)
		{
			this.p7Trace = p7Tr;
			if (!string.IsNullOrEmpty(mName))
			{
				this.p7Module = this.p7Trace.Register_Module(mName);
				this.LogLevel = logLevel;
			}
		}

		public void LogDebug(object message)
		{
			if (Traces.Level.DEBUG < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Debug(this.p7Module, message.ToString());
		}

		public void LogDebug(string message)
		{
			if (Traces.Level.DEBUG < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Debug(this.p7Module, message);
		}

		public void LogDebug(string format, params object[] args)
		{
			if (Traces.Level.DEBUG < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Debug(this.p7Module, string.Format(format, args));
		}

		public void LogInfo(object message)
		{
			if (Traces.Level.INFO < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Info(this.p7Module, message.ToString());
		}

		public void LogInfo(string message)
		{
			if (Traces.Level.INFO < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Info(this.p7Module, message);
		}

		public void LogInfo(string format, params object[] args)
		{
			if (Traces.Level.INFO < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Info(this.p7Module, string.Format(format, args));
		}

		public void LogWarning(object message)
		{
			if (Traces.Level.WARNING < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Warning(this.p7Module, message.ToString());
		}

		public void LogWarning(string message)
		{
			if (Traces.Level.WARNING < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Warning(this.p7Module, message);
		}

		public void LogWarning(string format, params object[] args)
		{
			if (Traces.Level.WARNING < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Warning(this.p7Module, string.Format(format, args));
		}

		public void LogError(object message)
		{
			if (Traces.Level.ERROR < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Error(this.p7Module, message.ToString());
		}

		public void LogError(string message)
		{
			if (Traces.Level.ERROR < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Error(this.p7Module, message);
		}

		public void LogError(Exception ex, string message)
		{
			if (Traces.Level.ERROR < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Error(this.p7Module, message + " : " + ex.ToString());
		}

		public void LogError(string format, params object[] args)
		{
			if (Traces.Level.ERROR < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Error(this.p7Module, string.Format(format, args));
		}

		public void LogError(Exception ex, string format, params object[] args)
		{
			if (Traces.Level.ERROR < this.LogLevel)
			{
				return;
			}
			this.p7Trace.Error(this.p7Module, string.Format(format, args) + " : " + ex.ToString());
		}

		private Traces p7Trace;

		private IntPtr p7Module;

		private Traces.Level LogLevel;
	}
}
