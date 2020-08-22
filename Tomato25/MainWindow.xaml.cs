﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Tomato25 {
    partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                return;
            }
            DataContext = new MainModel();
            Model.RestoreRequest += Restore;
            Model.MinimizeRequest += Minimize;
            Model.MaximizeRequest += Maximize;
            _fixPositionHelper = new FixPositionHelper(this);
        }

        public MainModel Model => (MainModel)DataContext;

        readonly FixPositionHelper _fixPositionHelper;

        void Window_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                Minimize();
            }
            else {
                Restore();
            }
        }

        void Window_Closing(object sender, CancelEventArgs e) {
            if (!Debugger.IsAttached) {
                e.Cancel = true;
                Minimize();
            }
        }

        bool _preventLoopIsVisible;
        void Minimize() {
            if (_preventLoopIsVisible) {
                return;
            }
            _preventLoopIsVisible = true;
            try {
                WindowState = WindowState.Minimized;
                Hide();
                Model.IsVisible = false;
            }
            finally {
                _preventLoopIsVisible = false;
            }
        }
        public void Restore() {
            if (_preventLoopIsVisible) {
                return;
            }
            _preventLoopIsVisible = true;
            try {
                Show();
                if (WindowState == WindowState.Minimized) {
                    WindowState = WindowState.Normal;
                }
                Model.IsVisible = true;
            }
            finally {
                _preventLoopIsVisible = false;
            }
        }
        void Maximize(bool maximize) {
            Show();
            if (maximize) {
                WindowState = WindowState.Maximized;
            }
            else {
                WindowState = WindowState.Normal;
            }
        }

        void Window_LocationChanged(object sender, EventArgs e) {
            _fixPositionHelper.FixPosition();
        }

        void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            _fixPositionHelper.FixPosition();
        }

        void ButtonStart_Click(object sender, RoutedEventArgs e) {
            Model.StartWork();
        }

        void ButtonPause_Click(object sender, RoutedEventArgs e) {
            Model.PauseWork();
        }

        void ButtonShortBreak_Click(object sender, RoutedEventArgs e) {
            Model.StartBreak();
        }

        void ButtonLongBreak_Click(object sender, RoutedEventArgs e) {
            Model.StartLongBreak();
        }

        void ButtonStop_Click(object sender, RoutedEventArgs e) {
            Model.Stop();
        }

        void ButtonClose_Click(object sender, RoutedEventArgs e) {
            if (Debugger.IsAttached) {
                Environment.Exit(0);
            }
            Minimize();
        }

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        void Window_Loaded(object sender, RoutedEventArgs e) {
            IntPtr handle = new WindowInteropHelper(this).EnsureHandle();

            new Action(() => WinAPIWrappers.SetWindowStyles(handle)).BeginInvoke(null, null);
        }
    }
}