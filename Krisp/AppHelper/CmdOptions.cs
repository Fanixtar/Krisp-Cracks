using System;
using CommandLine;

namespace Krisp.AppHelper
{
	internal class CmdOptions
	{
		[Option('s', "silent", Required = false, HelpText = "Provide app mode: silently or not.")]
		public bool Silent { get; set; }

		[Option('m', Required = false, HelpText = "Run after install.")]
		public bool FromMSI { get; set; }

		[Option("test-mode", Required = false, HelpText = "Run app on test mode.")]
		public bool TestMode { get; set; }
	}
}
