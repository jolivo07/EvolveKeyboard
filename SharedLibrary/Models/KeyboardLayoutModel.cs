using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SharedLibrary.Models
{
    public partial class KeyboardLayoutModel : ObservableObject
    {
        [ObservableProperty]
        private string name = "New Keyboard";

        [ObservableProperty]
        private double width = 1000;

        [ObservableProperty]
        private double height = 600;

        [ObservableProperty]
        private double windowX = 100;

        [ObservableProperty]
        private double windowY = 100;

        [ObservableProperty]
        private ObservableCollection<KeyboardPageModel> pages = new();
    }
}
