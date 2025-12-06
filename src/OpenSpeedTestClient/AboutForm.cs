using System.Diagnostics;

namespace OpenSpeedTestClient;

public class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "About OpenSpeedTest Client";
        this.StartPosition = FormStartPosition.CenterParent;
        this.ClientSize = new Size(420, 160);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var appName = new Label
        {
            Text = "OpenSpeedTest Client",
            Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 16)
        };

        var version = new Label
        {
            Text = "Build: 22/11/2025",
            AutoSize = true,
            Location = new Point(16, 52)
        };

        var attributionText = "Innovation icons created by Pixel perfect - Flaticon";
        var attributionLink = new LinkLabel
        {
            Text = attributionText,
            AutoSize = true,
            Location = new Point(16, 80),
            LinkBehavior = LinkBehavior.HoverUnderline
        };
        attributionLink.Links.Clear();
        attributionLink.Links.Add(0, attributionText.Length, "https://www.flaticon.com/free-icons/innovation");
        attributionLink.LinkClicked += (s, e) =>
        {
            var url = e.Link?.LinkData as string;
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        };

        var closeBtn = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Size = new Size(90, 30),
            Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 45)
        };

        this.Controls.Add(appName);
        this.Controls.Add(version);
        this.Controls.Add(attributionLink);
        this.Controls.Add(closeBtn);
    }
}
