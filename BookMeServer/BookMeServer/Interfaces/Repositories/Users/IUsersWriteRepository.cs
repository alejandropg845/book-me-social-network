using BookMeServer.DTOs.Auth;
using BookMeServer.DTOs.User;
using BookMeServer.Models;
using BookMeServer.Responses.UserResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Users
{
    public interface IUsersWriteRepository
    {
        Task<(int Id, string RoleName)> AddUserAsync(SqlConnection connection, string username, string hashedPassword);
        Task ChangeProfilePicAsync(SqlConnection connection, string userId, string profilePicUrl);
        Task<string> BlockUserAsync(SqlConnection connection, string userId, int otherUserId);
        Task<string> UnlockUserAsync(SqlConnection connection, string userId, int otherUserId);
    }
}
