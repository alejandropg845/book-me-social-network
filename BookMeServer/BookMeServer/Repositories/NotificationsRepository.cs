using BookMeServer.Interfaces.Repositories.Notifications;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.FollowRequestResponses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Repositories
{
    public class NotificationsRepository : INotificationsWriteRepository, INotificationsReadRepository
    {
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(SqlConnection connection, string userId)
        {
			var notifications = await connection.QueryAsync<Notification>(
				NotificationQueries.GetUserNotifications, new { userId });

			return notifications;
        }

		public async Task MarkAllNotificationsAsRead(SqlConnection connection, string userId)
		{ 
			await connection.ExecuteAsync(
				NotificationQueries.MarkAllNotificationsAsRead, 
				new { userId }
			);
		}

		public async Task MarkSingleNotificationAsRead(SqlConnection connection, int notificationId)
		{
			await connection.ExecuteAsync(
				NotificationQueries.MarkSingleNotificationAsRead, 
				new { notificationId }
			);
		}
        public async Task<SendFollowRequestResponse> ValidateSendFollowRequestAsync(SqlConnection connection, string userReceiverId, string userSenderId)
        {
            var (SenderExists,
                ReceiverExists,
                AlreadyInUserFollowers,
                SenderAlreadySentFR,
                SenderIsAlreadyReceiver,
                IsBlockedByUser,
                IsBlockedByOtherUser,
                ChatAlreadyExists) =
                await connection.QuerySingleAsync
                <(bool SenderExists,
                bool ReceiverExists,
                bool AlreadyInUserFollowers,
                bool SenderAlreadySentFR,
                bool SenderIsAlreadyReceiver,
                bool IsBlockedByUser,
                bool IsBlockedByOtherUser,
                bool ChatAlreadyExists)>
                  (FRQueries.SendFRValidations, new { userReceiverId, userSenderId });

            return new SendFollowRequestResponse
            {
                SenderExists = SenderExists,
                ReceiverExists = ReceiverExists,
                AlreadyFriends = AlreadyInUserFollowers,
                FollowRequestAlreadyExists = SenderAlreadySentFR,
                SenderIsAlreadyReceiver = SenderIsAlreadyReceiver,
                IsBlockedByUser = IsBlockedByUser,
                IsBlockedByOtherUser = IsBlockedByOtherUser,
                ChatExists = ChatAlreadyExists
            };
        }
        public async Task<int> AddAcceptedFollowRequestDataAsync(SqlConnection connection, IDbTransaction transaction, string recipientId, string actorId, Guid chatId)
        {
            int notificationId = await connection
            .QuerySingleAsync<int>(
                FRQueries.AddAcceptedFollowRequestData,
                new { recipientId, actorId, chatId },
                transaction
            );

            return notificationId;
        }
        public async Task<int> SendFollowRequestAsync(SqlConnection connection, string userSenderId, string userReceiverId)
        {
            int notificationId = await connection.QueryFirstAsync<int>(
                FRQueries.SendFollowRequest,
                new { userSenderId, userReceiverId }
            );

            return notificationId;
        }
        public async Task<RespondToUserFollowRequestResponse> ValidateRespondUserFollowRequestAsync(SqlConnection connection, string userReceiverId, string userSenderId)
        {
            var (SenderExists,
                ReceiverExists,
                FollowRequestExists,
                IsBlockedByUser,
                IsBlockedByOtherUser,
                ChatExists) = await connection.QuerySingleAsync
                <(
                    bool SenderExists,
                    bool ReceiverExists,
                    bool FollowRequestExists,
                    bool IsBlockedByUser,
                    bool IsBlockedByOtherUser,
                    bool ChatExists
                )>
                (
                    FRQueries.RespondToUserFRValidations,
                    new { userReceiverId, userSenderId }
                );

            return new RespondToUserFollowRequestResponse
            {
                ChatExists = ChatExists,
                FollowRequestExists = FollowRequestExists,
                IsBlockedByUser = IsBlockedByUser,
                IsBlockedByOtherUser = IsBlockedByOtherUser,
                ReceiverExists = ReceiverExists,
                SenderExists = SenderExists
            };
        }
        public async Task<int> SetAcceptedResponseFRDataAsync(SqlConnection connection, IDbTransaction transaction, string userSenderId, string userReceiverId, Guid chatId)
        {

            int notificationId = await connection
                .QuerySingleAsync<int>(
                FRQueries.SetAcceptedResponseFRData,
                new { chatId, userSenderId, userReceiverId },
                transaction
                );

            return notificationId;
        }

        public async Task RejectFRAsync(SqlConnection conn, int recipientId, int actorId)
        => await conn.ExecuteAsync(FRQueries.IsFRrejected, new { recipientId, actorId });
    }
}
