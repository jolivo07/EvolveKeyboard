using KeyboardDesigner.Services;
using KeyboardDesigner.ViewModels;
using SharedLibrary.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace KeyboardDesigner.Views
{
    public partial class DesignerView : UserControl
    {
        private DesignerInteractionService _interactionService;

        public DesignerView()
        {
            InitializeComponent();
            _interactionService = new DesignerInteractionService();
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = this.DataContext as DesignerViewModel;
            if (vm != null && vm.IsTestMode) return; // Do not handle drag/selection in Test Mode

            var element = sender as FrameworkElement;
            if (element == null) return;

            var model = element.DataContext as KeyboardButtonModel;
            if (model != null)
            {
                if (vm != null)
                {
                     vm.SelectedButton = model;
                }

                _interactionService.StartDrag(model, e.GetPosition(this));
                element.CaptureMouse();
                
                // We don't set Handled=true so standard button behaviors might still try to work,
                // but capturing mouse usually interrupts them if not careful.
                // For a designer, we take control.
            }
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_interactionService.IsDragging)
            {
                _interactionService.UpdateDrag(e.GetPosition(this));
            }
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
             if (_interactionService.IsDragging)
            {
                _interactionService.StopDrag();
                var element = sender as FrameworkElement;
                if (element != null)
                {
                    element.ReleaseMouseCapture();
                }
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var model = thumb?.DataContext as KeyboardButtonModel;
            if (model != null)
            {
                _interactionService.ResizeButton(model, e.HorizontalChange, e.VerticalChange);
            }
        }

        private void WindowThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var vm = this.DataContext as DesignerViewModel;
            if (vm?.Layout == null) return;

            // Calculate dynamic scale inverse
            double scale = 1.0;
            if (vm.PreviewScale > 0)
            {
                scale = 1.0 / vm.PreviewScale;
            }

            vm.Layout.WindowX += e.HorizontalChange * scale;
            vm.Layout.WindowY += e.VerticalChange * scale;
        }
    }
}
