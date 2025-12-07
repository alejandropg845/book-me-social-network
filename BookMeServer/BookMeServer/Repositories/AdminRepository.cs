using BookMeServer.DTOs.Admin;
using BookMeServer.Interfaces.Repositories.Admins;
using BookMeServer.Models;
using BookMeServer.Queries;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace BookMeServer.Repositories
{
    public class AdminRepository : IAdminRepository
    {

        public async Task<IEnumerable<PostsAdminDto>> GetPostsAsync(SqlConnection connection)
        {
            var posts = await connection
                .QueryAsync<PostsAdminDto>
                (@"SELECT Id, PostImageUrl, Description FROM Post WHERE IsDeleted = 0");

            return posts;
        }

        public async Task<IEnumerable<UsersAdminDto>> GetUsersAsync(SqlConnection connection)
        {
            var users = await connection.QueryAsync
                <UsersAdminDto>
                (@"SELECT Id, Username FROM Users WHERE IsDisabled = 0
                   AND Username != 'adm1n845'");

            return users;
        }

        public async Task<IEnumerable<DeletedPostsDto>> GetDeletedPostsAsync(SqlConnection connection)
        {
            var posts = await connection
                .QueryAsync<DeletedPostsDto>
                (@"SELECT PostImageUrl, Description FROM Post WHERE IsDeleted = 1");

            return posts;
        }
        public async Task<IEnumerable<string>> GetDisabledUsersAsync(SqlConnection connection)
        {
            var users = await connection.QueryAsync
                <string> (@"SELECT Username FROM Users WHERE IsDisabled = 1");

            return users;
        }

        public async Task DeletePostAsync(int postId, SqlConnection connection)
        => await connection.ExecuteAsync(AdminQueries.DeletePost, new { postId });

        public async Task DisableUserAsync(int userId, SqlConnection connection, IDbTransaction transaction)
        => await connection.ExecuteAsync(AdminQueries.DisableUser, new { userId }, transaction);

    }
}
