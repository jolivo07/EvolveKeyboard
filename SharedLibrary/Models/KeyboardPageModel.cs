using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SharedLibrary.Models
{
    public partial class KeyboardPageModel : ObservableObject
    {
        [ObservableProperty]
        private string name = "Page 1";

        [ObservableProperty]
        private ObservableCollection<KeyboardButtonModel> buttons = new();
    }
}
