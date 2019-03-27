using System;
using System.Windows.Media.Imaging;

namespace JoyLive
{
    internal class ListBoxContext
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Announcement { get; set; }
        public BitmapImage ImageProfile { get; set; }
        public UserAction PlayStream { get; set; }
        public UserAction OpenUser { get; set; }

        public ListBoxContext(User user)
        {
            Id = user.mid;
            Nickname = user.nickname;
            Announcement = user.announcement;
            ImageProfile = new BitmapImage(new Uri(user.headPic));
            PlayStream = new UserAction(user, UserAction.Action.Play);
            OpenUser = new UserAction(user, UserAction.Action.Open);

            if (user.price != 0)
                Nickname += $" — {user.price} 💰";
        }
    }
}