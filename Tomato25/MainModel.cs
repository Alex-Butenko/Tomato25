using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Tomato25 {
    class MainModel : INotifyPropertyChanged {
        public MainModel() {
            StartCommand = new RelayCommand(_ => StartWork(), _ => Mode != Mode.Work);
            PauseCommand = new RelayCommand(_ => PauseWork(), _ => Mode == Mode.Work);
            BreakCommand = new RelayCommand(_ => StartBreak(), _ => Mode == Mode.Work || Mode == Mode.Pause);
            LongBreakCommand = new RelayCommand(_ => StartLongBreak(), _ => Mode == Mode.Work || Mode == Mode.Pause);
            StopCommand = new RelayCommand(_ => Stop(), _ => Mode != Mode.Inactive);
        }

        TimeSpan _time;
        public TimeSpan Time {
            get => _time;
            set {
                _time = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Progress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        Mode _mode = Mode.Inactive;
        public Mode Mode {
            get => _mode;
            set {
                _mode = value;
                NotifyPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand StartCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand BreakCommand { get; }

        public ICommand LongBreakCommand { get; }

        public ICommand StopCommand { get; }

        void StartWork() {
            MaximizeRequest?.Invoke(false);
            if (Mode == Mode.Pause) {
                UnpauseCount();
            }
            else {
                if (_countOfWorks < 3) {
                    _nextAction = StartBreak;
                }
                else {
                    _nextAction = StartLongBreak;
                    _countOfWorks = 0;
                }
                _countOfWorks++;
                StopCount();
                if (Debugger.IsAttached) {
                    StartCount(TimeSpan.FromSeconds(25));
                }
                else {
                    StartCount(TimeSpan.FromMinutes(25));
                }
            }
            Mode = Mode.Work;
        }

        int _countOfWorks;

        public Action<bool> MaximizeRequest;

        void PauseWork() {
            PauseCount();
            Mode = Mode.Pause;
        }

        void StartBreak() {
            StopCount();
            MaximizeRequest?.Invoke(true);
            _nextAction = null;
            if (Debugger.IsAttached) {
                StartCount(TimeSpan.FromSeconds(5));
            }
            else {
                StartCount(TimeSpan.FromMinutes(5));
            }
            Mode = Mode.Break;
        }

        void StartLongBreak() {
            StopCount();
            MaximizeRequest?.Invoke(true);
            _nextAction = null;
            if (Debugger.IsAttached) {
                StartCount(TimeSpan.FromSeconds(20));
            }
            else {
                StartCount(TimeSpan.FromMinutes(20));
            }
            Mode = Mode.LongBreak;
        }

        void Stop() {
            _nextAction = null;
            StopCount();
            Mode = Mode.Inactive;
            MaximizeRequest?.Invoke(false);
        }

        public double Progress => (double)(_wholeTime.Ticks - Time.Ticks) / _wholeTime.Ticks;

        TimeSpan _wholeTime;
        DateTime _currentCountStartTime;
        TimeSpan _countAtStartTime;
        volatile bool _exit;
        readonly object _lock = new object();
        void StartCount(TimeSpan time) {
            lock (_lock) {
                _currentCountStartTime = DateTime.Now;
                _countAtStartTime = time;
                _wholeTime = time;
                Time = time;
                _exit = false;
            }
            Counting();
        }

        Action _nextAction;

        void Counting() {
            new Action(() => {
                while (!_exit) {
                    Thread.Sleep(200);
                    TimeSpan tmp;
                    lock (_lock) {
                        tmp = _countAtStartTime - (DateTime.Now - _currentCountStartTime);
                    }
                    Application.Current.Dispatcher.Invoke(() => {
                        if (tmp > TimeSpan.FromSeconds(1)) {
                            Time = tmp;
                        }
                        else {
                            Time = TimeSpan.Zero;
                            MaximizeRequest?.Invoke(false);
                            _nextAction?.Invoke();
                        }
                    });
                    if (tmp < TimeSpan.FromSeconds(1)) {
                        break;
                    }
                }
            }).BeginInvoke(null, null);
        }

        void PauseCount() {
            lock (_lock) {
                _exit = true;
            }
        }

        void UnpauseCount() {
            lock (_lock) {
                _currentCountStartTime = DateTime.Now;
                _countAtStartTime = Time;
                _exit = false;
            }
            Counting();
        }

        void StopCount() {
            lock (_lock) {
                _exit = true;
                _countAtStartTime = TimeSpan.Zero;
                Time = TimeSpan.Zero;
                _wholeTime = TimeSpan.Zero;
            }
        }

        public Action RestoreRequest;
        public Action MinimizeRequest;

        bool _isVisible;
        public bool IsVisible {
            get => _isVisible;
            set {
                _isVisible = value;
                if (value) {
                    RestoreRequest?.Invoke();
                }
                else {
                    MinimizeRequest?.Invoke();
                }
            }
        }

        double _top;
        public double Top {
            get => _top;
            set {
                if (Math.Abs(_top - value) < 1) {
                    return;
                }
                _top = value;
                NotifyPropertyChanged();
            }
        }

        double _left;
        public double Left {
            get => _left;
            set {
                if (Math.Abs(_left - value) < 1) {
                    return;
                }
                _left = value;
                NotifyPropertyChanged();
            }
        }

        double _height;
        public double Height {
            get => _height;
            set {
                if (Math.Abs(_height - value) < 1) {
                    return;
                }
                if (value < 200) {
                    return;
                }
                _height = value;
                NotifyPropertyChanged();
            }
        }

        double _width;

        public double Width {
            get => _width;
            set {
                if (Math.Abs(_width - value) < 1) {
                    return;
                }
                if (value < 200) {
                    return;
                }
                _width = value;
                NotifyPropertyChanged();
            }
        }
    }

    enum Mode {
        Inactive,
        Work,
        Pause,
        Break,
        LongBreak,
    }
}