using MangoLive.Json;

namespace MangoLive
{
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