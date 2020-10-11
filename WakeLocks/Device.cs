using System.Collections.Generic;

namespace WakeLocks
{
    class Device
    {
        public string DeviceName { get; set; }

        public IList<WakeRequest> WakeRequests { get; set; }

        public override string ToString() => $"{this.DeviceName}({this.WakeRequests?.Count ?? 0}";
    }
}
