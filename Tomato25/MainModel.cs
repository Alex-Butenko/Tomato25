using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tomato25 {
    class MainModel : INotifyPropertyChanged {
        public MainModel() {
            StartCommand = new RelayCommand(_ => StartWork(), _ => !(Mode == Mode.Work && State == State.Running));
            PauseCommand = new RelayCommand(_ => PauseWork(), _ => Mode == Mode.Work && State == State.Running);
            BreakCommand = new RelayCommand(_ => StartShortBreak(), _ => Mode == Mode.Work);
            LongBreakCommand = new RelayCommand(_ => StartLongBreak(), _ => Mode == Mode.Work);
            StopCommand = new RelayCommand(_ => Stop(), _ => Mode != Mode.Inactive);
        }

        public ICommand StartCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand BreakCommand { get; }

        public ICommand LongBreakCommand { get; }

        public ICommand StopCommand { get; }

        TimeSpan GetDuration(double length) =>
            Debugger.IsAttached
                ? TimeSpan.FromSeconds(length)
                : TimeSpan.FromMinutes(length);


        void StartWork() {
            MaximizeRequest?.Invoke(false);
            if (Mode != Mode.Work) {
                ToDefaultState();
            }
            CancelLoop();
            Mode = Mode.Work;
            State = State.Running;

            BackgroundLoop(GetDuration(25), StartBreak);
        }

        void PauseWork() {
            CancelLoop();
            State = State.Stopped;
        }

        void StartShortBreak() {
            _countOfShortBreaks++;
            StartBreak(5);
        }

        void StartLongBreak() {
            _countOfShortBreaks = 0;
            StartBreak(20);
        }

        void StartBreak() {
            if (_countOfShortBreaks >= 3) {
                StartLongBreak();
            }
            else {
                StartShortBreak();
            }
        }

        void StartBreak(double length) {
            MaximizeRequest?.Invoke(true);
            ToDefaultState();
            Mode = Mode.Break;
            State = State.Running;
            BackgroundLoop(GetDuration(length));
        }

        void Stop() {
            ToDefaultState();
            MaximizeRequest?.Invoke(false);
        }

        void ToDefaultState() {
            CancelLoop();
            Mode = Mode.Inactive;
            State = State.Stopped;
            TimeLeft = TimeSpan.Zero;
            Progress = 0;
        }

        TimeSpan _timeLeft;
        public TimeSpan TimeLeft {
            get => _timeLeft;
            private set {
                _timeLeft = value > TimeSpan.Zero ? value : TimeSpan.Zero;
                NotifyPropertyChanged();
            }
        }

        double _progress;
        public double Progress {
            get => _progress;
            private set {
                _progress = value;
                NotifyPropertyChanged();
            }
        }

        Mode _mode = Mode.Inactive;
        public Mode Mode {
            get => _mode;
            set {
                if (_mode != value) {
                    _mode = value;
                    NotifyPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        State _state;
        State State {
            get => _state;
            set {
                if (_state != value) {
                    _state = value;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        void CancelLoop() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        DateTime _startedAt;
        CancellationTokenSource _cancellationTokenSource;
        void BackgroundLoop(TimeSpan limit, Action onFinished = null) {
            TimeSpan timeElapsed = TimeLeft == TimeSpan.Zero ? TimeSpan.Zero : limit - TimeLeft;
            CancellationToken token = _cancellationTokenSource.Token;
            TimeLeft = limit - timeElapsed;
            _startedAt = DateTime.Now - timeElapsed;
            Task.Factory.StartNew(
                async () => {
                    while (_state == State.Running) {
                        await Task.Delay(TimeSpan.FromMilliseconds(200), token);
                        if (token.IsCancellationRequested) return;
                        timeElapsed = DateTime.Now - _startedAt;
                        TimeLeft = limit - timeElapsed;
                        Progress = (double)timeElapsed.Ticks / limit.Ticks;
                        if (timeElapsed > limit) {
                            ToDefaultState();
                            onFinished?.Invoke();
                        }
                    }
                },
                token,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        int _countOfShortBreaks;

        public Action RestoreRequest;
        public Action MinimizeRequest;

        public Action<bool> MaximizeRequest;

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
    }
}