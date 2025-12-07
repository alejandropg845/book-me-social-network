using Azure.AI.ContentSafety;
using Azure;
using BookMeServer.DTOs.Auth;
using BookMeServer.DTOs.User;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.UserResponses;
using BookMeServer.Services.Media;
using Dapper;
using Microsoft.Data.SqlClient;
using BookMeServer.Interfaces.Repositories.Users;

namespace BookMeServer.Services.App
{
    public class UserService : IUserService
    {
        private readonly string _stringConnection;
        private readonly string _cs_key;
        private readonly string _cs_endpoint;
        private readonly string _cloudinary_key;
        private readonly string _cloudinary_secret;
        private readonly IUsersWriteRepository _repoWrite;
        private readonly IUsersValidationsRepository _repoValidate;
        private readonly IUsersReadRepository _repoRead;

        public UserService(IConfiguration config, IUsersWriteRepository uwr, IUsersReadRepository urr, IUsersValidationsRepository uvr)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _cs_key = config["azure_info:content_safety_key"]!;
            _cs_endpoint = config["azure_info:content_safety_endpoint"]!;
            _cloudinary_key = config["cloudinary:api_key"]!;
            _cloudinary_secret = config["cloudinary:api_secret"]!;
            _repoWrite = uwr;
            _repoRead = urr;
            _repoValidate = uvr;
        }

        public async Task KeepAliveAsync()
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            await _repoRead.KeepAliveAsync(connection);
        }
        
        public async Task<LoginUserResponse> LoginUser(LoginDto dto)
        {

            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            var response = await _repoValidate.ValidateLoginUserAsync(connection, dto.Username);

            if (response.User is null) return response;

            bool isCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, response.User.Password);

            response.IsCorrect = isCorrect;

            if (!isCorrect) return response;

            if (response.IsDisabled) return response;

            return response;

        }

        public async Task<CreateUserResponse> RegisterUser(RegisterDto dto)
        {
            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            var response = await _repoValidate.ValidateRegisterUserAsync(connection, dto.Username);

            if (response.UsernameExists) return response;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var createdUserInfo = await _repoWrite
                .AddUserAsync(connection, dto.Username, hashedPassword);

            response.UserId = createdUserInfo.Id;
            response.RoleName = createdUserInfo.RoleName;
            return response;
        }
        public async Task<User> GetUserCredentialsForSidebarAndMyProfile(string userId)
        {
            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();
            try
            {
                // 👇 Este query también coloca el estado de online
                var user = await _repoRead.GetUserCredentialsAsync(connection, transaction, userId);
                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<ChangeProfilePicResponse> ChangeUserProfilePicAsync(string userId, ChangeProfilePicDto dto)
        {
            var response = new ChangeProfilePicResponse();

            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            bool userExists = await _repoValidate.UserExistsAsync(connection, userId);

            response.UserExists = userExists;
            if (!userExists) return response;

            bool isExplicitContent = await IsExplicitContent(dto.ProfilePicUrl);

            response.IsExplicitContent = isExplicitContent;
            if (isExplicitContent)
            {
                await dto.PublicId.DeleteImageFromCloudinary
                (
                    _cloudinary_key,
                    _cloudinary_secret
                );

                return response;
            }

            await _repoWrite.ChangeProfilePicAsync(connection, userId, dto.ProfilePicUrl);

            return response;
        }

        private async Task<bool> IsExplicitContent(string imageUrl)
        {
            var http = new HttpClient();

            byte[] imageBytes = await http.GetByteArrayAsync(imageUrl);


            ContentSafetyClient contentSafetyClient =
                new ContentSafetyClient
                (
                    new Uri(_cs_endpoint),
                    new AzureKeyCredential(_cs_key)
                );

            BlocklistClient blocklistClient =
            new BlocklistClient
                (
                    new Uri(_cs_endpoint),
                    new AzureKeyCredential(_cs_key)
                );


            BinaryData imageData = BinaryData.FromBytes(imageBytes);

            Response<AnalyzeImageResult> imageResponse;

            imageResponse = await contentSafetyClient.AnalyzeImageAsync(imageData);

            return ImageAnalysisResult(imageResponse.Value.CategoriesAnalysis);
        }

        private bool ImageAnalysisResult(IReadOnlyList<ImageCategoriesAnalysis> r)
        {

            if (r.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity != 0)

                return true;

            return false;

        }
        public async Task<IEnumerable<UsersFilteringResponse>> FilterUsersAsync(FilterUsersDto dto)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var user = await _repoRead.FilterUsersAsync(connection, dto);

            return user;
        }
        public async Task<OtherUserProfile> GetOtherUserCredentials(string? currentUserId, string userProfileId)
        {
            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            var user = await _repoRead.GetOtherUserProfileAsync(connection, userProfileId);

            if (currentUserId is not null)
            {
                var otherUserValidations = await _repoRead.GetOtherUserValidations(connection, currentUserId, userProfileId);


                user.UserSentFollowRequest = otherUserValidations.UserSentFollowRequest;
                user.CurrentUserSentFollowRequest = otherUserValidations.CurrentUserAlreadySentFR;
                user.BothUsersFollow = otherUserValidations.BothUsersFollow;
                user.IsBlockedByUser = otherUserValidations.UserIsBlocked;
                user.CurrentUserIsBlocked = otherUserValidations.CurrentUserIsBlocked;

                return user;

            } else
            {
                user.UserSentFollowRequest = false;
                user.CurrentUserSentFollowRequest = false;
                user.BothUsersFollow = false;
                user.IsBlockedByUser = false;
                user.CurrentUserIsBlocked = false;

                return user;
            }

        }

        public async Task<string> BlockOrUnlockUserAsync(string userId, int otherUserId)
        {

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            bool isBlockedByUser = await _repoValidate.IsBlockedByUserAsync(connection, userId, otherUserId);

            string action = string.Empty;

            if (!isBlockedByUser)
            
                action = await _repoWrite.BlockUserAsync(connection, userId, otherUserId);
            
            else
            
                action = await _repoWrite.UnlockUserAsync(connection, userId, otherUserId);
            

            return action;
        }
    }
}
