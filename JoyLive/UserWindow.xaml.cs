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
    public partial class UserWindow : Window
    {
        private User user;
        private Process process;

        public UserWindow(User user)
        {
            InitializeComponent();

            this.user = user;

            Title = $"{user.nickname} — {user.announcement}";

            imageProfile.Source = new BitmapImage(new Uri(user.headPic));
            textId.Text = user.mid;
            textNickname.Text = user.nickname;
            textAnnouncement.Text = user.announcement;
            textLiveSince.Text = user.playStartTime.ToHumanReadableFormat();
            textViewer.Text = user.onlineNum.ToString();//.ToHumanReadableFormat();
        }

        private async void ButtonDump_Click(object sender, RoutedEventArgs e)
        {
            if (process != null)
            {
                process.Kill();
                return;
            }

            buttonDump.Content = "Stop Process";

            var timenow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{timenow}_{user.mid}.flv";
            var filepath = Path.Combine(App.OutputDir, filename);

            ProcessStartInfo exec = new ProcessStartInfo
            {
                FileName = "rtmpdump.exe",
                Arguments = $"−−live −r {user.videoPlayUrl} -o \"{filepath}\"",
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

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(user.videoPlayUrl);
            SetStatus("Opening stream with default player...");
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{user.nickname} — {user.announcement}\n\n▶️ LiveShow » {user.videoPlayUrl.ToPlaylist()}");
            SetStatus("Link is copied into clipboard...");
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
    }
}