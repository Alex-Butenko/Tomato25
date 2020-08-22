using System;
using System.Runtime.InteropServices;

namespace Tomato25 {
    static class WinAPIWrappers {
        public static void SetWindowStyles(IntPtr handle) {
            const uint style = (uint)(WindowStyles.WS_VISIBLE
                                  | WindowStyles.WS_POPUP
                                  | WindowStyles.WS_CLIPSIBLINGS);
            const uint exStyle = (uint)(WindowStylesEx.WS_EX_LEFT
                                    | WindowStylesEx.WS_EX_LTRREADING
                                    | WindowStylesEx.WS_EX_RIGHTSCROLLBAR
                                    | WindowStylesEx.WS_EX_TOPMOST
                                    | WindowStylesEx.WS_EX_TOOLWINDOW
                                    | WindowStylesEx.WS_EX_LAYERED);
            SetWindowLongPtr(handle, GWL.STYLE, style);
            SetWindowLongPtr(handle, GWL.EXSTYLE, exStyle);
        }

        static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, uint dwNewLong) {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, uint dwNewLong);

        [Flags]
        enum WindowStyles : uint {
            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
            /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            /// </summary>
            WS_CLIPSIBLINGS = 0x4000000,

            /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
            WS_POPUP = 0x80000000u,

            /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
            WS_VISIBLE = 0x10000000,
        }
        [Flags]
        enum WindowStylesEx : uint {
            /// <summary>
            /// Specifies a window that is a layered window.
            /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WS_EX_LAYERED = 0x00080000,

            /// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
            WS_EX_LEFT = 0x00000000,

            /// <summary>
            /// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
            /// </summary>
            WS_EX_LTRREADING = 0x00000000,

            /// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            /// <summary>
            /// Specifies a window that is intended to be used as a floating toolbar.
            /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
            /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
            /// If a tool window has a system menu, its icon is not displayed on the title bar.
            /// However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
            /// </summary>
            WS_EX_TOOLWINDOW = 0x00000080,

            /// <summary>
            /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
            /// To add or remove this style, use the SetWindowPos function.
            /// </summary>
            WS_EX_TOPMOST = 0x00000008,
        }
        static class GWL {
            /// <summary>
            /// Retrieves the extended window styles.
            /// </summary>
            public const int EXSTYLE = -20;
            /// <summary>
            /// Retrieves the window styles.
            /// </summary>
            public const int STYLE = -16;
        }
    }
}