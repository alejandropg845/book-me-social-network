using BookMeServer.Models;

namespace BookMeServer.Responses.CommentResponses
{
    public class ReplyToCommentResponse
    {
        public bool CommentExists { get; set; }
        public bool ReplierExists { get; set; }
        public int InsertedRows { get; set; }
        public CommentReply InsertedReply { get; set; }
        public bool PostExists {  get; set; }
    }
}
