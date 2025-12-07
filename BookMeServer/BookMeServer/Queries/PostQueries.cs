namespace BookMeServer.Queries
{
    public static class PostQueries
    {
        public static readonly string GetPosts
            = @"

				SELECT
					p.Id, 
					u.Id AS PostUserId, 
					u.ImageUrl as AuthorImageUrl,
					p.PostImageUrl, 
					u.Username, 
					p.Description, 
					p.Likes as PostLikes, 
					p.PostedDate,
					CASE WHEN EXISTS (
						SELECT 1 FROM UserLikedPosts x 
						WHERE x.PostId = p.Id
						AND x.UserId = @userId
					) THEN 1 ELSE 0 END AS IsLikedByUser,
					COUNT(c.Id) AS CommentsNumber
					FROM Post p
						JOIN Users u ON p.UserId = u.Id
						LEFT JOIN Comment c ON c.PostId = p.Id
											AND c.IsDeleted = 0
						LEFT JOIN UserBlackList bl ON bl.UserId = @userId
													AND bl.OtherUserId = p.UserId
				WHERE p.IsDeleted = 0 AND bl.Id IS NULL

				GROUP BY
					p.Id,
					u.Id, 
					u.ImageUrl,
					p.PostImageUrl, 
					u.Username, 
					p.Description, 
					p.Likes, 
					p.PostedDate

				ORDER BY p.PostedDate DESC

                ";
		public static readonly string SetPostAsDeleted
			= @"UPDATE Post SET IsDeleted = 1 
                WHERE Id = @postId AND UserId = @userId";

        public static readonly string GetPostsAsAnonymous =
            @"SELECT
	            p.Id, 
	            u.Id AS PostUserId, 
	            u.ImageUrl as AuthorImageUrl,
	            p.PostImageUrl, 
	            u.Username, 
	            p.Description, 
	            p.Likes as PostLikes, 
	            p.PostedDate,
	            COUNT(c.Id) AS CommentsNumber
	            FROM Post p
		            JOIN Users u ON p.UserId = u.Id
		            LEFT JOIN Comment c ON c.PostId = p.Id 
									AND c.IsDeleted = 0
	            WHERE p.IsDeleted = 0
	            GROUP BY
		            p.Id, 
		            u.Id, 
		            u.ImageUrl,
		            p.PostImageUrl, 
		            u.Username, 
		            p.Description, 
		            p.Likes, 
		            p.PostedDate
	            ORDER BY p.PostedDate DESC
                ";
		public static readonly string RevertLikedPostData
			= @"
                DELETE FROM UserLikedPosts WHERE UserId = @userId AND PostId = @postId;

                UPDATE Post SET Likes = Likes - 1 WHERE Id = @postId;

                DELETE FROM Notifications WHERE 
											RecipientUserId = @postUserId
                                            AND ActorId = @userId
											AND PostId = @postId;

                ";
		public static readonly string SetLikedPostData
			= @"
                INSERT INTO UserLikedPosts (UserId, PostId) VALUES (@userId, @postId);
                    
                UPDATE Post SET Likes = Likes + 1 WHERE Id = @postId;
                    
                IF @userId != @postUserId
                    BEGIN
						INSERT INTO Notifications (
	                        RecipientUserId, ActorId, Type, PostId,
	                        Status, IsRead, CreatedAt
                        ) OUTPUT inserted.Id VALUES (
	                        @postUserId, @userId, 'PostLiked', @postId, 
	                        NULL, DEFAULT, DEFAULT
                        )
                    END
                ";
        public static readonly string GetUserPosts
            = @"
				SELECT
				p.Id, 
				u.Id AS PostUserId, 
				u.ImageUrl as AuthorImageUrl,
				p.PostImageUrl, 
				u.Username, 
				p.Description, 
				p.Likes as PostLikes, 
				p.PostedDate,
				CASE WHEN EXISTS (
					SELECT 1 FROM UserLikedPosts x 
					WHERE x.PostId = p.Id
					AND x.UserId = @userId
				) THEN 1 ELSE 0 END AS IsLikedByUser,
				COUNT(c.Id) AS CommentsNumber
				FROM Post p
					JOIN Users u ON p.UserId = u.Id
					LEFT JOIN Comment c ON c.PostId = p.Id
										AND c.IsDeleted = 0
				WHERE p.UserId = @otherUserProfileId AND p.IsDeleted = 0
				GROUP BY
					p.Id, 
					u.Id, 
					u.ImageUrl,
					p.PostImageUrl, 
					u.Username, 
					p.Description, 
					p.Likes, 
					p.PostedDate
				ORDER BY p.PostedDate DESC				

                ";
		public static readonly string GetUserPostsAsAnonymous =
            @"
				SELECT
				p.Id, 
				u.Id AS PostUserId, 
				u.ImageUrl as AuthorImageUrl,
				p.PostImageUrl, 
				u.Username, 
				p.Description, 
				p.Likes as PostLikes, 
				p.PostedDate,
				COUNT(c.Id) AS CommentsNumber
				FROM Post p
					JOIN Users u ON p.UserId = u.Id
					LEFT JOIN Comment c ON c.PostId = p.Id
										AND c.IsDeleted = 0
				WHERE p.UserId = @otherUserProfileId AND p.IsDeleted = 0
				GROUP BY
					p.Id, 
					u.Id, 
					u.ImageUrl,
					p.PostImageUrl, 
					u.Username, 
					p.Description, 
					p.Likes, 
					p.PostedDate
				ORDER BY p.PostedDate DESC";
        public static readonly string GetSinglePost // <== Cuando el usuario clickea notification sobre post (like, comment, etc)
            = @"SELECT
				p.Id, 
				u.Id AS PostUserId, 
				u.ImageUrl as AuthorImageUrl,
				p.PostImageUrl, 
				u.Username, 
				p.Description, 
				p.Likes as PostLikes, 
				p.PostedDate,
				CASE WHEN EXISTS (
					SELECT 1 FROM UserLikedPosts x 
					WHERE x.PostId = p.Id
					AND x.UserId = @userId
				) THEN 1 ELSE 0 END AS IsLikedByUser,
				COUNT(c.Id) AS CommentsNumber
				FROM Post p
					JOIN Users u ON p.UserId = u.Id
					LEFT JOIN Comment c ON c.PostId = p.Id
										AND c.IsDeleted = 0
				WHERE p.Id = @postId

				GROUP BY
					p.Id, 
					u.Id, 
					u.ImageUrl,
					p.PostImageUrl, 
					u.Username, 
					p.Description, 
					p.Likes, 
					p.PostedDate
                ";
        public static readonly string AddPost
            = @"INSERT INTO Post(PostImageUrl,Description,UserId, PostedDate) VALUES (@PostImageUrl, @Description, @UserId, GETDATE())";
        public static readonly string DeletePostValidations
            = @"SELECT 
                CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = @userId AND IsDisabled = 0) 
                THEN 1 ELSE 0 END AS UserExists,

                CASE WHEN EXISTS (SELECT 1 FROM Post 
                WHERE Id = @postId AND IsDeleted = 0 AND UserId = @userId) THEN 1 ELSE 0
                END AS PostExists;
                ";
        public static readonly string LikePostValidations
            = @"

                SELECT 

	            --Verificar que el post exista.
	            CASE WHEN EXISTS (SELECT 1 FROM Post WHERE Id = @postId AND IsDeleted = 0)
	            THEN 1 ELSE 0 
	            END AS PostExists,

	            --Verificar que el usuario exista.
	            CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = @userId AND IsDisabled = 0)
	            THEN 1 ELSE 0
	            END AS UserExists,

	            --Verificar que el usuario no tenga al otro usuario bloqueado
                CASE WHEN EXISTS
                (SELECT 1 FROM UserBlackList WHERE UserId = @userId
                AND OtherUserId = @postUserId) THEN 1 ELSE 0
                END AS IsBlockedByUser,

	            --Verificar que el otro usuario no tenga bloqueado al usuario
	            CASE WHEN EXISTS
	            (SELECT 1 FROM UserBlackList WHERE UserId = @postUserId
	            AND OtherUserId = @userId) THEN 1 ELSE 0
	            END AS IsBlockedByOtherUser,

                --Verificar que el usuario ya dio like a la publicación
	            CASE WHEN EXISTS
	            (SELECT 1 FROM UserLikedPosts WHERE UserId = @userId
								            AND PostId = @postId)
	            THEN 1 ELSE 0
	            END AS PostAlreadyLiked
                

            ";
        
    }
}
