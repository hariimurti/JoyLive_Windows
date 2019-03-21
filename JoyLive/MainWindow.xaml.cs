﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Title = string.Format("{0} v{1}", Title, App.GetBuildVersion());
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddStatus("Find Me at https://github.com/hariimurti");
            AddStatus("Starting...");

            var api = new JoyLiveApi();
            var status = await api.isLoggedIn();
            App.UseLoginMethod = status;
            AddStatus("Status : " + (status ? "Logged In" : "Logged Out"));
            if (!status)
            {
                var login = await api.Login();
                AddStatus("Login : " + (login ? "Success" : "Failed"));
                App.UseLoginMethod = login;
            }

            await GetNextPage();
        }

        private void AddStatus(string text)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var index = boxStatus.Items.Add($"{time} » {text}");
            boxStatus.SelectedIndex = index;
        }

        private async void ButtonLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            var ua = btn.Tag as UserAction;

            if (ua == null) return;

            btn.IsEnabled = false;
            var user = ua.user;
            if (ua.action == UserAction.Action.Play)
            {
                var room = await new JoyLiveApi().GetRoomInfo(user.rid);
                if (room.isPlaying)
                {
                    Process.Start(user.videoPlayUrl);
                    AddStatus($"Play : {user.nickname} ({user.mid})");
                }
                else
                {
                    AddStatus($"{user.nickname} ({user.mid}) - Offline");
                }
            }
            else
            {
                AddStatus($"Open : {user.nickname} ({user.mid})");
                var window = new UserWindow(user);
                window.Show();
            }
            btn.IsEnabled = true;
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserWindow(null);
            window.Show();
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            await ResetListWithPageOne();
        }

        private async void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            await GetNextPage();
        }

        private async Task ResetListWithPageOne()
        {
            buttonReset.IsEnabled = false;
            buttonMore.IsEnabled = false;

            var api = new JoyLiveApi();

            AddStatus("Reset: Loading page 1");

            var users = await api.Reset();
            if (!api.isError)
            {
                InsertUsers(users, true);
            }
            else
            {
                AddStatus(api.errorMessage);
            }

            buttonReset.IsEnabled = true;
            buttonMore.IsEnabled = true;

            if (listBox.Items.Count > 0)
            {
                listBox.ScrollIntoView(listBox.Items[0]);
            }
        }

        private async Task GetNextPage()
        {
            buttonReset.IsEnabled = false;
            buttonMore.IsEnabled = false;

            var api = new JoyLiveApi();

            AddStatus($"Loading page {api.GetNextPage()}");

            var users = await api.GetRoomInfo();
            if (!api.isError)
            {
                InsertUsers(users);
            }
            else
            {
                AddStatus(api.errorMessage);
            }

            buttonReset.IsEnabled = true;
            buttonMore.IsEnabled = true;
        }

        private void InsertUsers(User[] users, bool reset = false)
        {
            int count = 0;
            if (reset) listBox.Items.Clear();

            foreach (var user in users)
            {
                //only female & unknown
                if (user.sex == "1") continue;

                //check blacklist
                if (user.blacklist.Contains(user.mid)) continue;

                var context = new ListBoxContext(user);

                if (!listBox.Items.Contains(context))
                {
                    listBox.Items.Add(context);
                    count++;
                }
            }

            if (count > 0) AddStatus($"Added {count} new users");
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F12)
            {
                ButtonFind_Click(sender, e);
            }
        }
    }
}