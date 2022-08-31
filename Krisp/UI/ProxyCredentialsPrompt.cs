using System;
using System.Windows.Threading;
using Krisp.Models;
using Krisp.UI.ViewModels;
using Krisp.UI.Views.Windows;

namespace Krisp.UI
{
	internal class ProxyCredentialsPrompt : ICredentialsPrompt
	{
		public ProxyCredentialsPrompt()
		{
			this._dispatcher = Dispatcher.CurrentDispatcher;
		}

		PromptResult ICredentialsPrompt.Prompt(CredentialPromptData data)
		{
			PromptResult res = PromptResult.None;
			this._dispatcher.Invoke(delegate()
			{
				bool? flag = this.Show(data);
				bool? flag2 = flag;
				bool flag3 = true;
				res = (((flag2.GetValueOrDefault() == flag3) & (flag2 != null)) ? PromptResult.OK : PromptResult.Cancel);
			}, DispatcherPriority.Send);
			return res;
		}

		public bool? Show(CredentialPromptData credData)
		{
			if (ProxyCredentialsPrompt._instance == null)
			{
				ProxyCredentialsPrompt._instance = new ProxyCredentialsPromptWindow();
				ProxyCredentialsPrompt._instance.DataContext = new ProxyCredentialsPromptViewModel(credData);
				ProxyCredentialsPrompt._instance.Closed += delegate(object s, EventArgs e)
				{
					ProxyCredentialsPrompt._instance = null;
				};
				return ProxyCredentialsPrompt._instance.ShowDialog();
			}
			ProxyCredentialsPrompt._instance.BringWindowToTop();
			return null;
		}

		private static ProxyCredentialsPromptWindow _instance;

		private Dispatcher _dispatcher;
	}
}
