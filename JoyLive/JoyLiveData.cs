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
        public bool verified { get; set; }
        public string videoPlayUrl { get; set; }
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