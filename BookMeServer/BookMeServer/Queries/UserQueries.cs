namespace BookMeServer.Queries
{
    public static class UserQueries
    {
        public static readonly string ValidateLoginUser
            = @"SELECT Username, Password, Id, RoleName FROM 
                Users WHERE Username = @username;

                SELECT 1 FROM Users WHERE Username = @username
                AND IsDisabled = 1;";
        public static readonly string AddUser
            = @"INSERT INTO Users (Username, ImageUrl, Password, Status, IsDisabled, RoleName) 
                    OUTPUT INSERTED.Id, INSERTED.RoleName
                VALUES (@Username, NULL, @Password, 'offline', DEFAULT, DEFAULT)";
        public static readonly string BlockUser
            = @"INSERT INTO 
                    UserBlackList (UserId, OtherUserId)
			    VALUES (@userId, @otherUserId);";
        public static readonly string UnlockUser
            = @"DELETE FROM
                 UserBlackList WHERE UserId = @userId
                 AND OtherUserId = @otherUserId;";
        public static readonly string RegisterUserValidations
            = @"SELECT 
                  CASE WHEN EXISTS (SELECT Username FROM Users 
                  WHERE Username = @Username) THEN 1 ELSE 0 END AS UserExists";
        public static readonly string GetUserCredentialsForSidebar
            = @"

            UPDATE Users SET Status = 'online'
            WHERE Id = @userId;

            SELECT Id, Username, ImageUrl, Status 
            FROM Users WHERE Id = @userId;";
        public static readonly string GetFilteredUsers
            = @"SELECT Username, 
	                ImageUrl,
	                Status, 
	                Id 
                FROM Users WHERE Username LIKE @Username + '%'
		            AND Username NOT LIKE 'adm1n845'";
        public static readonly string GetOtherUserCredentialsValidations
            = @"SELECT

                  --Verificar que exista una FR previa.

                  CASE WHEN EXISTS (
                    SELECT 1 FROM Notifications WHERE ActorId = @userProfileId 
                    AND Type = 'FollowRequest'
                    AND RecipientUserId = @currentUserId
                  ) 
                  THEN 1 ELSE 0 END AS OtherUserSentFR,


                  --Verificar que exista una FR por el usuario actual

                  CASE WHEN EXISTS (
                  SELECT 1 FROM Notifications WHERE ActorId = @currentUserId
                  AND RecipientUserId = @userProfileId
                  AND Type = 'FollowRequest'
                  ) THEN 1 ELSE 0
                  END AS CurrentUserAlreadySentFR,

                  --Verificar que ambos usuarios se siguen

                  CASE WHEN EXISTS (SELECT 1 FROM UserFollowers 
                  WHERE UserId = @currentUserId
                  AND FollowerId = @userProfileId) THEN 1 ELSE 0
                  END AS BothUsersFollow,


                  --Verificar que el usuario actual bloqueó al otro usuario

                  CASE WHEN EXISTS (SELECT 1 AS IsBlockedByUser 
                  FROM UserBlackList WHERE UserId = @currentUserId 
                  AND OtherUserId = @userProfileId) THEN 1 ELSE 0
                  END AS UserIsBlocked,

                  --Verificar que el otro usuario bloqueó al actual

                CASE WHEN EXISTS (SELECT 1 AS IsBlockedByUser 
                FROM UserBlackList WHERE UserId = @userProfileId 
                AND OtherUserId = @currentUserId) THEN 1 ELSE 0
                END AS CurrentUserIsBlocked;

            ";
    }
}
