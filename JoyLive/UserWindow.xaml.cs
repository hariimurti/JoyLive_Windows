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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private User user;
        private Process process;
        private bool isRecording;

        public UserWindow(User user)
        {
            InitializeComponent();

            radioCustomTimeout.Content = $"{App.RetryTimeout}m";

            if (user == null)
            {
                LockButtonAndReset(true);

                SetStatus("Ready to find user...");
                return;
            }

            cardFind.Visibility = Visibility.Collapsed;

            this.user = user;
            LoadUserInfo(user);

            SetStatus("Let's start the party...");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PasteLinkOrId();
        }

        private bool PasteLinkOrId()
        {
            var paste = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(paste))
                return false;

            var link = Regex.IsMatch(paste, "(?:https?|rtmp)://.*/live/.*");
            var number = Regex.IsMatch(paste, @"^(\d+)$");
            if (link || number)
            {
                textInput.Text = paste;
                return true;
            }

            return false;
        }

        private void LabelFindMode(bool findMode = true)
        {
            labelLiveSince.Text = findMode ? "Birthday :" : "Live Since :";
            labelViewer.Text = findMode ? "LiveShow :" : "Viewer :";
            textViewer.Text = findMode ? "Unknown" : "0";
        }

        private void LoadUserInfo()
        {
            LabelFindMode();

            Title = "JoyLive — User Finder and Recorder";
            imageProfile.Source = null;
            textId.Text = "0";
            textPrice.Text = "0 💰";
            textNickname.Text = ". . . .";
            textAnnouncement.Text = ". . . .";
            textFans.Text = "0";
            textLiveSince.Text = "0000-00-00";
        }

        private void LoadUserInfo(User user)
        {
            if (string.IsNullOrWhiteSpace(user.announcement))
                user.announcement = "Hey, come and check my show now!";

            LabelFindMode(false);

            Title = $"{user.nickname} — {user.announcement}";
            imageProfile.Source = new BitmapImage(new Uri(user.headPic));
            textId.Text = user.mid;
            textPrice.Text = $"{user.price} 💰";
            textNickname.Text = user.nickname;
            textAnnouncement.Text = user.announcement;
            textLiveSince.Text = user.playStartTime.ToHumanReadableFormat();
            textViewer.Text = user.onlineNum.ToString();
            textFans.Text = user.fansNum;
        }

        private void LoadUserInfo(UserInfo userInfo)
        {
            if (string.IsNullOrWhiteSpace(userInfo.signature))
                userInfo.signature = "Hey, come and check my show now!";

            LabelFindMode();

            Title = $"{userInfo.nickname} — {userInfo.signature}";
            imageProfile.Source = new BitmapImage(new Uri(userInfo.headPic));
            textId.Text = userInfo.id;
            textPrice.Text = $"{userInfo.price} 💰";
            textNickname.Text = userInfo.nickname;
            textAnnouncement.Text = userInfo.signature;
            textFans.Text = userInfo.fansNum;
            textLiveSince.Text = userInfo.birthday;
            textViewer.Text = "Unknown";
        }

        private void LockButtonAndReset(bool state)
        {
            if (state) LoadUserInfo();

            buttonCopy.IsEnabled = !state;
            buttonPlay.IsEnabled = !state;
            buttonDump.IsEnabled = !state;
            buttonStop.IsEnabled = !state;
        }

        private void LockInput(bool state)
        {
            textInput.IsReadOnly = state;
            buttonPaste.IsEnabled = !state;
            buttonFind.IsEnabled = !state;
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            var result = PasteLinkOrId();
            if (!result) return;

            ButtonFind_Click(sender, e);
        }

        private async void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            var text = textInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                SetStatus("Input isn't valid!!!");
                return;
            }

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

            LockInput(true);
            LockButtonAndReset(true);

            SetStatus($"Get userid: {id}");

            var api = new JoyLiveApi();
            var userInfo = await api.GetUser(id);

            if (api.isError)
            {
                SetStatus(api.errorMessage);
                return;
            }

            // parse userInfo to user
            user = new User
            {
                rid = userInfo.rid,
                mid = userInfo.id,
                nickname = userInfo.nickname,
                announcement = userInfo.signature,
                videoPlayUrl = userInfo.videoPlayUrl
            };

            LoadUserInfo(userInfo);

            if (App.UseAccount)
            {
                var room = await api.GetRoomInfo(user.rid);
                if (!api.isError)
                {
                    textViewer.Text = room.isPlaying ? "Online" : "Offline";
                    if (room.isPlaying) cardFind.Visibility = Visibility.Collapsed;
                }
            }

            LockInput(false);
            LockButtonAndReset(false);

            SetStatus("Let's start the party");
        }

        private async void ButtonDump_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording) return;

            if (string.IsNullOrWhiteSpace(user.videoPlayUrl))
            {
                SetStatus("[Error] Can't find the stream link");
                return;
            }

            LockInput(true);
            cardFind.Visibility = Visibility.Collapsed;
            buttonDump.Visibility = Visibility.Collapsed;
            buttonStop.Visibility = Visibility.Visible;

            var api = new JoyLiveApi();

            isRecording = true;
            var firsttime = true;
            var timestart = DateTime.Now;
            while (isRecording)
            {
                var dump = false;
                if (App.UseAccount)
                {
                    var isPlaying = false;

                    if (!firsttime)
                    {
                        SetStatus("Checking room...");
                        var room = await api.GetRoomInfo(user.rid);
                        if (!api.isError)
                        {
                            isPlaying = room.isPlaying;
                            textViewer.Text = room.isPlaying ? "Online" : "Offline";
                            SetStatus(room.isPlaying ? "User Online" : "User Offline");
                        }
                        else
                        {
                            textViewer.Text = "Unknown";
                            SetStatus(api.errorMessage);
                        }
                        labelViewer.Text = "LiveShow :";
                    }

                    if (isPlaying || firsttime)
                        dump = await DumpStream();

                    firsttime = false;
                }
                else
                {
                    dump = await DumpStream();
                }

                if (dump) timestart = DateTime.Now;

                if (!isRecording) break;
                if (radioManual.IsChecked == false)
                {
                    if (radioImmediately.IsChecked == true) break;
                    if (radioCustomTimeout.IsChecked == true)
                    {
                        if (IsTimeout(timestart))
                            break;

                        var to = Timeout(timestart);
                        SetStatus($"Timeout in {to.Minutes.ToString("00")}:{to.Seconds.ToString("00")}");
                    }
                }
                else
                {
                    SetStatus("Please wait... Retry in 10s");
                }

                await Task.Delay(10000);
            }

            isRecording = false;
            LockInput(false);
            buttonDump.Visibility = Visibility.Visible;
            buttonStop.Visibility = Visibility.Collapsed;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            isRecording = false;
            if (process != null) process.Kill();

            buttonDump.Visibility = Visibility.Visible;
            buttonStop.Visibility = Visibility.Collapsed;
        }

        private TimeSpan Timeout(DateTime timestart)
        {
            var runtime = DateTime.Now - timestart;
            var current = TimeSpan.FromSeconds(runtime.TotalSeconds);
            return TimeSpan.FromMinutes(App.RetryTimeout) - current;
        }

        private bool IsTimeout(DateTime timestart)
        {
            var runtime = DateTime.Now - timestart;
            var current = TimeSpan.FromSeconds(runtime.TotalSeconds);
            var timeout = TimeSpan.FromMinutes(App.RetryTimeout);
            return (current > timeout);
        }

        private ProcessStartInfo RtmpDump(string url, string filepath)
        {
            return new ProcessStartInfo
            {
                FileName = "rtmpdump.exe",
                Arguments = $"−−live −r {url} -o \"{filepath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        private async Task<bool> DumpStream()
        {
            var timenow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{timenow}_{user.mid}.flv";
            var filepath = Path.Combine(App.OutputDir, filename);

            var rtmpdump = RtmpDump(user.videoPlayUrl, filepath);
            process = Process.Start(rtmpdump);
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
                    SetStatus("Live stream has ended");
                    break;

                default:
                    SetStatus("Network problem!");
                    break;
            }

            process?.Close();
            process?.Dispose();
            process = null;

            var result = false;
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
                    result = true;
                }
            }
            catch (Exception) { }
            return result;
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(user.videoPlayUrl))
            {
                SetStatus("[Error] Can't find the stream link");
                return;
            }

            buttonPlay.IsEnabled = false;
            Process.Start(user.videoPlayUrl);
            SetStatus("Play stream with default player");
            buttonPlay.IsEnabled = true;
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(user.videoPlayUrl))
            {
                SetStatus("[Error] Can't find the stream link");
                return;
            }

            buttonCopy.IsEnabled = false;
            Clipboard.SetText($"{user.nickname} — {user.announcement}\n\n▶️ LiveShow » {user.videoPlayUrl.ToPlaylist()}");
            SetStatus("Copied into clipboard");
            buttonCopy.IsEnabled = true;
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
                var time = DateTime.Now.ToString("HH:mm:ss");
                Dispatcher.Invoke(() => textStatus.Text = $"{time} » {text}");
            }
            catch (Exception) { }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F12)
            {
                if (isRecording) return;

                PasteLinkOrId();

                var isCollapsed = (cardFind.Visibility == Visibility.Collapsed);
                cardFind.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}