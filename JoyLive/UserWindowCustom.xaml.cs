using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for AdvancedWindow.xaml
    /// </summary>
    public partial class UserWindowCustom : Window
    {
        private UserInfo user;
        private Process process;

        public UserWindowCustom()
        {
            InitializeComponent();

            LockResource(true);

            var paste = Clipboard.GetText();
            if (paste.StartsWith("rtmp") || paste.StartsWith("http"))
            {
                textInput.Text = paste;
            }
        }

        private void LockResource(bool state)
        {
            if (state)
            {
                Title = "JoyLive — User Finder and Recorder";
                imageProfile.Source = null;
                textId.Text = "0";
                textPrice.Text = "0 💰";
                textNickname.Text = ". . . .";
                textAnnouncement.Text = ". . . .";
                textFans.Text = "0";
                textBirthday.Text = "0000-00-00";
            }

            buttonCopy.IsEnabled = !state;
            buttonPlay.IsEnabled = !state;
            buttonDump.IsEnabled = !state;
        }

        private async void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            var text = textInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                SetStatus("Input isn't valid!!!");
                return;
            }

            Console.WriteLine("Input : " + text);

            var id = text;
            if (text.StartsWith("rtmp") || text.StartsWith("http"))
            {
                Match regex = Regex.Match(text, @"live/(\d+)/?");
                if (regex.Success)
                {
                    id = regex.Groups[1].Value;
                }
            }

            Console.WriteLine("ID : " + id);
            if (Regex.IsMatch(id, @"^\d$"))
            {
                SetStatus("Input isn't valid!!!");
                return;
            }

            LockResource(true);
            buttonFind.IsEnabled = false;

            var api = new JoyLiveApi();
            user = await api.GetUser(id);

            buttonFind.IsEnabled = true;

            if (api.isError)
            {
                SetStatus(api.errorMessage);
                return;
            }
            
            Title = $"{user.nickname} — {user.signature}";
            imageProfile.Source = new BitmapImage(new Uri(user.headPic));
            textId.Text = user.id;
            textPrice.Text = $"{user.price} 💰";
            textNickname.Text = user.nickname;
            textAnnouncement.Text = user.signature;
            textFans.Text = user.fansNum;
            textBirthday.Text = user.birthday;

            LockResource(false);

            SetStatus("Let's start the party...");
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