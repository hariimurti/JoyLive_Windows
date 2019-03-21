using System.Collections.Generic;

namespace JoyLive
{
    public class JoyLiveBase
    {
        public int errno { get; set; }
        public string msg { get; set; }
    }

    public class JoyLiveRooms : JoyLiveBase
    {
        public Rooms data { get; set; }
    }

    public class JoyLiveRoomInfo : JoyLiveBase
    {
        public RoomInfo data { get; set; }
    }

    public class JoyLiveUserInfo : JoyLiveBase
    {
        public UserInfo data { get; set; }
    }

    public class JoyLiveLogin : JoyLiveBase
    {
        public Login data { get; set; }
    }

    public class Login
    {
        public string uid { get; set; }
    }

    public class Rooms
    {
        public User[] rooms { get; set; }
    }

    public class User
    {
        public string rid { get; set; }
        public long playStartTime { get; set; }
        public string sex { get; set; }
        public string mid { get; set; }
        public string nickname { get; set; }
        public string headPic { get; set; }
        public bool isPlaying { get; set; }
        public string status { get; set; }
        public double onlineNum { get; set; }
        public string fansNum { get; set; }
        public string announcement { get; set; }
        public string level { get; set; }
        public string moderatorLevel { get; set; }
        //public bool verified { get; set; }
        public string videoPlayUrl { get; set; }
        public int price { get; set; }

        public List<string> blacklist = new List<string>() { "2170276", "79041", "51987", "24499" };
    }

    public class UserInfo : User
    {
        public string id { get; set; }
        public string signature { get; set; }
        public string birthday { get; set; }
    }

    public class RoomInfo
    {
        public string id { get; set; }
        public string videoStreamName { get; set; }
        public int onlineNum { get; set; }
        public int price { get; set; }
        public bool isPlaying { get; set; }
        public string shareTitle { get; set; }
        public string shareContent { get; set; }
        public string sharePic { get; set; }
        public string shareUrl { get; set; }
    }

    public class UserAction
    {
        public User user { get; set; }
        public Action action { get; set; }

        public enum Action { Open, Play }

        public UserAction(User user, Action action)
        {
            this.user = user;
            this.action = action;
        }
    }
}