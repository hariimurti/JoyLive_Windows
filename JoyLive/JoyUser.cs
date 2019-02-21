namespace JoyLive
{
    public class JoyUser
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public ActionMode Mode { get; set; }

        public enum ActionMode { Play, Open }

        public JoyUser(string _id, string _nickname, string _image, string _url, ActionMode _mode)
        {
            Id = _id;
            Nickname = _nickname;
            ImageUrl = _image;
            Url = _url;
            Mode = _mode;
        }
    }

    internal static class JoyUserExt
    {
        public static string ToPlaylistUrl(this string link)
        {
            return link.Replace("rtmp://", "http://") + "/playlist.m3u8";
        }
    }
}