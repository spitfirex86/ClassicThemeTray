using ClassicThemeTray.Properties;
using NtApiDotNet;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ClassicThemeTray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Tray = new NotifyIcon { Visible = true };
            Tray.MouseClick += TrayOnMouseClick;

            ClassicEnabled = GetSectionSecurity() == DenyAccessSddl;

            Application.Run();
        }

        private static NotifyIcon Tray { get; set; }

        private static bool _classicEnabled;
        private static bool ClassicEnabled
        {
            get => _classicEnabled;
            set
            {
                if (value)
                {
                    Tray.Text = DisableText + ExitText;
                    Tray.Icon = Resources.classic;
                }
                else
                {
                    Tray.Text = EnableText + ExitText;
                    Tray.Icon = Resources.modern;
                }
                _classicEnabled = value;
            }
        }

        private const string EnableText = "Left click to enable Classic Theme";
        private const string DisableText = "Left click to disable Classic Theme";
        private const string ExitText = "\nRight click to exit";

        private const string AllowAccessSddl = "O:BAG:SYD:(A;;CCLCRC;;;IU)(A;;CCDCLCSWRPSDRCWDWO;;;SY)";
        private const string DenyAccessSddl = "O:BAG:SYD:(A;;RC;;;IU)(A;;DCSWRPSDRCWDWO;;;SY)";

        private static void ToggleClassicTheme()
        {
            if (ClassicEnabled)
            {
                // Disable Classic
                SetSectionSecurity(AllowAccessSddl);
                ClassicEnabled = false;
            }
            else
            {
                // Enable Classic
                SetSectionSecurity(DenyAccessSddl);
                ClassicEnabled = true;
            }
        }

        private static void SetSectionSecurity(string sddl)
        {
            using (NtObject ntObject = NtObject.OpenWithType("Section",
                $@"\Sessions\{Process.GetCurrentProcess().SessionId}\Windows\ThemeSection",
                null, GenericAccessRights.WriteDac))
                ntObject.SetSecurityDescriptor(new SecurityDescriptor(sddl), SecurityInformation.Dacl);
        }

        private static string GetSectionSecurity()
        {
            using (NtObject ntObject = NtObject.OpenWithType("Section",
                $@"\Sessions\{Process.GetCurrentProcess().SessionId}\Windows\ThemeSection",
                null, GenericAccessRights.ReadControl))
                return ntObject.GetSddl();
        }

        private static void TrayOnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ToggleClassicTheme();
            else if (e.Button == MouseButtons.Right)
                Environment.Exit(0);
        }
    }
}
