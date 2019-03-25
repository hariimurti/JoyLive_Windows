using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Shapes;

namespace JoyLive
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private string serial = string.Empty;

        public AuthWindow()
        {
            InitializeComponent();

            serial = App.GetSID();
            textSerial.Text = serial;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var key = textKey.Text;
            if (string.IsNullOrWhiteSpace(key)) return;

            if (!Configs.SaveKeyIfValid(serial, key))
            {
                MessageBox.Show("Key is not valid!", "Authentication", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
