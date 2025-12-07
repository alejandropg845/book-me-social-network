using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace BookMeServer.Hubs
{
    public class NotificationsHub : Hub
    {
        private readonly INotificationsService _connectionRepo;
        public NotificationsHub(INotificationsService hubConnection, IConfiguration config)
        {
            _connectionRepo = hubConnection;
        }

        public override Task OnConnectedAsync()
        {
            string? userId = Context.GetHttpContext()!.Request.Query["userId"]!;

            _connectionRepo.SaveUserConnection(userId.ToString(), Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.GetHttpContext()!.Request.Query["userId"]!;
            _connectionRepo.RemoveDisconnectedUser(userId.ToString());
            return base.OnDisconnectedAsync(exception);
        }

    }
}