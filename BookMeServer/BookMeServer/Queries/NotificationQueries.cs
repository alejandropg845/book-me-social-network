namespace BookMeServer.Queries
{
    public static class NotificationQueries
    {
        public static readonly string GetUserNotifications
            = @"
				SELECT
					n.Id,
					n.PostId,
					n.CreatedAt,
					u.Id AS ActorId,
					u.ImageUrl,
					u.Username,
					n.IsRead,
					n.Status,
					n.Type
				FROM Notifications n
				LEFT JOIN Users u
						ON n.ActorId = u.Id
				WHERE n.RecipientUserId = @userId
				ORDER BY n.CreatedAt DESC
				";
		public static readonly string MarkAllNotificationsAsRead
			= @"
				UPDATE Notifications SET IsRead = 1
				WHERE RecipientUserId = @userId;
				";

		public static readonly string MarkSingleNotificationAsRead
			= @"UPDATE Notifications 
				SET IsRead = 1 WHERE 
				Id = @notificationId";
    }
}
