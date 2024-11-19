using EBrief.ViewModels;

namespace EBrief.Stores
{
    public class ModalNavigationStore
    {
        private ViewModelBase? _currentViewModel;

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel?.Dispose();
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public bool IsOpen => _currentViewModel != null;

        public event Action? CurrentViewModelChanged;

        public void Close() => _currentViewModel = null;

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
