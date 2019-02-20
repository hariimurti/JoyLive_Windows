using JoyLive;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace JoyLive
{
    class ListBoxContext : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Announcement { get; set; }
        public string ImageUrl { get; set; }
        public LinkData OpenLink { get; set; }
        public LinkData CopyLink { get; set; }

        public ListBoxContext(string _id,  string _nickname, string _announcement, string _imageUrl, string _link)
        {
            Id = _id;
            Nickname = _nickname;
            Announcement = _announcement;
            ImageUrl = _imageUrl;

            OpenLink = new LinkData(_id, _nickname, _link);

            var http = _link.Replace("rtmp://", "http://") + "/playlist.m3u8";
            CopyLink = new LinkData(_id, _nickname, http);
        }

        public void SetImagePath(string _imagepath)
        {
            ImageUrl = _imagepath;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
