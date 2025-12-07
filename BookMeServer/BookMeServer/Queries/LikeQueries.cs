namespace BookMeServer.Queries
{
    public static class LikeQueries
    {
        public static readonly string RevertLikedReplyDataAsync
            = @"UPDATE CommentReply SET ReplyLikes = ReplyLikes - 1 WHERE Id = @replyId
                DELETE FROM UserLikedReplies WHERE UserId = @userId AND LikedReplyId = @replyId
                DELETE FROM Notifications 
                                WHERE RecipientUserId = @LikingToId 
                                AND ActorId = @userId
                                AND Type = 'ReplyLiked'";
        public static readonly string AddNonLikedReplyData
            = @"
                UPDATE CommentReply SET ReplyLikes = ReplyLikes + 1 WHERE Id = @replyId;
                    
                INSERT INTO UserLikedReplies VALUES (@userId, @replyId);

                -- Verificar que el usuario no se auto-lickea
                IF @userId != @likingToId
                    BEGIN
                        INSERT INTO Notifications (
	                        RecipientUserId, ActorId, Type, PostId,
	                        Status, IsRead, CreatedAt
                        ) VALUES (
	                        @likingToId, @userId, 'ReplyLiked', @postId, 
	                        NULL, DEFAULT, DEFAULT
                        )
                    END
                ";
        public static readonly string RevertLikedCommentData
            = @"
			    UPDATE Comment SET Likes = Likes - 1 WHERE Id = @commentId;

			    DELETE FROM UserLikedComments WHERE UserId = @userId AND LikedCommentId = @commentId;

			    DELETE FROM Notifications 
                                WHERE ActorId = @userId 
                                AND RecipientUserId = @authorIdComment
                                AND Type = 'LikedComment';

            ";
        public static readonly string AddLikedCommentData
            = @"
                
			    UPDATE Comment SET Likes = Likes + 1 WHERE Id = @commentId;

			    INSERT INTO UserLikedComments VALUES (@userId, @commentId);

			    -- Verificar que el usuario no se auto-likea para agregar notification
			    IF @userId != @authorIdComment
				    BEGIN
                        INSERT INTO Notifications (
	                        RecipientUserId, ActorId, Type, PostId,
	                        Status, IsRead, CreatedAt
                        ) OUTPUT inserted.Id VALUES (
	                        @authorIdComment, @userId, 'LikedComment', @postId, 
	                        NULL, DEFAULT, DEFAULT
                        )
				    END
            ";
    }
}
