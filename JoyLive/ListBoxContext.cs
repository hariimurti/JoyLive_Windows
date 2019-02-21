using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JoyLive
{
    internal class ListBoxContext : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Announcement { get; set; }
        public string ImageUrl { get; set; }
        public JoyUser PlayLink { get; set; }
        public JoyUser OpenLink { get; set; }

        public ListBoxContext(string _id, string _nickname, string _announcement, string _imageUrl, string _link)
        {
            Id = _id;
            Nickname = _nickname;
            Announcement = _announcement;
            ImageUrl = _imageUrl;

            PlayLink = new JoyUser(_id, _nickname, _imageUrl, _link, JoyUser.ActionMode.Play);
            OpenLink = new JoyUser(_id, _nickname, _imageUrl, _link, JoyUser.ActionMode.Open);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}