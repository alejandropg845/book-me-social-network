namespace BookMeServer.Queries
{
    public static class CommentQueries
    {
        public static readonly string AddCommentValidations
            = @"
                --Verificar que el post exista.
                SELECT
                CASE WHEN EXISTS
                (SELECT 1 FROM Post WHERE Id = @postId AND IsDeleted = 0) THEN 1 ELSE 0
                END AS PostExists,

                --Verificar que el usuario no tenga al otro usuario bloqueado
	            CASE WHEN EXISTS
	            (SELECT 1 FROM UserBlackList WHERE UserId = @userId
	            AND OtherUserId = @PostUserId) THEN 1 ELSE 0
	            END AS IsBlockedByUser,

	            --Verificar que el otro usuario no tenga bloqueado al usuario
	            CASE WHEN EXISTS
	            (SELECT 1 FROM UserBlackList WHERE UserId = @PostUserId
	            AND OtherUserId = @userId) THEN 1 ELSE 0
	            END AS IsBlockedByOtherUser,
                    
                --Verificar que el usuario comentando exista.
                CASE WHEN EXISTS
	            (SELECT Username FROM Users WHERE Id = @userId AND IsDisabled = 0) 
					THEN (SELECT Username FROM Users WHERE Id = @userId AND IsDisabled = 0) 
	            ELSE 
					NULL
	            END AS CommentingUser;
                ";
        public static readonly string AddCommentNotificationToUser
            = @"
                INSERT INTO Notifications (
	                RecipientUserId, ActorId, Type, PostId,
	                Status, IsRead, CreatedAt
                ) OUTPUT inserted.Id VALUES (
	                @postUserId, @userId, 'Comment', @postId, 
	                NULL, DEFAULT, DEFAULT
                )
            ";
        public static readonly string AddLikedCommentNotificationForOtherUser
            = @"
                INSERT INTO Notifications (
                    RecipientUserId, ActorId, Type, PostId,
                    Status, IsRead, CreatedAt
                ) OUTPUT inserted.Id VALUES (
                    @authorIdComment, @userId, 'LikedComment', @postId, 
                    NULL, DEFAULT, DEFAULT
                )
            ";
        public static readonly string AddAndGetCommentInfo
            = @"
                INSERT INTO Comment (Author, Content, CommentDate, PostId, UserId) 
                    OUTPUT INSERTED.CommentDate, INSERTED.Id
                VALUES (@Author, @Content, GETDATE(), @PostId, @userId)
            ";

        public static readonly string InsertAndGetReply
            = @"
                INSERT INTO CommentReply (CommentId, UserId, ReplyingToId, Content)
                VALUES (@commentId, @userId, @replyingToId, @content);

                SET @replyId = SCOPE_IDENTITY()

                SELECT 
	                cr.Id,
	                cr.UserId,
	                cr.CommentId,
	                u.Username AS AuthorUsername,
	                u.ImageUrl,
	                cr.ReplyLikes,
	                cr.ReplyingToId,
	                u2.Username as ReplyingToUsername,
	                cr.RepliedAt,
	                cr.Content,
	                0 AS IsLikedByUser
                FROM CommentReply cr
                LEFT JOIN Users u
	                ON cr.UserId = u.Id
                LEFT JOIN Users u2
	                ON u2.Id = cr.ReplyingToId
                WHERE cr.Id = @replyId
                
            ";

        public static readonly string LikeCommentValidations
			= @"
                SELECT
                CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = @userId AND IsDisabled = 0)
                THEN 1 ELSE 0 END AS UserExists,

                CASE WHEN EXISTS (SELECT 1 FROM Comment 
                WHERE Id = @CommentId AND IsDeleted = 0)
                THEN 1 ELSE 0 END AS CommentExists,

                CASE WHEN EXISTS (SELECT 1 FROM UserLikedComments WHERE UserId = @userId
                AND LikedCommentId = @CommentId)
                THEN 1 ELSE 0 END AS IsAlreadyLiked,

                CASE WHEN EXISTS (SELECT 1 FROM Post WHERE Id = @PostId AND IsDeleted = 0)
                THEN 1 ELSE 0 END AS PostExists;
                ";
		public static readonly string LikeReplyValidations
			= @"SELECT

                  CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = @userId
                  AND IsDisabled = 0) THEN 1 ELSE 0 END AS UserExists,

                  CASE WHEN EXISTS (SELECT 1 FROM CommentReply 
                  WHERE Id = @replyId) THEN 1 ELSE 0
                  END AS ReplyExists,
                  
                  CASE WHEN EXISTS (SELECT 1 FROM UserLikedReplies 
                  WHERE UserId = @userId AND LikedReplyId = @replyId)
                  THEN 1 ELSE 0 END AS IsAlreadyLiked,

                  CASE WHEN EXISTS (SELECT 1 FROM Comment 
                  WHERE Id = @commentId AND IsDeleted = 0) THEN 1 ELSE 0
                  END AS CommentExists,

                  CASE WHEN EXISTS (SELECT 1 FROM Post WHERE Id = @PostId AND IsDeleted = 0)
                  THEN 1 ELSE 0 END AS PostExists;
                  ";
        public static readonly string ReplyToCommentValidations
            = @"
                SELECT 

                    --Verificar que el comentario a responder existe
                    CASE WHEN EXISTS (SELECT 1 FROM Comment 
                    WHERE Id = @CommentId AND IsDeleted = 0)
                    THEN 1 ELSE 0 END AS CommentExists,

                    --Verificar que el usuario respondiendo existe
                    CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = @userId
                    AND IsDisabled = 0) THEN 1 ELSE 0 END AS ReplierExists,

                    CASE WHEN EXISTS (SELECT 1 FROM Post WHERE Id = @PostId AND IsDeleted = 0)
                    THEN 1 ELSE 0 END AS PostExists;

                ";
        
        public static readonly string GetPostComments
            = @"
                SELECT 
	                c.Id AS CommentId,
	                u.ImageUrl AS AuthorImage,
	                c.Author,
	                c.Content,
	                c.CommentDate,
	                c.PostId,
	                c.Likes AS CommentLikes,
	                COUNT(cr.Id) AS RepliesNumber,
	                u.Id AS AuthorId,
	                CASE WHEN EXISTS (
		                SELECT 1 FROM UserLikedComments x 
		                WHERE x.UserId = @userId
		                AND x.LikedCommentId = c.Id
	                ) THEN 1 ELSE 0 END AS IsLiked,
	                CASE WHEN c.UserId = @userId THEN 1 ELSE 0 END AS IsCommentOwner
	                FROM 
		                Comment c
		                LEFT JOIN Post p ON p.Id = c.PostId
		                LEFT JOIN CommentReply cr ON cr.CommentId = c.Id AND cr.IsDeleted = 0
		                LEFT JOIN Users u ON c.UserId = u.Id
	                WHERE c.PostId = @postId AND c.IsDeleted = 0
	
	                GROUP BY
		                c.Id,
		                u.ImageUrl,
		                c.Author,
		                c.Content,
		                c.CommentDate,
		                c.PostId,
		                c.Likes,
		                c.UserId,
		                u.Id,
		                p.Id
	                ORDER BY c.CommentDate ASC
	                OFFSET (3 * @number) ROWS FETCH NEXT 3 ROWS ONLY;
                ";
		public static readonly string GetPostCommentsAsAnonymous
			= @"SELECT 
	                c.Id AS CommentId,
	                u.ImageUrl AS AuthorImage,
	                c.Author,
	                c.Content,
	                c.CommentDate,
	                c.PostId,
	                c.Likes AS CommentLikes,
	                COUNT(cr.Id) AS RepliesNumber,
	                u.Id AS AuthorId
	                FROM 
		                Post p
		                LEFT JOIN Comment c ON p.Id = c.PostId
		                LEFT JOIN CommentReply cr ON cr.CommentId = c.Id AND cr.IsDeleted = 0
		                LEFT JOIN Users u ON c.UserId = u.Id
	                WHERE p.Id = @postId
	
	                GROUP BY
		                c.Id,
		                u.ImageUrl,
		                c.Author,
		                c.Content,
		                c.CommentDate,
		                c.PostId,
		                c.Likes,
		                c.UserId,
		                u.Id,
		                p.Id
	                ORDER BY c.CommentDate ASC
	                OFFSET (3 * @number) ROWS FETCH NEXT 3 ROWS ONLY;";
		public static readonly string SetCommentAsDeleted
			= @"UPDATE Comment SET IsDeleted = 1 WHERE 
                UserId = @userId AND Id = @commentId;";
        public static readonly string GetCommentReplies 
            = @"
                SELECT 
	            cr.Id,
	            cr.CommentId,
	            cr.UserId,
	            u.Username AS AuthorUsername,
	            u.ImageUrl,
	            cr.ReplyLikes,
	            cr.RepliedAt,
	            cr.Content,
	            u2.Username AS ReplyingToUsername,
	            CASE WHEN cr.UserId = @userId
		            THEN 1
		            ELSE 0
	            END AS IsReplyOwner,
	            CASE WHEN ulr.UserId IS NOT NULL 
		            THEN 1 
		            ELSE 0 
	            END AS IsLikedByUser
	            FROM 
	            CommentReply cr
	            LEFT JOIN UserLikedReplies ulr 
		            ON ulr.UserId = @userId
		            AND ulr.LikedReplyId = cr.Id
	            LEFT JOIN Users u
		            ON u.Id = cr.UserId
	            LEFT JOIN Users u2
		            ON u2.Id = cr.ReplyingToId

	            WHERE cr.CommentId = @commentId
						AND cr.IsDeleted = 0

	            ORDER BY cr.RepliedAt DESC
	            OFFSET (3 * @number) ROWS FETCH NEXT 3 ROWS ONLY;           

                ";
		public static readonly string GetCommentRepliesAsAnonymous
            = @"
                SELECT 
	            cr.Id,
	            cr.CommentId,
	            cr.UserId,
	            u.Username AS AuthorUsername,
	            u.ImageUrl,
	            cr.ReplyLikes,
	            cr.RepliedAt,
	            cr.Content,
	            u2.Username AS ReplyingToUsername
	            FROM 
	            CommentReply cr
	            LEFT JOIN Users u
		            ON u.Id = cr.UserId
	            LEFT JOIN Users u2 
		            ON u2.Id = cr.ReplyingToId
	            WHERE cr.CommentId = @commentId
	            AND cr.IsDeleted = 0
	            ORDER BY cr.RepliedAt DESC
	            OFFSET (3 * @number) ROWS FETCH NEXT 3 ROWS ONLY;                

                ";
		public static readonly string AddCommentReplyNotification
			= @"
				INSERT INTO Notifications (
	                RecipientUserId, ActorId, Type, PostId,
	                Status, IsRead, CreatedAt
                ) VALUES (
	                @replyingToId, @userId, 'CommentReply', @postId, 
	                NULL, DEFAULT, DEFAULT
                )
			";
		public static readonly string SetReplyAsDeleted
			= @"UPDATE CommentReply SET IsDeleted = 1 WHERE Id = @ReplyId
                AND UserId = @userId";
        public static readonly string DeleteReplyValidations
            = @"
                SELECT
                CASE WHEN EXISTS (SELECT 1 FROM Comment WHERE Id = @CommentId
                AND IsDeleted = 0) THEN 1 ELSE 0
                END AS CommentExists,
                
                CASE WHEN EXISTS (SELECT 1 FROM CommentReply WHERE Id = @ReplyId)
                THEN 1 ELSE 0 END AS ReplyExists
                ";
        public static readonly string DeleteCommentValidations
            = @"SELECT
                  CASE WHEN EXISTS 
                  (SELECT 1 FROM Comment WHERE Id = @commentId 
                  AND IsDeleted = 0) THEN 1 ELSE 0
                  END AS CommentExists,
                
                  CASE WHEN EXISTS
                  (SELECT 1 FROM Post WHERE Id = @postId
                   AND IsDeleted = 0)
                  THEN 1 ELSE 0 END AS PostExists;";
    }
}
