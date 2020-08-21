using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows;

namespace Pomodoro {
    class OneProgramInstance {
        public static readonly OneProgramInstance Instance;

        static OneProgramInstance() {
            PipeName = "Pomodoro_Pipe"
                + (Debugger.IsAttached ? ".debug" : "");
            Instance = new OneProgramInstance();
        }

        public bool ProgramHasAnotherInstance {
            get {
                if (HasAnotherProgramInstance) {
                    try {
                        NamedPipeClientStream pipeClient
                            = new NamedPipeClientStream(PipeName);
                        pipeClient.Connect(10000);
                        StreamString ss = new StreamString(pipeClient);
                        ss.WriteString(PipeShowWindowCommand);
                        pipeClient.Close();
                    }
                    catch (Exception ex) {
                    }
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        static bool HasAnotherProgramInstance {
            get {
                Process thisProc = Process.GetCurrentProcess();
                string thisProcName = thisProc.ProcessName.Replace(".vshost", "");
                return Process.GetProcesses()
                    .Any(p => p.ProcessName == thisProcName && p.Id != thisProc.Id);
            }
        }

        static readonly string PipeName;
        const string PipeShowWindowCommand = "Show window";
        const string PipeExitCommand = "Exit";
        //---------------------------------------------------------------------
        public void StartPipeServer(MainWindow window) {
            new Action(() => ServerThread(window)).BeginInvoke(null, null);
        }
        //---------------------------------------------------------------------
        void ServerThread(MainWindow window) {
            while (true) {
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
                pipeServer.WaitForConnection();
                string argument = null;
                try {
                    StreamString ss = new StreamString(pipeServer);
                    argument = ss.ReadString();
                }
                catch { }
                finally {
                    pipeServer.Close();
                }
                switch (argument) {
                    case PipeShowWindowCommand:
                        window.Dispatcher.BeginInvoke(new Action(window.Restore));
                        break;
                    case PipeExitCommand:
                        try {
                            Application.Current.Shutdown();
                        }
                        catch {
                            Environment.Exit(0);
                        }
                        break;
                }
            }
        }

        class StreamString {
            readonly Stream _ioStream;

            public StreamString(Stream ioStream) {
                _ioStream = ioStream;
            }

            public string ReadString() {
                int len = _ioStream.ReadByte() * 256;
                len += _ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                _ioStream.Read(inBuffer, 0, len);
                return new UnicodeEncoding().GetString(inBuffer);
            }

            public int WriteString(string outString) {
                byte[] outBuffer = new UnicodeEncoding().GetBytes(outString);
                int len = outBuffer.Length;
                _ioStream.WriteByte((byte)(len / 256));
                _ioStream.WriteByte((byte)(len & 255));
                _ioStream.Write(outBuffer, 0, len);
                _ioStream.Flush();
                return outBuffer.Length + 2;
            }
        }
    }
}