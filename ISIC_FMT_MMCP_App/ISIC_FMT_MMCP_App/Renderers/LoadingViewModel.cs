using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Xamarin.Forms;
using System.Collections.Generic;

namespace Isic.ViewModels
{
    public class LoadingViewModel : AbstractViewModel
    {
        public ICommand LoadCommand { get; set; }

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
                        await Task.Delay(TimeSpan.FromSeconds(5), cancelSrc.Token);
                    }
                    catch { }
                }
                //this.Result(cancelSrc.IsCancellationRequested ? "Search Cancelled" : "Search Completed");
            });
        }
    }
}
