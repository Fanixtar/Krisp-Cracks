using System;
using System.Text;

namespace Krisp.Models
{
	public interface IDiagnosticsBase
	{
		void DumpDiagnosticInfo(StringBuilder sb, int indent);

		string InstanceName { get; }
	}
}
