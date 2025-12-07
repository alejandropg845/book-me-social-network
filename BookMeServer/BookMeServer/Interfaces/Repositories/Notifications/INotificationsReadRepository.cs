using BookMeServer.Models;
using BookMeServer.Responses.FollowRequestResponses;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Interfaces.Repositories.Notifications
{
    public interface INotificationsReadRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(SqlConnection connection, string userId);
        Task<SendFollowRequestResponse> ValidateSendFollowRequestAsync(SqlConnection connection, string userReceiverId, string userSenderId);
        Task<RespondToUserFollowRequestResponse> ValidateRespondUserFollowRequestAsync(SqlConnection connection, string userReceiverId, string userSenderId);

    }
}
