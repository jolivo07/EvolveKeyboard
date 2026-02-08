using CommunityToolkit.Mvvm.ComponentModel;

namespace SharedLibrary.Models
{
    public partial class KeyboardButtonModel : ObservableObject
    {
        [ObservableProperty]
        private string text = "Key";

        [ObservableProperty]
        private string color = "#DDDDDD";

        [ObservableProperty]
        private string textColor = "#000000";

        [ObservableProperty]
        private double fontSize = 14;

        [ObservableProperty]
        private bool isBold = false;

        [ObservableProperty]
        private double width = 60;

        [ObservableProperty]
        private double height = 60;

        [ObservableProperty]
        private double x = 0;

        [ObservableProperty]
        private double y = 0;

        [ObservableProperty]
        private string action = "SendKey";

        [ObservableProperty]
        private string value = "";
    }
}
