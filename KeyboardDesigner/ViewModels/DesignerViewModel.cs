using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SharedLibrary.Models;
using SharedLibrary.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KeyboardDesigner.ViewModels
{
    public partial class DesignerViewModel : ObservableObject
    {
        [ObservableProperty]
        private KeyboardLayoutModel layout;

        [ObservableProperty]
        private KeyboardPageModel? selectedPage;

        [ObservableProperty]
        private KeyboardButtonModel? selectedButton;

        [ObservableProperty]
        private bool isTestMode;

        [ObservableProperty]
        private double screenWidth;

        [ObservableProperty]
        private double screenHeight;

        [ObservableProperty]
        private double previewScale = 0.2; // Default fallback

        private KeyExecutionService _keyExecutionService;

        public DesignerViewModel()
        {
            _keyExecutionService = new KeyExecutionService();
            
            // Capture Primary Screen Size
            ScreenWidth = SystemParameters.PrimaryScreenWidth;
            ScreenHeight = SystemParameters.PrimaryScreenHeight;
            
            // Calculate scale to fit in sidebar (Max width ~260px, Max height ~200px)
            double targetWidth = 260.0;
            double targetHeight = 200.0;
            
            if (ScreenWidth > 0 && ScreenHeight > 0)
            {
                double scaleX = targetWidth / ScreenWidth;
                double scaleY = targetHeight / ScreenHeight;
                PreviewScale = Math.Min(scaleX, scaleY);
            }
            else if (ScreenHeight > 0)
            {
                PreviewScale = targetHeight / ScreenHeight;
            }

            Layout = new KeyboardLayoutModel();
            var defaultPage = new KeyboardPageModel { Name = "Main" };
            Layout.Pages.Add(defaultPage);
            SelectedPage = defaultPage;
        }

        [RelayCommand]
        private void ToggleTestMode()
        {
            IsTestMode = !IsTestMode;
            if (IsTestMode)
            {
                SelectedButton = null; // Deselect when entering test mode
            }
        }

        [RelayCommand]
        private void ExecuteButton(KeyboardButtonModel button)
        {
            if (IsTestMode)
            {
                _keyExecutionService.ExecuteAction(button);
            }
            else
            {
                // Should not happen if binding is correct, but safe guard
                SelectButton(button);
            }
        }


        [RelayCommand]
        private void AddPage()
        {
            string baseName = "Page";
            int count = Layout.Pages.Count + 1;
            string newName = $"{baseName} {count}";

            // Ensure unique name
            while (Layout.Pages.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                count++;
                newName = $"{baseName} {count}";
            }

            var newPage = new KeyboardPageModel { Name = newName };
            Layout.Pages.Add(newPage);
            SelectedPage = newPage;
        }

        [RelayCommand]
        private void AddButton()
        {
            if (SelectedPage == null) return;
            var btn = new KeyboardButtonModel 
            { 
                Text = "New", 
                X = 10, 
                Y = 10, 
                Width = 80, 
                Height = 50 
            };
            SelectedPage.Buttons.Add(btn);
            SelectedButton = btn;
        }

        [RelayCommand]
        private void DeleteButton()
        {
            if (SelectedPage != null && SelectedButton != null)
            {
                SelectedPage.Buttons.Remove(SelectedButton);
                SelectedButton = null;
            }
        }

        [RelayCommand]
        private void SelectButton(KeyboardButtonModel btn)
        {
            SelectedButton = btn;
        }

        [RelayCommand]
        private async Task CreateExample()
        {
            Layout = LayoutSerializer.GenerateExampleLayout();
            SelectedPage = Layout.Pages.FirstOrDefault();
            SelectedButton = null;

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VirtualKeyboardSuite", "Layouts", "keyboardlayout.json");
            await LayoutSerializer.SaveAsync(Layout, path);
            
            MessageBox.Show($"Example Layout created and saved to:\n{path}", "Success");
        }

        [RelayCommand]
        private async Task SaveLayout()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                FileName = "keyboardlayout.json"
            };

            if (dialog.ShowDialog() == true)
            {
                await LayoutSerializer.SaveAsync(Layout, dialog.FileName);
                MessageBox.Show("Layout Saved!");
            }
        }

        [RelayCommand]
        private async Task LoadLayout()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var loaded = await LayoutSerializer.LoadAsync(dialog.FileName);
                if (loaded != null)
                {
                    Layout = loaded;
                    SelectedPage = Layout.Pages.FirstOrDefault();
                    SelectedButton = null;
                }
            }
        }
    }
}
