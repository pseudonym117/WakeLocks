using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace WakeLocks
{
    class PowerCfgParser
    {
        private static Regex requesterRegex = new Regex(@"\[([A-Z]+)\] (.*)", RegexOptions.Compiled);

        public IList<Device> GetPowerRequests()
        {
            var startInfo = new ProcessStartInfo("powercfg.exe", "/requests")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas"
            };
            using var powercfgRequests = Process.Start(startInfo);

            powercfgRequests.WaitForExit();

            if (powercfgRequests.ExitCode != 0)
            {
                var err = powercfgRequests.StandardError.ReadToEnd();
                Debugger.Log(0, null, err);
                return new List<Device>();
            }

            return this.ReadDevicesFromStream(powercfgRequests.StandardOutput).ToList();
        }

        private IEnumerable<Device> ReadDevicesFromStream(StreamReader stream)
        {
            var deviceName = string.Empty;
            WakeRequest currentWakeRequest = null;
            var wakeLocks = new List<WakeRequest>();

            while (!stream.EndOfStream)
            {
                var currentLine = stream.ReadLine();

                if (string.IsNullOrEmpty(currentLine))
                {
                    if (currentWakeRequest != null)
                    {
                        wakeLocks.Add(currentWakeRequest);
                    }
                    if (!string.IsNullOrEmpty(deviceName))
                    {
                        yield return new Device { DeviceName = deviceName, WakeRequests = wakeLocks };
                    }
                    deviceName = string.Empty;
                    currentWakeRequest = null;
                    wakeLocks = new List<WakeRequest>();
                    continue;
                }

                if (string.IsNullOrEmpty(deviceName))
                {
                    deviceName = currentLine.Trim(' ', ':');
                    continue;
                }

                if (currentLine != "None.")
                {
                    if (currentWakeRequest != null && string.IsNullOrEmpty(currentWakeRequest.FriendlyName))
                    {
                        currentWakeRequest.FriendlyName = currentLine;
                        continue;
                    }
                    else if (currentWakeRequest != null)
                    {
                        wakeLocks.Add(currentWakeRequest);
                    }

                    var match = requesterRegex.Match(currentLine);
                    if (match.Success)
                    {
                        var requesterType = match.Groups[1].Value;
                        var executable = match.Groups[2].Value;
                        currentWakeRequest = new WakeRequest { RequesterType = requesterType, Executable = executable };
                    }
                    else
                    {
                        Debugger.Log(0, null, $"failed regex match on: {currentLine}");
                    }
                }
            }
        }
    }
}
