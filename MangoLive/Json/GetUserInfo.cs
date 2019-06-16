using System.Collections.Generic;

namespace MangoLive.Json
{
    public class UserInfo
    {
        public string id { get; set; }
        public string nickname { get; set; }
        public string headPic { get; set; }
        public string level { get; set; }
        public int nextLevel { get; set; }
        public int nextLevelNeedExp { get; set; }
        public string attentionNum { get; set; }
        public string fansNum { get; set; }
        public string type { get; set; }
        public string birthday { get; set; }
        public string sex { get; set; }
        public string signature { get; set; }
        public List<object> lastPostImages { get; set; }
        public int isVip { get; set; }
        public int vipLevel { get; set; }
        public string bgImg { get; set; }
        public string moderatorLevel { get; set; }
        public bool isPlaying { get; set; }
        public int price { get; set; }
        public string videoPlayUrl { get; set; }
        public string rid { get; set; }
        public string verified { get; set; }
        public string verifyInfo { get; set; }
        public bool isAttention { get; set; }
    }

    public class GetUserInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public UserInfo data { get; set; }
    }
}
