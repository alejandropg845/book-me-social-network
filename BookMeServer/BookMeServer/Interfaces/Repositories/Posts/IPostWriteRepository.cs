using BookMeServer.DTOs.Post;
using BookMeServer.Models;
using BookMeServer.Responses.PostResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Posts
{
    public interface IPostWriteRepository
    {
        Task AddPostAsync(SqlConnection connection, string postImageUrl, string description, int userId);
        Task SetPostAsDeletedAsync(SqlConnection connection, int postId, string userId);
        Task RevertLikedPostDataAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int postUserId);
        Task<int> SetLikedPostDataAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int postUserId);
    }
}
