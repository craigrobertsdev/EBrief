using System.Windows.Input;

namespace EBrief.ViewModels
{
    public class AddNewCourtListViewModel : ViewModelBase
    {
		private string _selectedFileName;
		public string SelectedFileName
		{
			get
			{
				return _selectedFileName;
			}
			set
			{
				_selectedFileName = value;
				OnPropertyChanged(nameof(SelectedFileName));
			}
		}

		public ICommand ChooseFileCommand { get; }

		public AddNewCourtListViewModel()
		{

		}
	}
}
