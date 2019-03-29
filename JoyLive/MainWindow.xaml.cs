using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

            if (App.UseAccount)
            {
                AddStatus("Checking status...");
                var api = new JoyLiveApi();
                var status = await api.isLoggedIn();
                App.UseAccount = status;
                AddStatus("Status : " + (status ? "Logged In" : "Logged Out"));
                if (!status)
                {
                    var login = await api.Login();
                    AddStatus("Login : " + (login ? "Success" : "Failed"));
                    App.UseAccount = login;
                }
            }

            Configs.SetRetryTimeoutValue();

            await GetNextPage();
            buttonMore.IsEnabled = true;

            var serial = App.GetSID();
            var isKeyValid = Configs.IsKeyValid(serial);
            if (!isKeyValid)
            {
                var auth = new AuthWindow();
                if (auth.ShowDialog() == false)
                    this.Close();
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
                await Task.Run(() =>
                {
                    InsertUsers(users, true);
                });
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
                await Task.Run(() =>
                {
                    InsertUsers(users);
                });
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
            Dispatcher.Invoke(() =>
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