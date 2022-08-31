using System;
using System.Windows.Input;
using Krisp.UI.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	internal class GenericPageViewModel : BindableBase, IGenericPageViewModel, IPageViewModel
	{
		public IGenericPageModel Model
		{
			get
			{
				return this._model;
			}
			set
			{
				if (value != this._model)
				{
					this._model = value;
					base.RaisePropertyChanged("Model");
				}
			}
		}

		public GenericPageViewModel()
		{
			Action defaultAction = this.Model.DefaultAction;
			if (defaultAction == null)
			{
				return;
			}
			defaultAction();
		}

		public void SetModel(IGenericPageModel model)
		{
			this.Model = model;
		}

		public ICommand ButtonCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._buttonCommand) == null)
				{
					relayCommand = (this._buttonCommand = new RelayCommand(delegate(object param)
					{
						Action buttonAction = this.Model.ButtonAction;
						if (buttonAction == null)
						{
							return;
						}
						buttonAction();
					}));
				}
				return relayCommand;
			}
		}

		public MenuItemsVisibility MenuItemsVisibility { get; } = new MenuItemsVisibility();

		private IGenericPageModel _model = new GenericPageModel();

		private RelayCommand _buttonCommand;
	}
}
