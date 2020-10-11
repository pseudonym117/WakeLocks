
using System.Diagnostics;
using System.IO;

namespace WakeLocks
{
    class WakeRequest
    {
        public string FriendlyName { get; set; }

        public string RequesterType { get; set; }

        public string Executable { get; set; }

        public string FullName => $"{this.FriendlyName}: {this.RequesterType} {this.Executable}";

        public override string ToString() => this.FullName;

        public bool Kill()
        {
            var exeName = Path.GetFileNameWithoutExtension(this.Executable);
            var toKill = Process.GetProcessesByName(exeName);

            Debugger.Log(0, null, $"Killing {toKill.Length} processes\n");

            bool successfulKill = true;
            foreach (var proc in toKill)
            {
                Debugger.Log(0, null, $"Closing process with pid {proc.Id} via closing main window");
                proc.CloseMainWindow();
            }

            foreach (var proc in toKill)
            {
                var exited = proc.WaitForExit(2000);
                if (!exited)
                {
                    Debugger.Log(0, null, $"Failed to close process with pid {proc.Id}; Killing...");
                    proc.Kill();
                    exited = proc.WaitForExit(1000);

                    if (!exited)
                    {
                        Debugger.Log(0, null, $"Failed to kill process with pid {proc.Id}");
                        successfulKill = false;
                    }
                }
            }

            return successfulKill;
        }
    }
}
