using System;
using System.Text;
using P7;

namespace Krisp.AppHelper
{
	public static class LogWrapper
	{
		public static void Init(string logFolder, Traces.Level logLevel)
		{
			if (LogWrapper.p7Client == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				try
				{
					stringBuilder.AppendFormat("/P7.Sink={0} ", LogWrapper.LOG_SINK_TYPE);
					stringBuilder.AppendFormat("/P7.Dir=\"{0}\" ", logFolder);
					stringBuilder.AppendFormat("/P7.Files={0} ", LogWrapper.LOG_MAX_FILES_COUNT_IN_DEST);
					stringBuilder.AppendFormat("/P7.FSize={0} ", LogWrapper.LOG_MAX_FILES_CUMULATIVE_SIZE);
					stringBuilder.AppendFormat("/P7.Roll={0} ", LogWrapper.LOG_FILES_ROLLING_VALUE);
					stringBuilder.AppendFormat("/P7.Trc.Verb={0} ", (uint)logLevel);
					stringBuilder.AppendFormat("/P7.Format=\"{0}\" ", LogWrapper.LOG_ENTRY_FORMAT);
					LogWrapper.p7Client = new Client(stringBuilder.ToString());
					LogWrapper.p7Trace = new Traces(LogWrapper.p7Client, "App");
					LogWrapper.LogLevel = logLevel;
					return;
				}
				catch (Exception ex)
				{
					throw new Exception("Error on init logger with Args: " + stringBuilder.ToString(), ex);
				}
			}
			throw new Exception("Logger already initialized");
		}

		public static bool ShareClient(string shareName)
		{
			if (LogWrapper.p7Client == null)
			{
				throw new Exception("Logger not initialized");
			}
			return LogWrapper.p7Client.Share(shareName);
		}

		public static bool ShareTrace(string shareName)
		{
			if (LogWrapper.p7Client == null)
			{
				throw new Exception("Logger not initialized");
			}
			return LogWrapper.p7Trace.Share(shareName);
		}

		public static Logger GetLogger(string moduleName)
		{
			if (LogWrapper.p7Trace == null)
			{
				throw new Exception("Logger not initialized");
			}
			return new Logger(LogWrapper.p7Trace, moduleName, LogWrapper.LogLevel);
		}

		private static readonly string LOG_SINK_TYPE = "FileTxt";

		private static readonly uint LOG_MAX_FILES_COUNT_IN_DEST = 10U;

		private static readonly uint LOG_MAX_FILES_CUMULATIVE_SIZE = 10U;

		private static readonly string LOG_FILES_ROLLING_VALUE = "1mb";

		private static readonly string LOG_ENTRY_FORMAT = "%tf [%ti] %lv %cn.%mn - %ms";

		private static Client p7Client;

		private static Traces p7Trace;

		private static Traces.Level LogLevel;
	}
}
