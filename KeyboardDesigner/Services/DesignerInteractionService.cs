using SharedLibrary.Models;
using System.Windows;

namespace KeyboardDesigner.Services
{
    public class DesignerInteractionService
    {
        private Point _startClick;
        private double _startX;
        private double _startY;
        private bool _isDragging;
        private KeyboardButtonModel? _target;

        public bool IsDragging => _isDragging;

        public void StartDrag(KeyboardButtonModel target, Point mousePosition)
        {
            _target = target;
            _startClick = mousePosition;
            _startX = target.X;
            _startY = target.Y;
            _isDragging = true;
        }

        public void UpdateDrag(Point currentMousePosition)
        {
            if (!_isDragging || _target == null) return;

            double offsetX = currentMousePosition.X - _startClick.X;
            double offsetY = currentMousePosition.Y - _startClick.Y;

            // Ensure we don't drag to negative coordinates
            double newX = _startX + offsetX;
            double newY = _startY + offsetY;

            if (newX >= 0) _target.X = newX;
            if (newY >= 0) _target.Y = newY;
        }

        public void StopDrag()
        {
            _isDragging = false;
            _target = null;
        }

        public void ResizeButton(KeyboardButtonModel button, double deltaHorizontal, double deltaVertical)
        {
            // Apply resize with min size limits
            double newWidth = System.Math.Max(20, button.Width + deltaHorizontal);
            double newHeight = System.Math.Max(20, button.Height + deltaVertical);

            // Check if Shift key is pressed for aspect ratio maintenance
            bool maintainAspectRatio = System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || 
                                     System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);

            if (maintainAspectRatio && button.Width > 0 && button.Height > 0)
            {
                double ratio = button.Width / button.Height;
                // Use the larger delta to drive the resize
                if (System.Math.Abs(deltaHorizontal) > System.Math.Abs(deltaVertical))
                {
                    newHeight = newWidth / ratio;
                }
                else
                {
                    newWidth = newHeight * ratio;
                }
            }

            button.Width = newWidth;
            button.Height = newHeight;
        }
    }
}
