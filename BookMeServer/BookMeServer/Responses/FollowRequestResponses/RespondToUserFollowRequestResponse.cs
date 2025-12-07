namespace BookMeServer.Responses.FollowRequestResponses
{
    public class RespondToUserFollowRequestResponse
    {
        public bool SenderExists { get; set; }
        public bool ReceiverExists { get; set; }
        public bool FollowRequestExists { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsBlockedByUser { get; set; }
        public bool IsBlockedByOtherUser { get; set; }
        public bool ChatExists { get; set; }
    }
}
