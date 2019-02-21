namespace JoyLive
{
    public class JoyLiveData
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public RoomInfo[] rooms { get; set; }
    }

    public class RoomInfo
    {
        public string rid { get; set; }
        public string sex { get; set; }
        public string mid { get; set; }
        public string nickname { get; set; }
        public string headPic { get; set; }
        public bool isPlaying { get; set; }
        public string status { get; set; }
        public int onlineNum { get; set; }
        public string fansNum { get; set; }
        public string announcement { get; set; }
        public bool verified { get; set; }
        public string videoPlayUrl { get; set; }
    }
}