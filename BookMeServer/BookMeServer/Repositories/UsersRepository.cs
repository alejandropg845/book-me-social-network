using Azure.AI.ContentSafety;
using Azure;
using BookMeServer.DTOs.Auth;
using BookMeServer.DTOs.User;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.UserResponses;
using Dapper;
using Microsoft.Data.SqlClient;
using BookMeServer.Services.Media;
using System.Data.Common;
using BookMeServer.Interfaces.Repositories.Users;
using System.Data;

namespace BookMeServer.Repositories
{
    public class UsersRepository : IUsersWriteRepository, IUsersReadRepository, IUsersValidationsRepository
    {

        public async Task<LoginUserResponse> ValidateLoginUserAsync(SqlConnection connection, string username)
        {
            var multi_query = await connection.QueryMultipleAsync(
                UserQueries.ValidateLoginUser,
                new { username }
            );

            User? user = (await multi_query.ReadAsync<User>()).FirstOrDefault();
            bool isUserDisabled = (await multi_query.ReadAsync<bool>()).FirstOrDefault();

            return new LoginUserResponse
            {
                IsDisabled = isUserDisabled,
                User = user
            };
        }

        public async Task KeepAliveAsync(SqlConnection connection)
        {
            string query = @"SELECT 1 FROM Users WHERE Id = 0";

            await connection.QueryFirstOrDefaultAsync(query);
        }
        public async Task<CreateUserResponse> ValidateRegisterUserAsync(SqlConnection connection, string username)
        {
            var userNameExists = await connection.ExecuteScalarAsync<bool>(
                UserQueries.RegisterUserValidations, 
                new
                {
                    Username = username.ToLower()
                }
            );

            return new CreateUserResponse
            {
                UsernameExists = userNameExists
            };

        }
        public async Task<(int Id, string RoleName)> AddUserAsync(SqlConnection connection, string username, string hashedPassword)
        {
            var x = await connection.QuerySingleAsync<(int Id, string RoleName)>(
                UserQueries.AddUser, 
                new
                {
                    Username = username.ToLower(),
                    Password = hashedPassword
                }
            );

            return new (x.Id, x.RoleName);
        }

        public async Task<bool> IsBlockedByUserAsync(SqlConnection connection, string userId, int otherUserId)
        {
            bool isBlockedByUser = await connection.ExecuteScalarAsync<bool>
                (
                    @"SELECT 1 FROM UserBlackList WHERE 
                    UserId = @userId AND OtherUserId = @otherUserId",
                    new { userId, otherUserId }
                );
            return isBlockedByUser;
        }

        public async Task<string> BlockUserAsync(SqlConnection connection, string userId, int otherUserId)
        {
            await connection.ExecuteAsync(UserQueries.BlockUser, new { userId, otherUserId });

            return "blocked";
        }
        public async Task<string> UnlockUserAsync(SqlConnection connection, string userId, int otherUserId)
        {

            await connection.ExecuteAsync(UserQueries.UnlockUser, new { userId, otherUserId });
            
            return "unlocked";
        }

        public async Task<User> GetUserCredentialsAsync(SqlConnection connection, IDbTransaction transaction, string userId)
        {
            var user = await connection
                .QueryFirstOrDefaultAsync<User>(
                    UserQueries.GetUserCredentialsForSidebar,
                    new { userId },
                    transaction
                );

            return user!;
        }

        public async Task<bool> UserExistsAsync(SqlConnection connection, string userId)
        {
            bool userExists = await connection.ExecuteScalarAsync<bool>(
                @"SELECT 1 FROM Users 
                WHERE Id = @userId AND IsDisabled = 0", 
                new { userId }
            );

            return userExists;
        }

        public async Task ChangeProfilePicAsync(SqlConnection connection, string userId, string profilePicUrl)
        {
            await connection.ExecuteAsync(
                @"UPDATE Users SET ImageUrl = @profilePicUrl WHERE Id = @userId",
                new
                {
                    profilePicUrl,
                    userId
                }
            );
        }

        public async Task<IEnumerable<UsersFilteringResponse>> FilterUsersAsync(SqlConnection connection, FilterUsersDto dto)
        {
            var users = await connection.QueryAsync<UsersFilteringResponse>(
                UserQueries.GetFilteredUsers, 
                new
                {
                    Username = dto.Username.ToLower()
                }
            );

            return users;
        }

        public async Task<OtherUserProfile> GetOtherUserProfileAsync(SqlConnection connection, string userProfileId)
        {
            var user = await connection.QueryFirstAsync<OtherUserProfile>(
                @"SELECT Id, Username, ImageUrl, Status 
                FROM Users WHERE Id = @userProfileId",
                new { userProfileId }
            );

            return user;
        }

        public async Task<OtherUserValidations> GetOtherUserValidations(SqlConnection connection, string currentUserId, string userProfileId)
        {
            var (OtherUserSentFR, 
                CurrentUserAlreadySentFR,
                BothUsersFollow, 
                UserIsBlocked, 
                CurrentUserIsBlocked) = await connection.QuerySingleAsync
                <(
                    bool OtherUserSentFR, 
                    bool CurrentUserAlreadySentFR,
                    bool BothUsersFollow, 
                    bool UserIsBlocked, 
                    bool CurrentUserIsBlocked)>
                (
                    UserQueries.GetOtherUserCredentialsValidations,
                    new { currentUserId, userProfileId }
                );

            return new OtherUserValidations
            {
                UserSentFollowRequest = OtherUserSentFR,
                CurrentUserAlreadySentFR = CurrentUserAlreadySentFR,
                BothUsersFollow = BothUsersFollow,
                UserIsBlocked = UserIsBlocked,
                CurrentUserIsBlocked = CurrentUserIsBlocked
            };
        }
        
    }
}
