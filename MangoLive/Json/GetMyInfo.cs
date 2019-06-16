using System.Collections.Generic;

namespace MangoLive.Json
{
    public class MyInfo
    {
        public string id { get; set; }
        public string nickname { get; set; }
        public string headPic { get; set; }
        public string birthday { get; set; }
        public string sex { get; set; }
        public string signature { get; set; }
        public string level { get; set; }
        public string exp { get; set; }
        public int nextLevel { get; set; }
        public int nextLevelNeedExp { get; set; }
        public string attentionNum { get; set; }
        public string fansNum { get; set; }
        public string type { get; set; }
        public string coin { get; set; }
        public string gameCoin { get; set; }
        public int incomeAvailable { get; set; }
        public bool canEditSex { get; set; }
        public bool needEditSex { get; set; }
        public int needEditSexCoin { get; set; }
        public int messageNeedCoin { get; set; }
        public int chargeRoomNeedLevel { get; set; }
        public bool isShowBalance { get; set; }
        public bool isNeedTicket { get; set; }
        public int freeEndNeedTicketMiniLevel { get; set; }
        public int miniLevelNeedExp { get; set; }
        public bool isFeedbackNotice { get; set; }
        public bool isShowVip { get; set; }
        public int barragePrice { get; set; }
        public List<object> lastPostImages { get; set; }
        public int isVip { get; set; }
        public int isTicket { get; set; }
        public int leftFreeDay { get; set; }
        public int leftTicketDay { get; set; }
        public string bgImg { get; set; }
        public string ryToken { get; set; }
        public bool isBan { get; set; }
        public int banLeftTime { get; set; }
        public int vipLevel { get; set; }
        public int leftDays { get; set; }
        public bool showModify { get; set; }
    }

    public class GetMyInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public MyInfo data { get; set; }
    }
}
