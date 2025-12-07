namespace BookMeServer.Responses.ChatResponses
{
    public class SendMessageResponse
    {
        public bool SenderExists { get; set; }
        public bool ReceiverExists { get; set; }
        public bool ChatExists { get; set; }
        public bool IsBlockedByUser { get; set; }
        public bool IsBlockedByOtherUser { get; set; }

    }
}
