namespace JoyLive
{
    class LinkData
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Link { get; set; }

        public LinkData(string _id, string _nickname, string _link)
        {
            Id = _id;
            Nickname = _nickname;
            Link = _link;
        }
    }
}
