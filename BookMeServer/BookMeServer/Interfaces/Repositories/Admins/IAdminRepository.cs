using BookMeServer.DTOs.Admin;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Admins
{
    public interface IAdminRepository
    {
        Task<IEnumerable<PostsAdminDto>> GetPostsAsync(SqlConnection connection);
        Task<IEnumerable<UsersAdminDto>> GetUsersAsync(SqlConnection connection);
        Task<IEnumerable<DeletedPostsDto>> GetDeletedPostsAsync(SqlConnection connection);
        Task<IEnumerable<string>> GetDisabledUsersAsync(SqlConnection connection);
        Task DeletePostAsync(int postId, SqlConnection connection);
        Task DisableUserAsync(int userId, SqlConnection connection, IDbTransaction transaction);
    }
}
