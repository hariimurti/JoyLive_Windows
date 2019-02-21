using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for AdvancedWindow.xaml
    /// </summary>
    public partial class AdvancedWindow : Window
    {
        private JoyUser user;
        private Process process;

        public AdvancedWindow(JoyUser user)
        {
            InitializeComponent();

            this.user = user;

            Title = "JoyLive : " + user.Nickname;

            imageProfile.Source = new BitmapImage(new Uri(user.ImageUrl));
            textId.Text = user.Id;
            textNickname.Text = user.Nickname;

            linkRtmp.NavigateUri = new Uri(user.Url);
            textRtmp.Text = user.Url;

            linkHttp.NavigateUri = new Uri(user.Url.ToPlaylistUrl());
            textHttp.Text = user.Url.ToPlaylistUrl();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (process != null)
            {
                process.Kill();
                return;
            }

            buttonDump.Content = "Stop Process";

            var timenow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{timenow}_{user.Id}.flv";
            var filepath = System.IO.Path.Combine(App.OutputDir, filename);

            ProcessStartInfo exec = new ProcessStartInfo
            {
                FileName = "rtmpdump.exe",
                Arguments = $"−−live −r {user.Url} -o \"{filepath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process = Process.Start(exec);
            process.OutputDataReceived += new DataReceivedEventHandler(DataReceivedHandler);
            process.BeginOutputReadLine();
            process.ErrorDataReceived += new DataReceivedEventHandler(DataReceivedHandler);
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            switch (process.ExitCode)
            {
                case -1:
                    SetStatus("Stopped by user!");
                    break;

                case 0:
                    SetStatus("Live stream has ended...");
                    break;

                default:
                    SetStatus("Network problem!");
                    break;
            }

            process?.Dispose();
            process?.Close();
            process = null;

            buttonDump.Content = "Dump Stream";

            try
            {
                var output = new FileInfo(filepath);
                if (output.Length < 2048)
                {
                    File.Delete(filepath);
                    SetStatus("Stream is offline!");
                }
                else
                {
                    SetStatus($"Saved as : {filename}");
                }
            }
            catch (Exception) { }
        }

        private void DataReceivedHandler(object s, DataReceivedEventArgs o)
        {
            if (string.IsNullOrWhiteSpace(o.Data)) return;
            SetStatus(o.Data);
        }

        private void SetStatus(string text)
        {
            try
            {
                Dispatcher.Invoke(() => textStatus.Text = text);
            }
            catch (Exception) { }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var link = e.Uri.ToString();
            if (link.StartsWith("rtmp"))
            {
                Process.Start(link);
                SetStatus("Opening stream with default player...");
            }
            else
            {
                Clipboard.SetText($"{user.Nickname}\n\n▶️ LiveShow » {link}");
                SetStatus("Link is copied into clipboard...");
            }
        }
    }
}