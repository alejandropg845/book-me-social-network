namespace BookMeServer.Responses.UserResponses
{
    public class OtherUserValidations
    {
        public bool UserSentFollowRequest { get; set; }
        public bool CurrentUserAlreadySentFR { get; set; }
        public bool BothUsersFollow { get; set; }
        public bool UserIsBlocked { get; set; }
        public bool CurrentUserIsBlocked { get; set; }

    }
}
