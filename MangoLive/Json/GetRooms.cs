using System.Collections.Generic;

namespace MangoLive.Json
{
    public class User
    {
        public string rid { get; set; }
        public int playStartTime { get; set; }
        public string sex { get; set; }
        public string mid { get; set; }
        public string nickname { get; set; }
        public string headPic { get; set; }
        public bool isPlaying { get; set; }
        public string status { get; set; }
        public int onlineNum { get; set; }
        public string fansNum { get; set; }
        public string announcement { get; set; }
        public string level { get; set; }
        public string moderatorLevel { get; set; }
        public bool verified { get; set; }
        public string verifyInfo { get; set; }
        public string videoPlayUrl { get; set; }
        public List<object> topics { get; set; }
        public int price { get; set; }
        public string city { get; set; }
        public List<object> tags { get; set; }
        public int weight { get; set; }
        public string hotWeight { get; set; }
        public string isBrand { get; set; }
    }

    public class GetRooms
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public User[] data { get; set; }
    }
}
