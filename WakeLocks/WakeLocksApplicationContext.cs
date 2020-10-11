using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WakeLocks.Properties;

namespace WakeLocks
{
    class WakeLocksApplicationContext : ApplicationContext
    {
        private readonly PowerCfgParser _parser;
        private readonly ContextMenuStrip _contextMenu;
        private readonly NotifyIcon _icon;
        private readonly Timer _timer;

        public WakeLocksApplicationContext()
        {
            this._parser = new PowerCfgParser();
            this._contextMenu = new ContextMenuStrip();
            this._icon = new NotifyIcon
            {
                ContextMenuStrip = this._contextMenu,
                Icon = Resources.Unlocked,
                Text = "WakeLocks",
                Visible = true,
            };
            this._icon.MouseClick += OnIconClick;

            this._timer = new Timer
            {
                Interval = 5000,
                Enabled = true,
            };
            this._timer.Tick += this.OnRefreshMenu;

            this.BuildAndSetMenu();
        }

        private void OnIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this._contextMenu.Visible)
                {
                    this._contextMenu.Hide();
                }
                else
                {
                    this._contextMenu.Show(Cursor.Position);
                }
            }
        }

        private void OnRefreshMenu(object sender, System.EventArgs e) => this.BuildAndSetMenu();

        private void BuildAndSetMenu()
        {
            Debugger.Log(0, null, "updating menu\n");

            var devices = this._parser.GetPowerRequests();
            var isLocked = devices.Any(device => device.WakeRequests.Any());

            this._icon.Icon = isLocked ? Resources.Locked : Resources.Unlocked;

            this._contextMenu.AutoSize = false;
            this._contextMenu.Items.Clear();
            this._contextMenu.Items.AddRange(this.BuildMenu(devices).ToArray());
            this._contextMenu.AutoSize = true;
        }

        private IEnumerable<ToolStripItem> BuildMenu(IList<Device> devices)
        {
            yield return new ToolStripLabel("Current Locks:");
            
            foreach (var device in devices)
            {
                yield return new ToolStripSeparator();
                yield return new ToolStripLabel(device.DeviceName);

                foreach (var request in device.WakeRequests)
                {
                    yield return new ToolStripButton($" - {request.FriendlyName}", null, (sender, e) => request.Kill());
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._timer.Enabled = false;
            this._icon.Visible = false;
            
            if (disposing)
            {
                this._timer.Tick -= this.OnRefreshMenu;

                this._icon.Dispose();
                this._contextMenu.Dispose();
                this._timer.Dispose();
            }
        }
    }
}
