using System;

namespace Krisp.Models
{
	public interface ICredentialsPrompt
	{
		PromptResult Prompt(CredentialPromptData data);
	}
}
