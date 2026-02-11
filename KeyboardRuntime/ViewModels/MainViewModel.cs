using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLibrary.Services;
using Microsoft.Win32;
using SharedLibrary.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KeyboardRuntime.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly KeyExecutionService _executionService;

        [ObservableProperty]
        private KeyboardLayoutModel? layout;

        [ObservableProperty]
        private KeyboardPageModel? currentPage;

        public MainViewModel()
        {
            _executionService = new KeyExecutionService();
            _executionService.CommandRequested += OnCommandRequested;
            
            // Auto-load logic moved to explicit method to control visibility
        }

        public async Task TryAutoLoadNearbyAsync()
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var currentDir = Environment.CurrentDirectory;
                var candidates = new[]
                {
                    Path.Combine(baseDir, "EvolveKeyboard.json"),
                    Path.Combine(baseDir, "Layouts", "EvolveKeyboard.json"),
                    Path.Combine(currentDir, "EvolveKeyboard.json"),
                    Path.Combine(currentDir, "Layouts", "EvolveKeyboard.json"),
                    Path.Combine(baseDir, "keyboardlayout.json"),           // fallback common name
                    Path.Combine(baseDir, "MountFocusLayout.json")          // for dev/testing
                };

                string? found = candidates.FirstOrDefault(File.Exists);

                if (found == null)
                {
                    var recursive = Directory.EnumerateFiles(baseDir, "EvolveKeyboard.json", SearchOption.AllDirectories).FirstOrDefault();
                    found = recursive;
                }

                if (found != null)
                {
                    var loaded = await LayoutSerializer.LoadAsync(found);
                    if (loaded != null)
                    {
                        Layout = loaded;
                        CurrentPage = Layout.Pages.FirstOrDefault();
                    }
                }
            }
            catch
            {
                // swallow exceptions to avoid blocking startup; user can still load manually
            }
        }

        private async Task LoadDefaultLayoutAsync()
        {
             var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VirtualKeyboardSuite", "Layouts", "keyboardlayout.json");
             if (File.Exists(path))
             {
                 var loaded = await LayoutSerializer.LoadAsync(path);
                 if (loaded != null)
                 {
                     Layout = loaded;
                     CurrentPage = Layout.Pages.FirstOrDefault();
                 }
             }
             else
             {
                 // Create a minimal valid layout if no file exists (Empty, not example)
                 Layout = new KeyboardLayoutModel { Name = "Default", Width = 1000, Height = 600 };
                 var mainPage = new KeyboardPageModel { Name = "Main" };
                 Layout.Pages.Add(mainPage);
                 CurrentPage = mainPage;
             }
        }

        private void OnCommandRequested(object? sender, string command)
        {
            if (Layout == null || CurrentPage == null) return;

            if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            if (command.StartsWith("Navigate:"))
            {
                var targetPageName = command.Substring("Navigate:".Length);
                var targetPage = Layout.Pages.FirstOrDefault(p => p.Name.Equals(targetPageName, StringComparison.OrdinalIgnoreCase));
                if (targetPage != null)
                {
                    CurrentPage = targetPage;
                }
            }
            else if (command == "NextPage")
            {
                var index = Layout.Pages.IndexOf(CurrentPage);
                if (index < Layout.Pages.Count - 1)
                {
                    CurrentPage = Layout.Pages[index + 1];
                }
                else
                {
                    CurrentPage = Layout.Pages.FirstOrDefault(); // Loop back
                }
            }
            else if (command == "PreviousPage")
            {
                var index = Layout.Pages.IndexOf(CurrentPage);
                if (index > 0)
                {
                    CurrentPage = Layout.Pages[index - 1];
                }
                else
                {
                    CurrentPage = Layout.Pages.LastOrDefault(); // Loop back
                }
            }
        }

        [RelayCommand]
        private async Task LoadLayout()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var loaded = await LayoutSerializer.LoadAsync(dialog.FileName);
                if (loaded != null)
                {
                    Layout = loaded;
                    CurrentPage = Layout.Pages.FirstOrDefault();
                }
            }
        }

        [RelayCommand]
        private void ExecuteButton(KeyboardButtonModel button)
        {
            _executionService.ExecuteAction(button);
        }

        [RelayCommand]
        private void ChangePage(KeyboardPageModel page)
        {
            CurrentPage = page;
        }
    }
}
