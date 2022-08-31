using System;
using Krisp.UI.Models;

namespace Krisp.UI.ViewModels
{
	public interface IGenericPageViewModel : IPageViewModel
	{
		void SetModel(IGenericPageModel data);
	}
}
