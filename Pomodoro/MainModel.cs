using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
//-----------------------------------------------------------------------------
namespace Pomodoro {
	class MainModel : INotifyPropertyChanged {
		TimeSpan _time;
		public TimeSpan Time {
			get {
				return _time;
			}
			set {
				_time = value;
				NotifyPropertyChanged(nameof(Time));
				NotifyPropertyChanged(nameof(Progress));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		//---------------------------------------------------------------------
		Mode _mode = Mode.Inactive;
		public Mode Mode {
			get {
				return _mode;
			}
			set {
				_mode = value;
				NotifyPropertyChanged(nameof(Mode));
				NotifyPropertyChanged(nameof(CanBreak));
				NotifyPropertyChanged(nameof(CanPause));
				NotifyPropertyChanged(nameof(CanStart));
				NotifyPropertyChanged(nameof(CanStop));
			}
		}
		//---------------------------------------------------------------------
		public void StartWork() {
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
		//---------------------------------------------------------------------
		int _countOfWorks;
		//---------------------------------------------------------------------
		public Action<bool> MaximizeRequest;
		//---------------------------------------------------------------------
		public void PauseWork() {
			PauseCount();
			Mode = Mode.Pause;
		}
		//---------------------------------------------------------------------
		public void StartBreak() {
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
		//---------------------------------------------------------------------
		public void StartLongBreak() {
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
		//---------------------------------------------------------------------
		public void Stop() {
			_nextAction = null;
			StopCount();
			Mode = Mode.Inactive;
			MaximizeRequest?.Invoke(false);
		}
		//---------------------------------------------------------------------
		public bool CanStart {
			get {
				return Mode != Mode.Work;
			}
		}
		//---------------------------------------------------------------------
		public bool CanPause {
			get {
				return Mode == Mode.Work;
			}
		}
		//---------------------------------------------------------------------
		public bool CanStop {
			get {
				return Mode != Mode.Inactive;
			}
		}
		//---------------------------------------------------------------------
		public bool CanBreak {
			get {
				return Mode == Mode.Work || Mode == Mode.Pause;
			}
		}
		//---------------------------------------------------------------------
		public double Progress {
			get {
				return (double) (_wholeTime.Ticks - Time.Ticks) / _wholeTime.Ticks;
			}
		}
		//---------------------------------------------------------------------
		TimeSpan _wholeTime;
		DateTime _currentCountStartTime;
		TimeSpan _countAtStartTime;
		volatile bool _exit;
		object _lock = new object();
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
		//---------------------------------------------------------------------
		Action _nextAction;
		//---------------------------------------------------------------------
		void Counting() {
			new Action(() => {
				while (!_exit) {
					Thread.Sleep(200);
					TimeSpan tmp;
					lock (_lock) {
						tmp = _countAtStartTime - (DateTime.Now - _currentCountStartTime);
					}
					Application.Current.Dispatcher.Invoke(new Action(() => {
						if (tmp > TimeSpan.FromSeconds(1)) {
							Time = tmp;
						}
						else {
							Time = TimeSpan.Zero;
							MaximizeRequest?.Invoke(false);
							_nextAction?.Invoke();
						}
					}));
					if (tmp < TimeSpan.FromSeconds(1)) {
						break;
					}
				}
			}).BeginInvoke(null, null);
		}
		//---------------------------------------------------------------------
		void PauseCount() {
			lock (_lock) {
				_exit = true;
			}
		}
		//---------------------------------------------------------------------
		void UnpauseCount() {
			lock (_lock) {
				_currentCountStartTime = DateTime.Now;
				_countAtStartTime = Time;
				_exit = false;
			}
			Counting();
		}
		//---------------------------------------------------------------------
		void StopCount() {
			lock (_lock) {
				_exit = true;
				_countAtStartTime = TimeSpan.Zero;
				Time = TimeSpan.Zero;
				_wholeTime = TimeSpan.Zero;
			}
		}
		//---------------------------------------------------------------------
		public Action RestoreRequest;
		public Action MinimizeRequest;
		//---------------------------------------------------------------------
		bool _isVisible;
		public bool IsVisible {
			get {
				return _isVisible;
			}
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
		//---------------------------------------------------------------------
		double _top;
		public double Top {
			get {
				return _top;
			}
			set {
				if (Math.Abs(_top - value) < 1) {
					return;
				}
				_top = value;
				NotifyPropertyChanged(nameof(Top));
			}
		}
		//---------------------------------------------------------------------
		double _left;
		public double Left {
			get {
				return _left;
			}
			set {
				if (Math.Abs(_left - value) < 1) {
					return;
				}
				_left = value;
				NotifyPropertyChanged(nameof(Left));
			}
		}
		//---------------------------------------------------------------------
		double _height;
		public double Height {
			get {
				return _height;
			}
			set {
				if (Math.Abs(_height - value) < 1) {
					return;
				}
				if (value < 200) {
					return;
				}
				_height = value;
				NotifyPropertyChanged(nameof(Height));
			}
		}
		//---------------------------------------------------------------------
		double _width;
		public double Width {
			get {
				return _width;
			}
			set {
				if (Math.Abs(_width - value) < 1) {
					return;
				}
				if (value < 200) {
					return;
				}
				_width = value;
				NotifyPropertyChanged(nameof(Width));
			}
		}
	}
	//-------------------------------------------------------------------------
	enum Mode {
		Inactive,
		Work,
		Pause,
		Break,
		LongBreak,
	}
}