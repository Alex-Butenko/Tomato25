using System;
using System.Windows;
using Forms = System.Windows.Forms;
using Graphics = System.Drawing.Graphics;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Tomato25 {
    partial class MainWindow {
        class FixPositionHelper {
            public FixPositionHelper(MainWindow window) {
                _window = window;
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero)) {
                    _dpi = graphics.DpiX;
                }
            }

            MainWindow _window;

            bool _dontRaiseLocationSizeChanged;

            public void FixPosition() {
                if (_dontRaiseLocationSizeChanged || !_window.IsLoaded
                    || _window.WindowState == WindowState.Maximized
                     ) {
                    return;
                }
                _dontRaiseLocationSizeChanged = true;
                try {
                    int left = WpfToWinFormsSize(_window.Left);
                    int top = WpfToWinFormsSize(_window.Top);
                    int right = WpfToWinFormsSize(_window.Left + _window.Width);
                    int bottom = WpfToWinFormsSize(_window.Top + _window.Height);
                    const int magicConstForMinimizedState = -32000;
                    if (Math.Abs(magicConstForMinimizedState - left) < 3
                        || Math.Abs(magicConstForMinimizedState - top) < 3) {
                        return;
                    }
                    Rectangle rect = new Rectangle(left, top, right - left, bottom - top);
                    Forms.Screen screenForWindow = null;
                    foreach (Forms.Screen screen in Forms.Screen.AllScreens) {
                        Rectangle area = new Rectangle(screen.WorkingArea.Location, screen.WorkingArea.Size);
                        area.Y -= Forms.SystemInformation.CaptionHeight - 2;
                        area.X -= 30;
                        if (IsPointInsideRectangle(area, rect.Location)) {
                            screenForWindow = screen;
                            break;
                        }
                        area.X += 60;
                        if (IsPointInsideRectangle(area, new Point(rect.Right, rect.Top))) {
                            screenForWindow = screen;
                            break;
                        }
                    }
                    if (screenForWindow != null) {
                        SnapWindowToBounds(screenForWindow.WorkingArea, rect);
                    }
                    else {
                        int shortestDistance = int.MaxValue;
                        Point windowCenter = CenterOfRectangle(rect);
                        foreach (Forms.Screen screen in Forms.Screen.AllScreens) {
                            if (DistanceToRectangleCentre(screen.WorkingArea, windowCenter) < shortestDistance) {
                                screenForWindow = screen;
                            }
                        }
                        MoveWindowInsideScreen(screenForWindow.WorkingArea, rect);
                    }
                }
                finally {
                    _dontRaiseLocationSizeChanged = false;
                }
            }

            int WpfToWinFormsSize(double size) => (int)(size * _dpi / 96.0);
            double WinFormsToWpfSize(int size) => (size * 96.0) / _dpi;

            float _dpi;

            int DistanceToRectangleCentre(Rectangle rect, Point point) {
                Point center = CenterOfRectangle(rect);
                return (int)Math.Sqrt(Math.Pow(center.X - point.X, 2)
                    + Math.Pow(center.Y - point.Y, 2));
            }

            Point CenterOfRectangle(Rectangle rect) {
                int centerX = (rect.Left + rect.Right) / 2;
                int centerY = (rect.Top + rect.Bottom) / 2;
                return new Point(centerX, centerY);
            }

            bool IsPointInsideRectangle(Rectangle rect, Point point) {
                return rect.Left <= point.X && point.X <= rect.Right
                    && rect.Top <= point.Y && point.Y <= rect.Bottom;
            }

            bool IsRectangleInsideRectangle(Rectangle outerRect, Rectangle innerRect) {
                return IsPointInsideRectangle(outerRect, new Point(innerRect.Left, innerRect.Top))
                    && IsPointInsideRectangle(outerRect, new Point(innerRect.Left, innerRect.Bottom))
                    && IsPointInsideRectangle(outerRect, new Point(innerRect.Right, innerRect.Top))
                    && IsPointInsideRectangle(outerRect, new Point(innerRect.Right, innerRect.Bottom));
            }

            void MoveWindowInsideScreen(Rectangle screen, Rectangle window) {
                if ((screen.Height + 100) < window.Height) {
                    window.Height = screen.Height;
                }
                if ((screen.Width + 100) < window.Width) {
                    window.Width = screen.Width;
                }
                if (window.Right > screen.Right) {
                    window.X -= (window.Right - screen.Right);
                }
                if (window.Bottom > screen.Bottom) {
                    window.Y -= (window.Bottom - screen.Bottom);
                }
                if (window.X < screen.X) {
                    window.X = screen.X;
                }
                if (window.Y < screen.Y) {
                    window.Y = screen.Y;
                }
                _window.Model.Left = WinFormsToWpfSize(window.Left);
                _window.Model.Top = WinFormsToWpfSize(window.Top);
                _window.Model.Width = WinFormsToWpfSize(window.Width);
                _window.Model.Height = WinFormsToWpfSize(window.Height);
            }

            void SnapWindowToBounds(Rectangle screen, Rectangle window) {
                const int snapingDistance = 15;
                if ((window.X > screen.X) && (window.X - screen.X) < snapingDistance) {
                    window.X = screen.X;
                }
                if ((window.Y > screen.Y) && (window.Y - screen.Y) < snapingDistance) {
                    window.Y = screen.Y;
                }
                if ((window.Right < screen.Right) && (screen.Right - window.Right) < snapingDistance) {
                    window.X -= (window.Right - screen.Right);
                }
                if ((window.Bottom < screen.Bottom) && (screen.Bottom - window.Bottom) < snapingDistance) {
                    window.Y -= (window.Bottom - screen.Bottom);
                }
                _window.Model.Left = WinFormsToWpfSize(window.Left);
                _window.Model.Top = WinFormsToWpfSize(window.Top);
                _window.Model.Width = WinFormsToWpfSize(window.Width);
                _window.Model.Height = WinFormsToWpfSize(window.Height);
            }
        }
    }
}
