using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.PostResponses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using BookMeServer.Interfaces.Repositories.Posts;
using System.Data;
namespace BookMeServer.Repositories
{
    public class PostRepository : IPostWriteRepository, IPostReadRepository
    {
        public async Task<IEnumerable<Post>> GetAllPostsAsync(SqlConnection connection, string? userId)
        {
            if (userId is not null)
            {
                var posts = await connection.QueryAsync<Post>(
                    PostQueries.GetPosts, 
                    new { userId }
                );

                return posts;

            } else
            {
                var posts = await connection.QueryAsync<Post>(PostQueries.GetPostsAsAnonymous);

                return posts;
            }
        }
        public async Task<IEnumerable<Post>> GetUserPostsAsync(SqlConnection connection, string? userId, int otherUserProfileId)
        {

            string query;

            if (userId is not null)
                query = PostQueries.GetUserPosts;
            else
                query = PostQueries.GetUserPostsAsAnonymous;

                var userPosts = await connection.QueryAsync<Post>(
                    query, new { userId, otherUserProfileId });

            return userPosts;

        }

        public async Task<Post?> GetSinglePostAsync(SqlConnection connection, int postId, string userId)
        {
            var post = await connection.QueryFirstOrDefaultAsync<Post>(
                PostQueries.GetSinglePost, new { postId, userId });

            return post;
        }

        public async Task<int> GetUserIdAsync(SqlConnection connection, string userId)
        {
            int userIdExists = await connection.ExecuteScalarAsync<int>(
                @"SELECT Id FROM Users WHERE Id = @userId AND IsDisabled = 0",
                new { userId }
            );

            return userIdExists;
        }

        public async Task AddPostAsync(SqlConnection connection, string postImageUrl, string description, int userId)
        {
            var result = await connection.ExecuteAsync(
                PostQueries.AddPost, new
                {
                    postImageUrl,
                    description,
                    UserId = userId
                }
            );
        }
        
        public async Task<DeletePostResponse> ValidateDeletePostAsync(SqlConnection connection, int postId, string userId)
        {

            var (UserExists, PostExists) = await connection.QuerySingleAsync<(bool UserExists, bool PostExists)>
            (
                PostQueries.DeletePostValidations,
                new { postId, userId }
            );

            return new DeletePostResponse
            {
                UserExists = UserExists,
                PostExists = PostExists,
            };
        }
        public async Task SetPostAsDeletedAsync(SqlConnection connection, int postId, string userId)
        {
            await connection
            .ExecuteAsync(
                PostQueries.SetPostAsDeleted,
                new
                {
                    userId,
                    postId
                }
            );

        }
        
        public async Task<LikePostResponse> ValidateLikePostAsync(SqlConnection connection, string userId, int postId, int postUserId)
        {
            var r = await connection.QuerySingleAsync
                <(
                    bool PostExists, 
                    bool UserExists,
                    bool IsBlockedByUser, 
                    bool IsBlockedByOtherUser,
                    bool PostAlreadyLiked
                )>(
                    PostQueries.LikePostValidations,
                    new { userId, postId, postUserId }
                );

            return new LikePostResponse
            {
                IsBlockedByOtherUser = r.IsBlockedByOtherUser,
                IsBlockedByUser = r.IsBlockedByUser,
                PostExists = r.PostExists,
                UserExists = r.UserExists,
                PostIsLiked = r.PostAlreadyLiked
            };
        }
        public async Task RevertLikedPostDataAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int postUserId)
        => await connection.ExecuteAsync(PostQueries.RevertLikedPostData, new { userId, postId, postUserId }, transaction);
        
        public async Task<int> SetLikedPostDataAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int postUserId)
        => await connection.QueryFirstOrDefaultAsync<int>(PostQueries.SetLikedPostData, new { userId, postId, postUserId }, transaction);
          
    }
}
