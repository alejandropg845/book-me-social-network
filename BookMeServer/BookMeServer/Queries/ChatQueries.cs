namespace BookMeServer.Queries
{
    public static class ChatQueries
    {
        public static readonly string GetChats
            = @"

                WITH NumberedMessages AS (
                    SELECT
                        cm.ChatId,
                        cm.Message,
                        cm.UserId,
                        ROW_NUMBER() OVER (PARTITION BY cm.ChatId ORDER BY cm.SentAt DESC) AS RowNumber
                    FROM ChatMessage cm
                )

                SELECT 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id as OtherUserId,

                    MAX(nm.Message) AS LastChatMessage,

                    MAX(nm.UserId) AS LastMessageUserId,

                    COUNT(DISTINCT CASE WHEN cm.IsMarkedAsRead = 0 AND cm.UserId = c.UserId THEN cm.Id END) AS NoReadMessages,
                    
                    COUNT(DISTINCT cm.Id) AS MessagesNumber,

                    (CASE WHEN EXISTS (SELECT 1 FROM UserBlackList x WHERE x.UserId = @currentUserId AND x.OtherUserId = u.Id) THEN 1 ELSE 0 END) AS IsBlockedByUser
                
                FROM Chat c 

	                JOIN Users u ON c.UserId = u.Id AND c.UserId != @currentUserId
	                    LEFT JOIN ChatMessage cm ON c.Id = cm.ChatId
	                    LEFT JOIN UserBlackList ubl ON ubl.UserId = c.UserId
	                    LEFT JOIN NumberedMessages nm ON nm.ChatId = c.Id AND nm.RowNumber = 1

                WHERE c.Id IN 
                    (SELECT Id FROM Chat WHERE UserId = @currentUserId)
	                AND u.Username LIKE @keyword +'%'
    
                GROUP BY 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id;
            ";
        public static readonly string SendMessageValidations
            = @"SELECT 

                    --Verificar que existe el usuario que envía

	                CASE 
		                WHEN EXISTS (SELECT 1 
                        FROM Users WHERE Id = @userId AND IsDisabled = 0) THEN 1 ELSE 0 
	                END AS SenderExists,

                    --Verificar que existe el usuario que recibe

                    CASE 
		                WHEN EXISTS (SELECT 1 
                        FROM Users WHERE Id = @OtherUserId AND IsDisabled = 0) THEN 1 ELSE 0 
	                END AS ReceiverExists,

                    --Verificar que existe el chat de ambos usuarios

	                CASE 
		                WHEN EXISTS (SELECT 1 
                        FROM Chat WHERE Id = @chatId)
		                THEN 1 ELSE 0
	                END AS ChatExists,

                    --Verificar que el usuario no bloqueó al otro usuario
                    
                    CASE WHEN EXISTS (SELECT 1 FROM UserBlackList WHERE UserId = @userId
					  AND OtherUserId = @OtherUserId) THEN 1 ELSE 0
	                END AS IsBlockedByUser,

                    --Verificar que el otro usuario no bloqueó al usuario actual

	                CASE WHEN EXISTS (SELECT 1 FROM UserBlackList WHERE UserId = @OtherUserId
					                  AND OtherUserId = @userId) THEN 1 ELSE 0
	                END AS IsBlockedByOtherUser;
                   ";
        public static readonly string UpdatedCurrentUserChat
            = @"
                WITH NumberedMessages AS (
                    SELECT
                        cm.ChatId,
                        cm.Message,
                        cm.UserId,
                        ROW_NUMBER() OVER(PARTITION BY cm.ChatId ORDER BY cm.SentAt DESC) AS RowNumber
                    FROM ChatMessage cm
                )

                SELECT 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id as OtherUserId,
                    MAX(nm.Message) AS LastChatMessage,
                    MAX(nm.UserId) AS LastMessageUserId,
                    COUNT(DISTINCT CASE WHEN cm.IsMarkedAsRead = 0 AND cm.UserId = c.UserId THEN cm.Id END) AS NoReadMessages,
                    COUNT(DISTINCT cm.Id) AS MessagesNumber,
                    (CASE WHEN EXISTS (SELECT 1 FROM UserBlackList x WHERE x.UserId = @userId AND x.OtherUserId = u.Id) THEN 1 ELSE 0 END) AS IsBlockedByUser
                FROM Chat c 
	                JOIN Users u ON c.UserId = u.Id AND c.UserId != @userId
	                LEFT JOIN ChatMessage cm ON c.Id = cm.ChatId
	                LEFT JOIN UserBlackList ubl ON ubl.UserId = c.UserId
	                LEFT JOIN NumberedMessages nm ON nm.ChatId = c.Id AND nm.RowNumber = 1
                WHERE c.Id = @chatId
    
                GROUP BY 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id;
                ";
        public static readonly string UpdatedOtherUserChat
            = @"
                WITH NumberedMessages AS (
                    SELECT
                        cm.ChatId,
                        cm.Message,
                        cm.UserId,
                        ROW_NUMBER() OVER(PARTITION BY cm.ChatId ORDER BY cm.SentAt DESC) AS RowNumber
                    FROM ChatMessage cm
                )

                SELECT 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id as OtherUserId,
                    MAX(nm.Message) AS LastChatMessage,
                    MAX(nm.UserId) AS LastMessageUserId,
                    COUNT(DISTINCT CASE WHEN cm.IsMarkedAsRead = 0 AND cm.UserId = c.UserId THEN cm.Id END) AS NoReadMessages,
                    COUNT(DISTINCT cm.Id) AS MessagesNumber,
                    (CASE WHEN EXISTS (SELECT 1 FROM UserBlackList x WHERE x.UserId = @otherUserId AND x.OtherUserId = u.Id) THEN 1 ELSE 0 END) AS IsBlockedByUser
                FROM Chat c 
	                JOIN Users u ON c.UserId = u.Id AND c.UserId != @otherUserId
	                LEFT JOIN ChatMessage cm ON c.Id = cm.ChatId
	                LEFT JOIN UserBlackList ubl ON ubl.UserId = c.UserId
	                LEFT JOIN NumberedMessages nm ON nm.ChatId = c.Id AND nm.RowNumber = 1
                WHERE c.Id = @chatId
    
                GROUP BY 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id;

                ";
        public static readonly string GetChatMessages
            = @"
                SELECT *
                FROM ChatMessage 
                WHERE ChatId = @chatId
                ORDER BY SentAt DESC 
                OFFSET (@number * 30) ROWS 
                FETCH NEXT 30 ROWS ONLY;";

        public static readonly string MarkChatMessagesAsRead
            = @"UPDATE ChatMessage SET IsMarkedAsRead = 1
                WHERE ChatId = @chatId
                AND UserId != @userId;
                ";
        public static readonly string GetUpdatedChat
            = @"
                WITH NumberedMessages AS (
                    SELECT
                        cm.ChatId,
                        cm.Message,
                        cm.UserId,
                        ROW_NUMBER() OVER(PARTITION BY cm.ChatId ORDER BY cm.SentAt DESC) AS RowNumber
                    FROM ChatMessage cm
                )

                SELECT 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id as OtherUserId,
                    MAX(nm.Message) AS LastChatMessage,
                    MAX(nm.UserId) AS LastMessageUserId,
                    COUNT(DISTINCT CASE WHEN cm.IsMarkedAsRead = 0 AND cm.UserId = c.UserId THEN cm.Id END) AS NoReadMessages,
                    COUNT(DISTINCT cm.Id) AS MessagesNumber,
                    (CASE WHEN EXISTS (SELECT 1 FROM UserBlackList x WHERE x.UserId = @userId AND x.OtherUserId = u.Id) THEN 1 ELSE 0 END) AS IsBlockedByUser
                FROM Chat c 
	                JOIN Users u ON c.UserId = u.Id AND c.UserId != @userId
	                LEFT JOIN ChatMessage cm ON c.Id = cm.ChatId
	                LEFT JOIN UserBlackList ubl ON ubl.UserId = c.UserId
	                LEFT JOIN NumberedMessages nm ON nm.ChatId = c.Id AND nm.RowNumber = 1
                WHERE c.Id = @chatId
    
                GROUP BY 
                    c.Id,
                    u.Username,
                    u.ImageUrl,
                    u.Id;

            ";
        public static readonly string MarkedAsReadMessage
            = @"INSERT INTO ChatMessage (ChatId, Message, UserId, IsMarkedAsRead) 
                    OUTPUT INSERTED.* VALUES (@ChatId, @Message, @userId, 1)";
        public static readonly string NotMarkedAsReadMessage
            = @"INSERT INTO ChatMessage (ChatId, Message, UserId) 
                    OUTPUT INSERTED.* VALUES (@ChatId, @Message, @userId)";

    }
}
