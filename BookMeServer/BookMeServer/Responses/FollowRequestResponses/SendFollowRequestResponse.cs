namespace BookMeServer.Responses.FollowRequestResponses
{
    public class SendFollowRequestResponse
    {
        public bool SenderExists { get; set; }
        public bool ReceiverExists { get; set; }
        public bool SenderIsAlreadyReceiver { get; set; }
        public bool FollowRequestAlreadyExists { get; set; }
        public bool UserSendsHimselfFR { get; set; }
        public bool AlreadyFriends {  get; set; }
        public bool IsBlockedByUser { get; set; }
        public bool IsBlockedByOtherUser { get; set; }
        public bool ChatExists { get; set; }
    }
}
