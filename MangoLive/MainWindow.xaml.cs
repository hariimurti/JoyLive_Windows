using MangoLive.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MangoLive
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

            var lastHeight = Configs.GetHeight();
            if (SystemParameters.PrimaryScreenHeight > lastHeight)
            {
                var diff = lastHeight - Height;
                Height = lastHeight;
                listBox.MinHeight += diff;
            }

            var top = Configs.GetWindowTop();
            var left = Configs.GetWindowLeft();
            if (top != 0 || left != 0)
            {
                Top = top;
                Left = left;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buttonMore.IsEnabled = false;
            AddStatus("Starting...");
            AddStatus("Find Me at https://t.me/paijemdev");

            AddStatus("Reading configs...");
            Configs.SetRetryTimeoutValue();

            AddStatus("Get my Account...");
            var myinfo = await MangoApi.GetMyInfo();
            if (myinfo.errno != 0)
            {
                while (true)
                {
                    AddStatus("Login with account...");
                    var login = await MangoApi.Login();
                    if (login.errno != 0)
                    {
                        AddStatus("Status : Login Failed!");
                        var dialog = MessageBox.Show($"{login.msg}\n\n-- -- --\n[YES] Try again? [NO] To close.", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (dialog == MessageBoxResult.No)
                        {
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        AddStatus("Status : Login Success");
                        break;
                    }
                }
            }
            else
            {
                AddStatus("Status : Logged In");
            }

            await GetNextPage();
            buttonMore.IsEnabled = true;

            var serial = App.GetSID();
            var isKeyValid = Configs.IsKeyValid(serial);
            if (!isKeyValid)
            {
                var auth = new AuthWindow();
                if (auth.ShowDialog() == false)
                    Environment.Exit(1);
            }
        }

        private void AddStatus(string text)
        {
            Dispatcher.Invoke(() =>
            {
                var time = DateTime.Now.ToString("HH:mm:ss");
                var index = boxStatus.Items.Add($"{time} » {text}");
                boxStatus.SelectedIndex = index;
            });
        }

        private void ButtonLink_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            if (btn == null) return;

            var ua = btn.Tag as UserAction;
            if (ua == null) return;

            btn.IsEnabled = false;
            var user = ua.user;
            if (ua.action == UserAction.Action.Play)
            {
                Process.Start(user.videoPlayUrl);
                AddStatus($"Play : {user.nickname} ({user.mid})");
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
            await ResetListWithPageZero();
        }

        private async void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            await GetNextPage();
        }

        private int nextPage = 0;
        private async Task ResetListWithPageZero()
        {
            await GetNextPage(true);

            if (listBox.Items.Count > 0)
            {
                listBox.ScrollIntoView(listBox.Items[0]);
            }
        }

        private async Task GetNextPage(bool reset = false)
        {
            buttonReset.IsEnabled = false;
            buttonMore.IsEnabled = false;

            if (reset)
                nextPage = 0;

            bool noMore = false;
            AddStatus($"Loading page {nextPage}");

            var rooms = await MangoApi.GetRooms(nextPage);
            if (rooms.errno == 0)
            {
                if (rooms.data.Length > 0)
                {
                    nextPage++;
                    await Task.Run(() =>
                    {
                        InsertUsers(rooms.data, reset);
                    });
                }
                else
                {
                    AddStatus("No more users");
                    noMore = true;
                }
            }
            else
            {
                AddStatus(rooms.msg);
            }

            buttonReset.IsEnabled = true;
            buttonMore.IsEnabled = !noMore;
        }

        private void InsertUsers(User[] users, bool reset = false)
        {
            Dispatcher.Invoke(() =>
            {
                int count = 0;
                if (reset) listBox.Items.Clear();

                foreach (var user in users)
                {
                    //only female & unknown
                    if (user.sex == "1") continue;
                    
                    var context = new ListBoxContext(user);

                    var found = false;
                    foreach (ListBoxContext item in listBox.Items)
                    {
                        if (item.Id == context.Id)
                            found = true;
                    }

                    if (!found)
                    {
                        listBox.Items.Add(context);
                        count++;
                    }
                }

                if (count > 0) AddStatus($"Added {count} new users");
            });
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F12)
            {
                ButtonFind_Click(sender, e);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Width > 350) Width = 350;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if ((Height > 550) && (SystemParameters.PrimaryScreenHeight > Height))
                Configs.SaveHeight(Height);

            Configs.SaveWindow(Top, Left);
        }
    }
}