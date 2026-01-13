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
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private KeyboardLayoutModel layout;

        [ObservableProperty]
        private KeyboardPageModel? selectedPage;

        [ObservableProperty]
        private KeyboardButtonModel? selectedButton;

        public MainViewModel()
        {
            Layout = new KeyboardLayoutModel();
            var defaultPage = new KeyboardPageModel { Name = "Main" };
            Layout.Pages.Add(defaultPage);
            SelectedPage = defaultPage;
        }

        [RelayCommand]
        private void AddPage()
        {
            var newPage = new KeyboardPageModel { Name = $"Page {Layout.Pages.Count + 1}" };
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
