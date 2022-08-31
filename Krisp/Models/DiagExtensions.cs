using System;
using System.Text;

namespace Krisp.Models
{
	public static class DiagExtensions
	{
		public static StringBuilder beginLineSeparator(this IDiagnosticsBase dInstance, StringBuilder sb, int indent = 0)
		{
			sb.AppendFormat("{0}{1}\r\n", "".PadLeft(indent), string.Concat(new string[]
			{
				DiagExtensions.DIAG_LINE_SEPARATOR,
				" +++++   ",
				dInstance.InstanceName,
				"   +++++ ",
				DiagExtensions.DIAG_LINE_SEPARATOR
			}));
			return sb;
		}

		public static StringBuilder endLineSeparator(this IDiagnosticsBase dInstance, StringBuilder sb, int indent = 0)
		{
			sb.AppendFormat("{0}{1}\r\n", "".PadLeft(indent), string.Concat(new string[]
			{
				DiagExtensions.DIAG_LINE_SEPARATOR,
				" -----   ",
				dInstance.InstanceName,
				"   ----- ",
				DiagExtensions.DIAG_LINE_SEPARATOR
			}));
			return sb;
		}

		public static readonly string DIAG_LINE_SEPARATOR = "=======";
	}
}
