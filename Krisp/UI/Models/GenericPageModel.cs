using System;
using Krisp.Services;
using Krisp.UI.ViewModels;
using MVVMFoundation;

namespace Krisp.UI.Models
{
	public class GenericPageModel : IGenericPageModel
	{
		public string PageText { get; set; } = TranslationSourceViewModel.Instance["SomethingWentWrong"];

		public Action DefaultAction { get; set; }

		public string ButtonText { get; set; } = TranslationSourceViewModel.Instance["ReportAProblem"];

		public Action ButtonAction { get; set; } = delegate()
		{
			ServiceContainer.Instance.GetService<IRelayCommandsService>().ReportBugCommand.Execute(null);
		};
	}
}
