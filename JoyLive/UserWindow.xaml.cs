﻿using System;
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
        private bool isStopped;
        private readonly int maxRetry = 100;
        private const string button_dump = "Dump Stream";
        private const string button_stop = "Stop Process";

        public UserWindow(User user)
        {
            InitializeComponent();

            if (user == null)
            {
                Title = "JoyLive — User Finder and Recorder";

                labelLiveSince.Text = "Birthday :";
                LockButton(true);

                SetStatus("Ready to find user...");

                return;
            }

            cardFind.Visibility = Visibility.Collapsed;
            Height = 250;

            this.user = user;
            LoadUserInfo(user);

            SetStatus("Let's start the party...");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var paste = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(paste))
                return;
            if (!paste.StartsWith("rtmp") && !paste.StartsWith("http"))
                return;

            textInput.Text = paste;
        }

        private void LoadUserInfo()
        {
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

            Title = $"{userInfo.nickname} — {userInfo.signature}";
            imageProfile.Source = new BitmapImage(new Uri(userInfo.headPic));
            textId.Text = userInfo.id;
            textPrice.Text = $"{userInfo.price} 💰";
            textNickname.Text = userInfo.nickname;
            textAnnouncement.Text = userInfo.signature;
            textFans.Text = userInfo.fansNum;
            textLiveSince.Text = userInfo.birthday;
        }

        private void LockButton(bool state)
        {
            if (state) LoadUserInfo();

            buttonCopy.IsEnabled = !state;
            buttonPlay.IsEnabled = !state;
            buttonDump.IsEnabled = !state;
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            Window_Loaded(sender, e);
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

            LockButton(true);
            buttonPaste.IsEnabled = false;
            buttonFind.IsEnabled = false;

            var api = new JoyLiveApi();
            var userInfo = await api.GetUser(id);

            buttonPaste.IsEnabled = true;
            buttonFind.IsEnabled = true;

            if (api.isError)
            {
                SetStatus(api.errorMessage);
                return;
            }

            // parse userInfo to user
            user = new User
            {
                mid = userInfo.id,
                nickname = userInfo.nickname,
                announcement = userInfo.signature,
                videoPlayUrl = userInfo.videoPlayUrl
            };

            LoadUserInfo(userInfo);
            LockButton(false);

            SetStatus("Let's start the party");
        }

        private async void ButtonDump_Click(object sender, RoutedEventArgs e)
        {
            if (process != null)
            {
                isStopped = true;
                process.Kill();
                return;
            }

            buttonDump.Content = button_dump;

            isStopped = false;
            int counter = 0;
            while(true)
            {
                await DumpStream();
                if (isStopped || (checkboxForever.IsChecked == false) || (counter > maxRetry))
                    break;

                await Task.Delay(30000);
                counter++;
            }

            buttonDump.Content = button_stop;
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

        private async Task DumpStream()
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
            SetStatus("Opening stream with default player");
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{user.nickname} — {user.announcement}\n\n▶️ LiveShow » {user.videoPlayUrl.ToPlaylist()}");
            SetStatus("Link is copied into clipboard");
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
    }
}