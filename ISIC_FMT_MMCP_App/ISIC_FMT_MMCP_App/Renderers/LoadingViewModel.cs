using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Xamarin.Forms;
using Isic.Debugger;

namespace Isic.ViewModels
{
    public class LoadingViewModel : AbstractViewModel
    {
        public ICommand loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                if (loadCommand == null)
                {
                    loadCommand = LoadingCommand();
                }
                return loadCommand;
            }
            set
            {
                loadCommand = value;
            }
        }
        public LoadingViewModel(IUserDialogs dialogs) : base(dialogs)
        {
            LoadCommand = LoadingCommand();
        }

        public ICommand LoadingCommand()
        {
            return new Command(async () =>
            {
                var cancelSrc = new CancellationTokenSource();
                var config = new ProgressDialogConfig()
                    .SetTitle("Searching for devices...")
                    .SetIsDeterministic(false)
                    .SetMaskType(MaskType.Black);
                //.SetCancel(onCancel: cancelSrc.Cancel);

                using (this.Dialogs.Progress(config))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3), cancelSrc.Token);
                    }
                    catch { }
                }
                //this.Result(cancelSrc.IsCancellationRequested ? "Search Cancelled" : "Search Completed");
            });
        }

        public ICommand savePreferences;
        public ICommand SavePreferences
        {
            get
            {
                if (savePreferences == null)
                {
                    savePreferences = SavingCommand();
                }
                return savePreferences;
            }
            set
            {
                savePreferences = value;
            }
        }

        

        public ICommand SavingCommand()
        {
            return new Command(async () =>
            {
                var config = new ProgressDialogConfig()
                    .SetTitle("Saving your preferences...")
                    .SetIsDeterministic(false)
                    .SetMaskType(MaskType.Black);

                using (this.Dialogs.Progress(config))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                    catch { }

                }
                this.Dialogs.ShowSuccess("Preferences saved!");
            });
        }
    }
}
