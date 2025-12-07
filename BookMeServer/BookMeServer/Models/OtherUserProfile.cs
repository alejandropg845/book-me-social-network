namespace BookMeServer.Models
{
    public class OtherUserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public bool UserSentFollowRequest { get; set; }
        public bool CurrentUserSentFollowRequest { get; set; }
        public bool IsBlockedByUser { get; set; }
        public bool BothUsersFollow { get; set; }
        public bool CurrentUserIsBlocked { get; set; }
    }
}
