using KeyboardRuntime.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace KeyboardRuntime
{
    public partial class MainWindow : System.Windows.Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private MainViewModel _viewModel;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = _viewModel;

            InitializeNotifyIcon();

            Closed += MainWindow_Closed;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _viewModel.TryAutoLoadNearbyAsync();

                if (_viewModel.Layout != null)
                {
                    Left = _viewModel.Layout.WindowX;
                    Top = _viewModel.Layout.WindowY;
                    Width = _viewModel.Layout.Width;
                    Height = _viewModel.Layout.Height;
                    ClampToCurrentScreenWorkArea();
                }
            }
            finally
            {
                Opacity = 1;
                ShowNoActivate();
            }
        }

        private void ShowNoActivate()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            if (!IsVisible)
            {
                Show();
            }

            var helper = new WindowInteropHelper(this);
            if (helper.Handle != IntPtr.Zero)
            {
                SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        }

        private void ClampToCurrentScreenWorkArea()
        {
            var dpi = VisualTreeHelper.GetDpi(this);

            var topLeftPx = new System.Drawing.Point(
                (int)Math.Round(Left * dpi.DpiScaleX),
                (int)Math.Round(Top * dpi.DpiScaleY)
            );

            var screen = System.Windows.Forms.Screen.FromPoint(topLeftPx);
            var waPx = screen.WorkingArea;

            var waLeft = waPx.Left / dpi.DpiScaleX;
            var waTop = waPx.Top / dpi.DpiScaleY;
            var waRight = (waPx.Left + waPx.Width) / dpi.DpiScaleX;
            var waBottom = (waPx.Top + waPx.Height) / dpi.DpiScaleY;

            var newLeft = Left;
            var newTop = Top;

            if (newLeft < waLeft) newLeft = waLeft;
            if (newTop < waTop) newTop = waTop;

            if (newLeft + Width > waRight) newLeft = waRight - Width;
            if (newTop + Height > waBottom) newTop = waBottom - Height;

            if (newLeft < waLeft) newLeft = waLeft;
            if (newTop < waTop) newTop = waTop;

            Left = newLeft;
            Top = newTop;
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application; 
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Evolve Keyboard Runtime";
            
            // Double click to show/bring to front
            _notifyIcon.DoubleClick += (s, args) => 
            {
                WindowState = WindowState.Normal;
                ShowNoActivate();
            };

            // Context menu
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Show Keyboard", null, (s, args) => 
            {
                WindowState = WindowState.Normal;
                ShowNoActivate();
            });
            contextMenu.Items.Add("-"); // Separator
            contextMenu.Items.Add("Exit", null, (s, args) => Close());
            
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
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
                ClampToCurrentScreenWorkArea();
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
