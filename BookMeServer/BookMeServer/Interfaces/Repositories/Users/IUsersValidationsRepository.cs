using BookMeServer.Responses.UserResponses;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Interfaces.Repositories.Users
{
    public interface IUsersValidationsRepository
    {
        Task<LoginUserResponse> ValidateLoginUserAsync(SqlConnection connection, string username);
        Task<CreateUserResponse> ValidateRegisterUserAsync(SqlConnection connection, string username);
        Task<bool> IsBlockedByUserAsync(SqlConnection connection, string userId, int otherUserId);
        Task<bool> UserExistsAsync(SqlConnection connection, string userId);

    }
}
