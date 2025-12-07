using BookMeServer.DTOs.Post;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Posts;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.PostResponses;
using BookMeServer.Services.Media;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Transactions;

namespace BookMeServer.Services.App
{
    public class PostService : IPostService
    {
        private readonly string _stringConnection;
        private readonly INotificationsService _hubConnService;
        private readonly IHubContext<NotificationsHub> _notisHub;
        private readonly string _cs_endpoint;
        private readonly string _cs_key;
        private readonly string _cloudinary_key;
        private readonly string _cloudinary_secret;
        private readonly IPostWriteRepository _writeRepo;
        private readonly IPostReadRepository _readRepo;
        public PostService(IConfiguration config, INotificationsService connService, IHubContext<NotificationsHub> nots, IPostWriteRepository repo, IPostReadRepository postRead)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _hubConnService = connService;
            _notisHub = nots;
            _cs_key = config["azure_info:content_safety_key"]!;
            _cs_endpoint = config["azure_info:content_safety_endpoint"]!;
            _cloudinary_key = config["cloudinary:api_key"]!;
            _cloudinary_secret = config["cloudinary:api_secret"]!;
            _writeRepo = repo;
            _readRepo = postRead;
        }
        public async Task<IEnumerable<Post>> GetAllPostsAsync(string? userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var posts = await _readRepo.GetAllPostsAsync(connection, userId);

            return posts;
        }
        public async Task<IEnumerable<Post>> GetUserPostsAsync(string? userId, int otherUserProfileId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            return await _readRepo.GetUserPostsAsync(connection, userId, otherUserProfileId);
        }

        public async Task<Post?> GetSinglePostAsync(int postId, string userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync(); 

            var post = await _readRepo.GetSinglePostAsync(connection, postId, userId);

            return post;
        }

        public async Task<AddPostResponse> AddPostAsync(string userId, AddPostDto post)
        {
            var response = new AddPostResponse();

            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            int userIdd = await _readRepo.GetUserIdAsync(connection, userId);

            response.UserExists = userIdd != 0;
            if (userIdd == 0) return response;

            
            bool isExplicitContent = await ValidatePostContentAsync(
                post.PostImageUrl,
                post.Description,
                post.PublicId
            );

            response.IsExplicitContent = isExplicitContent;
            if (isExplicitContent) return response;

            await _writeRepo.AddPostAsync(connection, post.PostImageUrl, post.Description, userIdd);

            return response;

        }

        private async Task<bool> ValidatePostContentAsync(string postImageUrl, string description, string publicId)
        {
            (bool isExplicitImage, bool isExplicitDescription) = 
                await postImageUrl.VerifyContent(
                    _cs_endpoint, 
                    _cs_key, 
                    description
                );


            if (isExplicitImage)
            {
                await publicId.DeleteImageFromCloudinary(_cloudinary_key, _cloudinary_secret);
                return true;
            }

            if (isExplicitDescription)
            {
                await publicId.DeleteImageFromCloudinary(_cloudinary_key, _cloudinary_secret);

                return true;
            }

            return false;
        }
        public async Task<DeletePostResponse> DeletePostAsync(string userId, int postId)
        {

            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            var response = await _readRepo.ValidateDeletePostAsync(connection, postId, userId);

            if (!response.UserExists || !response.PostExists) return response;

            await _writeRepo.SetPostAsDeletedAsync(connection, postId, userId);

            return response;

        }
        public async Task<LikePostResponse> LikePostAsync(int postId, string userId, int postUserId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var response = await _readRepo.ValidateLikePostAsync(
                connection, 
                userId,
                postId,
                postUserId
            );

            if (!response.UserExists || !response.PostExists ||
                response.IsBlockedByUser || response.IsBlockedByOtherUser) return response;

            int likeNotificationId = 0;

            var transaction = await connection.BeginTransactionAsync();
            try
            {

                if (response.PostIsLiked) //User wants to remove the like
                
                    await _writeRepo.RevertLikedPostDataAsync(connection, transaction, userId, postId, postUserId);
                
                else // Usuario no ha dado like al post
                
                    likeNotificationId = await _writeRepo.SetLikedPostDataAsync(connection, transaction, userId, postId, postUserId);

                await transaction.CommitAsync();

                //Enviar notificacion al user por medio del hub
                //y no se avisa si el mismo usuario se auto-likea


                if (int.Parse(userId) != postUserId)
                {
                    string? otherUserId = _hubConnService
                        .GetUserConnection(postUserId.ToString());

                    /* Si no es 0, es porque se dio like, de lo contrario, se quitó */
                    if (otherUserId != null && likeNotificationId is not 0)
                    {

                        /* Abrimos nueva conexión porque la anterior sigue estando asociada a la transacción terminada */
                        
                        var noti = await Extensions.GetNotificationAsync(connection, likeNotificationId);


                        await _notisHub.Clients.Client(otherUserId)
                        .SendAsync("ReceivedNotification", noti);

                        await _notisHub.Clients.Client(otherUserId)
                        .SendAsync("ReceivedAnyNotification",
                        new MessagePopup
                        {
                            ImageUrl = noti.ImageUrl,
                            NotiType = "L",
                            Message = $"{noti.Username} has liked your post.",
                            Username = noti.Username,
                            PostId = postId,
                            UserId = 0
                        });

                    }
                }

                return response;

            } catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
