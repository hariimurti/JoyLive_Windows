using System;
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
            AddStatus("Website https://github.com/hariimurti");
            AddStatus("Starting...");
            await GetNextPage();
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
            var ua = btn.Tag as UserAction;

            if (ua == null) return;

            var user = ua.user;
            if (ua.action == UserAction.Action.Play)
            {
                AddStatus($"Play : {user.nickname} ({user.mid})");
                Process.Start(user.videoPlayUrl);
            }
            else
            {
                AddStatus($"Open : {user.nickname} ({user.mid})");
                var window = new UserWindow(user);
                window.Show();
            }

        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserWindowCustom();
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
            buttonMore.IsEnabled = true;
        }

        private void InsertUsers(User[] users, bool reset = false)
        {
            int count = 0;
            if (reset) listBox.Items.Clear();

            foreach (var user in users)
            {
                //only female
                if (user.sex != "2") continue;

                var context = new ListBoxContext(user);

                if (!listBox.Items.Contains(context))
                {
                    listBox.Items.Add(context);
                    count++;
                }
            }

            if (count > 0)
                AddStatus($"Added {count} new girls...");
            else
                AddStatus($"Can't find any girl in page {JoyLiveApi.GetCurrentPage()} of {users.Length} users...");
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