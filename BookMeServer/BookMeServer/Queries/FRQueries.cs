namespace BookMeServer.Queries
{
    public static class FRQueries
    {
        public static readonly string SendFRValidations
            = @"
					SELECT
				CASE WHEN EXISTS
					(SELECT 1 FROM Users WHERE Id = @userSenderId AND IsDisabled = 0)
					THEN 1 ELSE 0
				END AS SenderExists,

				CASE WHEN EXISTS
					(SELECT 1 FROM Users WHERE Id = @userReceiverId AND IsDisabled = 0)
					THEN 1 ELSE 0
				END AS ReceiverExists,

				CASE WHEN EXISTS
					(SELECT 1 FROM UserFollowers
					WHERE UserId = @userReceiverId 
					AND FollowerId = @userSenderId)
					THEN 1 ELSE 0
				END AS AlreadyInUserFollowers,

				CASE WHEN EXISTS
					(SELECT 1 FROM Notifications
					WHERE ActorId = @userSenderId
					AND RecipientUserId = @userReceiverId
					AND Type = 'FollowRequest'
					)
					THEN 1 ELSE 0
				END AS SenderAlreadySentFR,

				CASE WHEN EXISTS
					(SELECT 1 FROM Notifications 
					WHERE RecipientUserId = @userSenderId 
					AND ActorId = @userReceiverId
					AND Type = 'FollowRequest'
					)
					THEN 1 ELSE 0
				END AS UserSenderIsReceiver,

				--Verificar que el usuario no tenga al otro usuario bloqueado
					CASE WHEN EXISTS
					(SELECT 1 FROM UserBlackList WHERE UserId = @userSenderId
					AND OtherUserId = @userReceiverId) THEN 1 ELSE 0
					END AS IsBlockedByUser,

				--Verificar que el otro usuario no tenga bloqueado al usuario
				CASE WHEN EXISTS
				(SELECT 1 FROM UserBlackList WHERE UserId = @userReceiverId
				AND OtherUserId = @userSenderId) THEN 1 ELSE 0
				END AS IsBlockedByOtherUser,

				--Verificar que ya se siguen o que contienen un chat
				CASE WHEN EXISTS (SELECT 1 FROM UserFollowers WHERE UserId = @userSenderId
				AND FollowerId = @userReceiverId) THEN 1 ELSE 0 END AS ChatExists
				;

			";
		public static readonly string AddAcceptedFollowRequestData
			= @"-- Aceptar la Follow Request
				DELETE FROM Notifications
				WHERE ActorId = @recipientId 
					AND RecipientUserId = @actorId
					AND Type = 'FollowRequest';

                -- Crear chat para sender
                INSERT INTO Chat VALUES (@chatId, @actorId);
                -- Crear chat para receiver
                INSERT INTO Chat VALUES (@chatId, @recipientId);

                -- Agregar a followers 
                INSERT INTO UserFollowers (UserId, FollowerId)
                                VALUES (@recipientId, @actorId);

                INSERT INTO UserFollowers (UserId, FollowerId)
                                VALUES (@actorId, @recipientId);

                /* El que envió la FR es el que recibirá la notificación
                de que su solicitud fue aceptada */

                INSERT INTO Notifications (
					ActorId, PostId, RecipientUserId, 
					Status, Type, IsRead, CreatedAt
				) OUTPUT inserted.Id VALUES (
					@recipientId, NULL, @actorId,
					'Accepted', 'FollowRequest', 0, DEFAULT
				)
				";
		public static readonly string SetAcceptedResponseFRData
			= @"
                UPDATE Notifications
					SET Status = 'Accepted', 
					IsRead = 1 
					OUTPUT inserted.Id
                WHERE ActorId = @actorId 
                AND RecipientUserId = @recipientId
				AND Type = 'FollowRequest';

                INSERT INTO Chat VALUES (@chatId, @actorId);

                INSERT INTO Chat VALUES (@chatId, @recipientId);

                INSERT INTO UserFollowers (UserId, FollowerId)
                                VALUES (@recipientId, @actorId);

                INSERT INTO UserFollowers (UserId, FollowerId)
                                VALUES (@actorId, @recipientId);
                ";


        public static readonly string RespondToUserFRValidations
			= @"
                SELECT 

	            --Verificar que existe el que envía la FR
	            CASE WHEN EXISTS (SELECT 1 FROM 
					              Users WHERE Id = @userSenderId AND IsDisabled = 0) THEN 1 ELSE 0
	            END AS SenderExists,

	            --Verificar que existe el que recibe la FR

	            CASE WHEN EXISTS (SELECT 1 FROM 
					              Users WHERE Id = @userReceiverId AND IsDisabled = 0) THEN 1 ELSE 0
	            END AS ReceiverExists,

	            --Verificar que existe la petición de seguimiento

	            CASE WHEN EXISTS (SELECT 1 FROM Notifications 
                                WHERE ActorId = @userSenderId
                            AND RecipientUserId = @userReceiverId
							AND Type = 'FollowRequest'
							) THEN 1 ELSE 0
	            END AS FollowRequestExists,

	            --Verificar que el usuario no bloqueó al otro usuario

	            CASE WHEN EXISTS (SELECT 1 FROM UserBlackList WHERE UserId = @userSenderId
	              AND OtherUserId = @userReceiverId) THEN 1 ELSE 0
	            END AS IsBlockedByUser,

	            --Verificar que el otro usuario no bloqueó al usuario actual

	            CASE WHEN EXISTS (SELECT 1 FROM UserBlackList WHERE UserId = @userSenderId
					              AND OtherUserId = @userReceiverId) THEN 1 ELSE 0
	            END AS IsBlockedByOtherUser,

				--Verificar que ya son followers ambos
				CASE WHEN EXISTS (SELECT 1 FROM UserFollowers WHERE UserId = @userSenderId
				AND FollowerId = @userReceiverId) THEN 1 ELSE 0 END AS ChatExists
                
                ";
		public static readonly string IsFRrejected
			= @"DELETE FROM Notifications
                WHERE ActorId = @actorId 
                AND RecipientUserId = @recipientId
				AND Type = 'FollowRequest'";
		public static readonly string SendFollowRequest
			= @"
				 INSERT INTO Notifications (
					ActorId, PostId, RecipientUserId,
					Status, Type, IsRead, CreatedAt
				) OUTPUT INSERTED.Id VALUES (
					@userSenderId, NULL, @userReceiverId,
					'Pending', 'FollowRequest', 0, DEFAULT
				)
			";
    }
}
