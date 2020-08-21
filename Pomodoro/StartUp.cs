using QuickConverter;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace Pomodoro {
    static class StartUp {
        [STAThread]
        public static void Main() {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

            if (!Debugger.IsAttached && OneProgramInstance.Instance.ProgramHasAnotherInstance) {
                return;
            }

            Application app = new Application();

            // Setup Quick Converter.
            // Add the System namespace so we can use primitive types (i.e. int, etc.).
            EquationTokenizer.AddNamespace(typeof(object));
            // Add the System.Windows namespace so we can use Visibility.Collapsed, etc.
            EquationTokenizer.AddNamespace(typeof(Visibility));
            EquationTokenizer.AddNamespace(typeof(TimeSpan));
            EquationTokenizer.AddNamespace(typeof(string));
            EquationTokenizer.AddNamespace(typeof(Mode));

            MainWindow mainWindow = new MainWindow();
            NotifyIconManager.Init(mainWindow.Model);
            OneProgramInstance.Instance.StartPipeServer(mainWindow);

            app.Run(mainWindow);
        }
    }
}