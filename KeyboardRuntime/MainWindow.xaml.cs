using KeyboardRuntime.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace KeyboardRuntime
{
    public partial class MainWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private MainViewModel _viewModel;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = _viewModel;
            
            // Start invisible to avoid flickering
            this.Opacity = 0;

            // Auto-load layout before showing window
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.TryAutoLoadNearbyAsync();
                
                // Force position update after layout load and before showing
                if (_viewModel.Layout != null)
                {
                    this.Left = _viewModel.Layout.WindowX;
                    this.Top = _viewModel.Layout.WindowY;
                    this.Width = _viewModel.Layout.Width;
                    this.Height = _viewModel.Layout.Height;
                }
            }
            finally
            {
                // Ensure window becomes visible regardless of load success/failure
                this.Opacity = 1;
                this.Activate(); // Bring to foreground
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Layout) && _viewModel.Layout != null)
            {
                // Force update window position and size when Layout changes
                // This overrides any previous manual movements (DragMove) which break bindings
                this.Left = _viewModel.Layout.WindowX;
                this.Top = _viewModel.Layout.WindowY;
                this.Width = _viewModel.Layout.Width;
                this.Height = _viewModel.Layout.Height;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
