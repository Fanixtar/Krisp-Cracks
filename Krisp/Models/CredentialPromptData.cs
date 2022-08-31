using System;
using System.Net;

namespace Krisp.Models
{
	public class CredentialPromptData
	{
		public CredentialPromptData(NetworkCredential cred)
		{
			this.Credentials = cred;
		}

		public NetworkCredential Credentials { get; set; }

		public string Title { get; set; }

		public string Message { get; set; }

		public bool SaveChecked { get; set; }

		public bool GenericCredentials { get; set; }

		public bool ShowSaveCheckBox { get; set; }

		public int ErrorCode { get; set; }
	}
}
