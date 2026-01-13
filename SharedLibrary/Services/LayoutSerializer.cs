using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SharedLibrary.Models;
using System.Collections.ObjectModel;

namespace SharedLibrary.Services
{
    public static class LayoutSerializer
    {
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public static async Task SaveAsync(KeyboardLayoutModel layout, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, layout, _options);
        }

        public static async Task<KeyboardLayoutModel?> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<KeyboardLayoutModel>(stream, _options);
        }

        public static KeyboardLayoutModel GenerateExampleLayout()
        {
            var layout = new KeyboardLayoutModel { Name = "Example Keyboard" };

            // Page 1: Main
            var page1 = new KeyboardPageModel { Name = "Main" };
            
            // Button 1: SendKey "1", Yellow
            page1.Buttons.Add(new KeyboardButtonModel 
            { 
                Text = "1", 
                Value = "VK_1", 
                Action = "SendKey", 
                Color = "#FFD700", // Yellow
                Width = 80, Height = 80, X = 50, Y = 50 
            });

            // Button 2: SendKey "2", Orange
            page1.Buttons.Add(new KeyboardButtonModel 
            { 
                Text = "2", 
                Value = "VK_2", 
                Action = "SendKey", 
                Color = "#FFA500", // Orange
                Width = 80, Height = 80, X = 150, Y = 50 
            });

            // Button Next: RunCommand "NextPage", Blue
            page1.Buttons.Add(new KeyboardButtonModel 
            { 
                Text = "Next", 
                Value = "NextPage", 
                Action = "RunCommand", 
                Color = "#4169E1", // RoyalBlue
                Width = 180, Height = 60, X = 50, Y = 150 
            });

            layout.Pages.Add(page1);

            // Page 2: Commands
            var page2 = new KeyboardPageModel { Name = "Commands" };

            // Button Cash Drawer: RunCommand "open_cash_drawer", Green
            page2.Buttons.Add(new KeyboardButtonModel 
            { 
                Text = "Cash Drawer", 
                Value = "open_cash_drawer", 
                Action = "RunCommand", 
                Color = "#32CD32", // LimeGreen
                Width = 150, Height = 80, X = 50, Y = 50 
            });

            // Button Back: RunCommand "PreviousPage", Gray
            page2.Buttons.Add(new KeyboardButtonModel 
            { 
                Text = "Back", 
                Value = "PreviousPage", 
                Action = "RunCommand", 
                Color = "#808080", // Gray
                Width = 150, Height = 60, X = 50, Y = 150 
            });

            layout.Pages.Add(page2);

            return layout;
        }
    }
}
