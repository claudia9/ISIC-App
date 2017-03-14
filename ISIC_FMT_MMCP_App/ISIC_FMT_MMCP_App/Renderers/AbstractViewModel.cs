using Acr.UserDialogs;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Isic.ViewModels
{
    public abstract class AbstractViewModel: INotifyPropertyChanged
    {

        protected AbstractViewModel(IUserDialogs dialogs)
        {
            this.Dialogs = dialogs;
        }

        protected IUserDialogs Dialogs { get; }

        protected virtual void Result(string msg)
        {
            this.Dialogs.Alert(msg);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}