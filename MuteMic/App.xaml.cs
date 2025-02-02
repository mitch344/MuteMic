using System.Windows;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using System.Drawing;
using Application = System.Windows.Application;
using System.IO;

namespace MuteMic
{
    public partial class App : Application
    {
        private static Mutex mutex = new Mutex(true, "{F6A4473B-F2D5-4F61-B55C-96BB8AC2A016}");
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem muteMenuItem;

        private bool isMuted = false;
        private MMDeviceEnumerator enumerator;
        private MMDevice microphone;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                System.Windows.MessageBox.Show("An instance of this application is already running in your system tray.",
                                                 "Instance Already Running",
                                                 MessageBoxButton.OK,
                                                 MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }

            InitializeTrayIcon();
            InitializeMicrophone();
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();

            muteMenuItem = new ToolStripMenuItem("Mute Microphone")
            {
                CheckOnClick = true
            };
            muteMenuItem.Checked = isMuted;
            muteMenuItem.Click += ToggleMute;

            trayMenu.Items.Add(muteMenuItem);
            trayMenu.Items.Add("Exit", null, ExitApp);

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mic.ico");
            trayIcon = new NotifyIcon
            {
                Icon = new Icon(iconPath),
                ContextMenuStrip = trayMenu,
                Visible = true,
                Text = "MuteMic"
            };
        }

        private void InitializeMicrophone()
        {
            enumerator = new MMDeviceEnumerator();
            microphone = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            isMuted = microphone.AudioEndpointVolume.Mute;
            muteMenuItem.Checked = isMuted;
        }

        private void ToggleMute(object sender, EventArgs e)
        {
            isMuted = !isMuted;
            muteMenuItem.Checked = isMuted;
            microphone.AudioEndpointVolume.Mute = isMuted;
        }

        private void ExitApp(object sender, EventArgs e)
        {
            if (isMuted)
            {
                var result = System.Windows.MessageBox.Show("Your microphone is muted. Would you like to unmute it before exiting?",
                                                             "Unmute Microphone",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    isMuted = false;
                    muteMenuItem.Checked = false;
                    microphone.AudioEndpointVolume.Mute = false;
                }
            }

            trayIcon.Visible = false;
            Application.Current.Shutdown();
        }
    }
}
