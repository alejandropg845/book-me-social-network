using BookMeServer.DTOs.User;
using BookMeServer.Models;
using BookMeServer.Responses.UserResponses;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Users
{
    public interface IUsersReadRepository
    {
        Task KeepAliveAsync(SqlConnection connection);
        Task<User> GetUserCredentialsAsync(SqlConnection connection, IDbTransaction transaction, string userId);
        Task<IEnumerable<UsersFilteringResponse>> FilterUsersAsync(SqlConnection connection, FilterUsersDto dto);
        Task<OtherUserProfile> GetOtherUserProfileAsync(SqlConnection connection, string userProfileId);
        Task<OtherUserValidations> GetOtherUserValidations(SqlConnection connection, string currentUserId, string userProfileId);

    }
}
