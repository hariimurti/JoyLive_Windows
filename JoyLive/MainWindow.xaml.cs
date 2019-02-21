using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ButtonNext_Click(null, null);
        }

        private void AddStatus(string text)
        {
            var time = DateTime.Now.ToString("HH:mm:ss");
            var index = boxStatus.Items.Add($"{time} » {text}");
            boxStatus.SelectedIndex = index;
        }

        private void ButtonLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            var user = btn.Tag as JoyUser;

            if (user == null) return;

            if (user.Mode == JoyUser.ActionMode.Open)
            {
                AddStatus($"Open : {user.Nickname} ({user.Id})");
                var window = new AdvancedWindow(user);
                window.Show();
            }
            else
            {
                AddStatus($"Play : {user.Nickname} ({user.Id})");
                Process.Start(user.Url);
            }
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            buttonReset.IsEnabled = false;
            buttonNext.IsEnabled = false;
            //textNextPage.Text = "Loading...";

            var api = new JoyLiveApi();

            AddStatus("Reset: Loading page 1...");

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
            buttonNext.IsEnabled = true;
            textNextPage.Text = "Get Page " + api.GetNextPage();
        }

        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            buttonReset.IsEnabled = false;
            buttonNext.IsEnabled = false;
            //textNextPage.Text = "Loading...";

            var api = new JoyLiveApi();

            AddStatus($"Loading page {api.GetNextPage()}...");

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
            buttonNext.IsEnabled = true;
            textNextPage.Text = "Get Page " + api.GetNextPage();
        }

        private void InsertUsers(RoomInfo[] users, bool reset = false)
        {
            int count = 0;
            if (reset) listBox.Items.Clear();

            foreach (var user in users)
            {
                //only female
                if (user.sex != "2") continue;

                count++;
                var context = new ListBoxContext(user.mid, user.nickname, user.announcement, user.headPic, user.videoPlayUrl);
                listBox.Items.Add(context);
            }

            if (count > 0)
                AddStatus($"Added {count} new girls...");
            else
                AddStatus($"Can't find any girl in {users.Length} users...");
        }
    }
}