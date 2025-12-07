using BookMeServer.DTOs.FollowRequest;
using BookMeServer.Models;
using BookMeServer.Responses.FollowRequestResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task MarkAllNotificationsAsRead(string userId);
        Task MarkSingleNotificationAsRead(int nId);
        Task<SendFollowRequestResponse> SendFollowRequest(string actorId, string recipientId);
        //Task<RespondToUserFollowRequestResponse> RespondToUserFollowRequest(string recipientUserId, int actorId, int notificationId, bool isAccepted);
        Task RejectFRAsync(int recipientId, int actorId);
    }
}
