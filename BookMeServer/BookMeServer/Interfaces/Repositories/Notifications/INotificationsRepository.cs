using BookMeServer.Models;
using BookMeServer.Responses.FollowRequestResponses;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Notifications
{
    public interface INotificationsRepository
    {
        Task MarkAllNotificationsAsRead(SqlConnection connection, string userId);
        Task MarkSingleNotificationAsRead(SqlConnection connection, int notificationId);
        Task RejectFRAsync(SqlConnection conn, int recipientId, int actorId);
        Task<int> AddAcceptedFollowRequestDataAsync(SqlConnection connection, DbTransaction transaction, string userReceiverId, string userSenderId, Guid chatId);
        Task<int> SendFollowRequestAsync(SqlConnection connection, string userSenderId, string userReceiverId);
        Task<int> SetAcceptedResponseFRDataAsync(SqlConnection connection, DbTransaction transaction, string userSenderId, string userReceiverId, Guid chatId);
    }
}
