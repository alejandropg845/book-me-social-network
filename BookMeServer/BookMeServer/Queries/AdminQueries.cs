namespace BookMeServer.Queries
{
    public static class AdminQueries
    {
        public static readonly string DisableUser
        = @"
        UPDATE Users SET IsDisabled = 1 WHERE Id = @userId;
        UPDATE Users SET Status = 'offline' WHERE Id = @userId;
        UPDATE Comment SET IsDeleted = 1 WHERE UserId = @userId;
        UPDATE CommentReply SET IsDeleted = 1 WHERE UserId = @userId;
        ";
        public static readonly string DeletePost
            = @"UPDATE Post SET IsDeleted = 1 WHERE Id = @postId";
    }
}
