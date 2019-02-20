using JoyLive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var link = btn.Tag as LinkData;

            if (link.Link.StartsWith("http"))
            {
                AddStatus($"Copy : {link.Nickname} ({link.Id})");
                //Clipboard.SetText(link.Link);
                var rtmp = link.Link.Replace("http://", "rtmp://").Replace("/playlist.m3u8","");
                Clipboard.SetText(rtmp);
            }
            else
            {
                AddStatus($"Play : {link.Nickname} ({link.Id})");
                Process.Start(link.Link);
            }
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            buttonReset.IsEnabled = false;
            buttonNext.IsEnabled = false;
            buttonNext.Content = "Loading...";

            var api = new JoyLiveApi();

            AddStatus("Reset: Loading page 1...");

            var users = await api.Reset();
            if (!api.isError)
            {
                await InsertUsers(users, true);
            }
            else
            {
                AddStatus(api.errorMessage);
            }

            buttonReset.IsEnabled = true;
            buttonNext.IsEnabled = true;
            buttonNext.Content = "Get Page " + api.GetNextPage();
        }

        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            buttonReset.IsEnabled = false;
            buttonNext.IsEnabled = false;
            buttonNext.Content = "Loading...";

            var api = new JoyLiveApi();

            AddStatus($"Loading page {api.GetNextPage()}...");

            var users = await api.GetRoomInfo();
            if (!api.isError)
            {
                await InsertUsers(users);
            }
            else
            {
                AddStatus(api.errorMessage);
            }

            buttonReset.IsEnabled = true;
            buttonNext.IsEnabled = true;
            buttonNext.Content = "Get Page " + api.GetNextPage();
        }

        private async Task InsertUsers(RoomInfo[] users, bool reset = false)
        {
            int count = 0;
            if (reset) listBox.Items.Clear();

            foreach (var user in users)
            {
                //only female
                if (user.sex != "2") continue;

                count++;
                var context = new ListBoxContext(user.mid, user.nickname, user.announcement, user.headPic, user.videoPlayUrl);
                context.SetImagePath(await JoyLiveApi.GetImageCache(context));
                listBox.Items.Add(context);
            }

            if (count > 0)
                AddStatus($"Added {count} new girls...");
            else
                AddStatus($"Can't find any girl in {users.Length} users...");
        }
    }
}
