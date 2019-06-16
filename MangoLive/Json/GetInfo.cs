namespace MangoLive.Json
{
    public class Info
    {
        public string id { get; set; }
        public bool loved { get; set; }
        public string videoPlayUrl { get; set; }
        public string videoPublishDomain { get; set; }
        public string videoPath { get; set; }
        public string videoStreamName { get; set; }
        public int previewNum { get; set; }
        public int price { get; set; }
        public int userCoin { get; set; }
        public string flowerNumber { get; set; }
        public bool isPlaying { get; set; }
        public int onlineNum { get; set; }
        public int rank { get; set; }
        public int userType { get; set; }
        public string shareTitle { get; set; }
        public string shareContent { get; set; }
        public string sharePic { get; set; }
        public string shareUrl { get; set; }
    }

    public class GetInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public Info data { get; set; }
    }
}
