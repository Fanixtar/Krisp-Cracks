using System;

namespace Krisp.UI.Models
{
	public interface IGenericPageModel
	{
		string PageText { get; set; }

		Action DefaultAction { get; set; }

		string ButtonText { get; set; }

		Action ButtonAction { get; set; }
	}
}
