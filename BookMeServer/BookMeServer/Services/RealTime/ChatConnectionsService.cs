using BookMeServer.Interfaces.Services;
using System.Collections.Generic;

namespace BookMeServer.Services.RealTime
{
    public class ChatConnectionsService : IChatsConnectionsService
    {
        private readonly Dictionary<string,string> _userConnection = new();
        private readonly Dictionary<string,string> _usersWithChatOpen = new();
        public void SaveUserConnection(string userId, string connectionId)
        => _userConnection.TryAdd(userId, connectionId);
        
        public string? GetUserConnectionId(string userId)
        {
            if (_userConnection.ContainsKey(userId))
            
                return _userConnection[userId];
            

            return null;
        }
        public void RemoveUserConnection(string userId)
        => _userConnection.Remove(userId);

        public void RemoveFromChatOpenWithNoChatId(string userId)
        {
           var key = 
                _usersWithChatOpen
                .Where(c => c.Key.Contains($":{userId}"))
                .FirstOrDefault().Key;

            if(key is not null)
            _usersWithChatOpen.Remove(key);


        }

        public string GetUserConnectionForChatOpen(int userId, string chatId)
        {
            if(_usersWithChatOpen.ContainsKey($"{chatId}:{userId}"))
            {
   
                return _usersWithChatOpen[$"{chatId}:{userId}"];
            }

            return null! ;
        }

        public void RemoveUserIdFromChatOpen(int userId, string chatId)
        => _usersWithChatOpen.Remove($"{chatId}:{userId}");
        

        public void SaveUserIdChatOpen(int userId, string chatId, string connectionId) 
        { 
            _usersWithChatOpen.TryAdd($"{chatId}:{userId}", connectionId);
        }
        
    }
}
