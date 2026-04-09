using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace KeyboardRuntime.Controls
{
    public class KeyboardCanvas : FrameworkElement
    {
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register(
                nameof(Buttons),
                typeof(IEnumerable<KeyboardButtonModel>),
                typeof(KeyboardCanvas),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnButtonsChanged));

        public IEnumerable<KeyboardButtonModel>? Buttons
        {
            get => (IEnumerable<KeyboardButtonModel>?)GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(KeyboardCanvas),
                new FrameworkPropertyMetadata(null));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        private static readonly ConcurrentDictionary<string, SolidColorBrush> BrushCache = new(StringComparer.OrdinalIgnoreCase);

        private KeyboardButtonModel? _pressedButton;

        public KeyboardCanvas()
        {
            Focusable = false;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var buttons = Buttons?.ToList();
            if (buttons == null || buttons.Count == 0) return;

            foreach (var btn in buttons)
            {
                DrawButton(dc, btn, ReferenceEquals(btn, _pressedButton));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            System.Windows.Point pos = e.GetPosition(this);
            var hit = HitTestButton(pos);
            if (hit == null) return;

            _pressedButton = hit;
            CaptureMouse();
            InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_pressedButton == null) return;

            System.Windows.Point pos = e.GetPosition(this);
            var releasedOver = HitTestButton(pos);
            var shouldClick = ReferenceEquals(releasedOver, _pressedButton);

            var clicked = _pressedButton;
            _pressedButton = null;
            ReleaseMouseCapture();
            InvalidateVisual();
            e.Handled = true;

            if (!shouldClick) return;

            var cmd = Command;
            if (cmd == null) return;

            if (cmd.CanExecute(clicked))
            {
                cmd.Execute(clicked);
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_pressedButton == null) return;
            _pressedButton = null;
            ReleaseMouseCapture();
            InvalidateVisual();
        }

        private KeyboardButtonModel? HitTestButton(System.Windows.Point p)
        {
            var buttons = Buttons;
            if (buttons == null) return null;

            foreach (var btn in buttons)
            {
                var rect = new System.Windows.Rect(btn.X, btn.Y, btn.Width, btn.Height);
                if (rect.Contains(p))
                {
                    return btn;
                }
            }

            return null;
        }

        private void DrawButton(DrawingContext dc, KeyboardButtonModel btn, bool isPressed)
        {
            var rect = new System.Windows.Rect(btn.X, btn.Y, btn.Width, btn.Height);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            var borderThickness = 2.0;
            var radius = 10.0;

            var background = GetBrush(btn.Color, Colors.Transparent);
            var border = GetBrush(btn.BorderColor, Colors.Transparent);
            var textBrush = GetBrush(btn.TextColor, Colors.Black);

            var opacity = isPressed ? 0.7 : 1.0;

            dc.PushOpacity(opacity);

            var outer = new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height);
            var inner = new System.Windows.Rect(
                rect.X + borderThickness,
                rect.Y + borderThickness,
                Math.Max(0, rect.Width - 2 * borderThickness),
                Math.Max(0, rect.Height - 2 * borderThickness));

            if (border.Color.A > 0 && borderThickness > 0)
            {
                dc.DrawRoundedRectangle(border, null, outer, radius, radius);
            }

            var innerRadius = Math.Max(0, radius - borderThickness);
            dc.DrawRoundedRectangle(background, null, inner, innerRadius, innerRadius);

            DrawText(dc, btn, inner, textBrush);

            dc.Pop();
        }

        private void DrawText(DrawingContext dc, KeyboardButtonModel btn, System.Windows.Rect contentRect, System.Windows.Media.Brush textBrush)
        {
            var text = btn.Text ?? string.Empty;
            if (text.Length == 0) return;

            var fontFamily = GetEffectiveFontFamily();
            var typeface = new Typeface(
                fontFamily,
                FontStyles.Normal,
                btn.IsBold ? FontWeights.Bold : FontWeights.Normal,
                FontStretches.Normal);

            var formatted = new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                btn.FontSize,
                textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip)
            {
                MaxTextWidth = Math.Max(0, contentRect.Width),
                MaxTextHeight = Math.Max(0, contentRect.Height),
                Trimming = TextTrimming.None,
                TextAlignment = TextAlignment.Center
            };

            var x = contentRect.X;
            var y = contentRect.Y + (contentRect.Height - formatted.Height) / 2;
            dc.DrawText(formatted, new System.Windows.Point(x, y));
        }

        private System.Windows.Media.FontFamily GetEffectiveFontFamily()
        {
            try
            {
                var app = System.Windows.Application.Current;
                if (app != null)
                {
                    var candidates = new[]
                    {
                        "MaterialDesignFont",
                        "MaterialDesignFontFamily",
                        "MaterialDesign.BodyFontFamily"
                    };

                    foreach (var key in candidates)
                    {
                        var resource = app.TryFindResource(key);
                        if (resource is System.Windows.Media.FontFamily ff)
                        {
                            return ff;
                        }
                    }
                }
            }
            catch
            {
            }

            return System.Windows.SystemFonts.MessageFontFamily;
        }

        private static SolidColorBrush GetBrush(string? hexOrName, System.Windows.Media.Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hexOrName))
            {
                return GetFrozenBrush(fallback);
            }

            return BrushCache.GetOrAdd(hexOrName.Trim(), key =>
            {
                try
                {
                    var colorObj = System.Windows.Media.ColorConverter.ConvertFromString(key);
                    if (colorObj is System.Windows.Media.Color c)
                    {
                        return GetFrozenBrush(c);
                    }
                }
                catch
                {
                }

                return GetFrozenBrush(fallback);
            });
        }

        private static SolidColorBrush GetFrozenBrush(System.Windows.Media.Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (KeyboardCanvas)d;
            canvas.UnsubscribeButtons(e.OldValue as IEnumerable<KeyboardButtonModel>);
            canvas.SubscribeButtons(e.NewValue as IEnumerable<KeyboardButtonModel>);
            canvas.InvalidateVisual();
        }

        private void SubscribeButtons(IEnumerable<KeyboardButtonModel>? buttons)
        {
            if (buttons == null) return;

            if (buttons is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged += Buttons_CollectionChanged;
            }

            foreach (var btn in buttons)
            {
                btn.PropertyChanged += Button_PropertyChanged;
            }
        }

        private void UnsubscribeButtons(IEnumerable<KeyboardButtonModel>? buttons)
        {
            if (buttons == null) return;

            if (buttons is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= Buttons_CollectionChanged;
            }

            foreach (var btn in buttons)
            {
                btn.PropertyChanged -= Button_PropertyChanged;
            }
        }

        private void Buttons_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<KeyboardButtonModel>())
                {
                    item.PropertyChanged -= Button_PropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<KeyboardButtonModel>())
                {
                    item.PropertyChanged += Button_PropertyChanged;
                }
            }

            InvalidateVisual();
        }

        private void Button_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }
    }
}
