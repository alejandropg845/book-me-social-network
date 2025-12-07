using BookMeServer.Interfaces.Services;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Services.RealTime
{
    public class NotificationsService : INotificationsService
    {
        private readonly Dictionary<string, string> _userConnections = new();
        private readonly string _stringConnection;
        public NotificationsService(IConfiguration config)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;

        }
        public void SaveUserConnection(string userId, string connectionId)
        => _userConnections.TryAdd(userId, connectionId);
            
        
        public string? GetUserConnection(string userId)
        {
            _userConnections.TryGetValue(userId, out string? connectionId);

            return connectionId;
        }
        public async void RemoveDisconnectedUser(string userId)
        {
            _userConnections.TryGetValue(userId, out string? connectionId);

            if (connectionId is not null)
            {
                _userConnections.Remove(userId);

                bool ok = false;
                while(ok is not true)
                    ok = await SetUserStatus(userId);
            }
        }

        public async Task<bool> SetUserStatus(string userId)
        {
            const string query = 
                @"UPDATE Users SET Status = 'offline'
                WHERE Id = @userId";
            
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            await connection.ExecuteAsync(query, new { userId });

            return true;
        }
    }
}
