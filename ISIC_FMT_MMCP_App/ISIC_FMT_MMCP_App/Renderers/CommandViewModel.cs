using System.Windows.Input;

namespace Isic.ViewModels
{
    public class CommandViewModel
    {
        public string Text { get; set; }
        public ICommand Command { get; set; }
    }
}