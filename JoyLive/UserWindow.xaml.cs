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
        private bool isRecording;

        public UserWindow(User user)
        {
            InitializeComponent();

            if (user == null)
            {
                Title = "JoyLive — User Finder and Recorder";

                labelLiveSince.Text = "Birthday :";
                labelViewer.Text = "LiveShow :";
                textViewer.Text = "Unknown";

                LockButton(true);

                SetStatus("Ready to find user...");

                return;
            }

            cardFind.Visibility = Visibility.Collapsed;
            Height = 250;

            this.user = user;
            LoadUserInfo(user);

            radioRetry.Content = $"{App.RetryTimeout}m";

            SetStatus("Let's start the party...");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PasteLink();
        }

        private bool PasteLink()
        {
            var paste = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(paste))
                return false;
            if (!paste.StartsWith("rtmp") && !paste.StartsWith("http"))
                return false;

            textInput.Text = paste;
            return true;
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
            textViewer.Text = "Unknown";
        }

        private void LockButton(bool state)
        {
            if (state) LoadUserInfo();

            buttonCopy.IsEnabled = !state;
            buttonPlay.IsEnabled = !state;
            buttonDump.IsEnabled = !state;
        }

        private void LockInput(bool state)
        {
            textInput.IsReadOnly = state;
            buttonPaste.IsEnabled = !state;
            buttonFind.IsEnabled = !state;
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            var result = PasteLink();
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
            LockButton(true);

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

            var room = await api.GetRoomInfo(user.rid);
            if (!api.isError)
                textViewer.Text = room.isPlaying ? "Online" : "Offline";

            LockInput(false);
            LockButton(false);

            SetStatus("Let's start the party");
        }

        private async void ButtonDump_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording) return;

            LockInput(true);
            buttonDump.Visibility = Visibility.Collapsed;
            buttonStop.Visibility = Visibility.Visible;

            var api = new JoyLiveApi();

            isRecording = true;
            var timestart = DateTime.Now;
            var counter = 0;
            while(true)
            {
                var dump = false;
                if (App.UseAccount)
                {
                    SetStatus("Checking room...");
                    var room = await api.GetRoomInfo(user.rid);
                    if (!api.isError)
                    {
                        textViewer.Text = room.onlineNum.ToString();
                        SetStatus(room.isPlaying ? "User Online" : "User Offline");
                        if (labelViewer.Text == "LiveShow :")
                        {
                            textViewer.Text = room.isPlaying ? "Online" : "Offline";
                        }
                        if (room.isPlaying)
                        {
                            dump = await DumpStream();
                        }
                    }
                    else
                    {
                        SetStatus(api.errorMessage);
                    }
                }
                else
                {
                    dump = await DumpStream();
                }

                if (dump)
                {
                    timestart = DateTime.Now;
                    counter = 0;
                }

                if (!isRecording) break;
                if (radioMaxRetry.IsChecked == false)
                {
                    if (radioNoRetry.IsChecked == true) break;
                    if ((radioRetry.IsChecked == true) && IsTimeRetryOver(timestart)) break;
                }

                counter++;
                SetStatus($"Retry {counter} ...");

                await Task.Delay(10000);
            }

            isRecording = false;
            LockInput(false);
            buttonDump.Visibility = Visibility.Visible;
            buttonStop.Visibility = Visibility.Collapsed;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (process != null)
            {
                isRecording = false;
                process.Kill();
            }

            buttonDump.Visibility = Visibility.Visible;
            buttonStop.Visibility = Visibility.Collapsed;
        }

        private bool IsTimeRetryOver(DateTime timestart)
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
            buttonPlay.IsEnabled = false;
            Process.Start(user.videoPlayUrl);
            SetStatus("Opening stream with default player");
            buttonPlay.IsEnabled = true;
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            buttonCopy.IsEnabled = false;
            Clipboard.SetText($"{user.nickname} — {user.announcement}\n\n▶️ LiveShow » {user.videoPlayUrl.ToPlaylist()}");
            SetStatus("Link is copied into clipboard");
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
    }
}