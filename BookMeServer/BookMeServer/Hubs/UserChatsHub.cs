using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace BookMeServer.Hubs
{
    public class UserChatsHub : Hub
    {
        private readonly IChatsConnectionsService _userChatsService;
        public UserChatsHub(IChatsConnectionsService _u)
        {
            _userChatsService = _u;
        }

        public override Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext()!.Request.Query["userId"]!;

            if (_userChatsService.GetUserConnectionId(userId) == null)
            {
                _userChatsService.SaveUserConnection(userId, Context.ConnectionId);
            }

            return base.OnConnectedAsync();
        }

        public void OnOpenChat(int userId, string chatId)
        {
            _userChatsService.SaveUserIdChatOpen(userId, chatId, Context.ConnectionId);
        }

        public void OnCloseChat(int userId, string chatId)
        {
            _userChatsService.RemoveUserIdFromChatOpen(userId, chatId);
        }

        public async Task OnTypingMessage(int userId, int otherUserId, string message)
        {
            string? userConnectionId = _userChatsService.GetUserConnectionId(otherUserId.ToString());

            if (userConnectionId != null)
            {
                bool typing;

                if (string.IsNullOrEmpty(message))
                    typing = false;
                else 
                    typing = true;

                await Clients.Client(userConnectionId)
                    .SendAsync("ReceiveOnTypingMessage", userId, typing);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.GetHttpContext()!.Request.Query["userId"]!;

            if(_userChatsService.GetUserConnectionId(userId) != null)
            {
                _userChatsService.RemoveUserConnection(userId);
            }

            _userChatsService.RemoveFromChatOpenWithNoChatId(userId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
