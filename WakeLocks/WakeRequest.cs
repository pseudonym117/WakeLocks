
namespace WakeLocks
{
    class WakeRequest
    {
        public string FriendlyName { get; set; }

        public string RequesterType { get; set; }

        public string Executable { get; set; }

        public string FullName => $"{this.FriendlyName}: {this.RequesterType} {this.Executable}";

        public override string ToString() => this.FullName;
    }
}
