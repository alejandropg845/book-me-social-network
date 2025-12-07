using BookMeServer.DTOs.Admin;
using BookMeServer.Interfaces.Repositories.Admins;
using BookMeServer.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Services.App
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _repo;
        private readonly string _stringConnection;
        public AdminService(IAdminRepository repo, IConfiguration config)
        {
            _repo = repo;
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;

        }
        public async Task<IEnumerable<PostsAdminDto>> GetPostsAsync()
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var posts = await _repo.GetPostsAsync(connection);
            return posts;
        }
        public async Task<IEnumerable<UsersAdminDto>> GetUsersAsync()
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var users = await _repo.GetUsersAsync(connection);

            return users;
        }
        public async Task<IEnumerable<DeletedPostsDto>> GetDeletedPostsAsync()
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var posts = await _repo.GetDeletedPostsAsync(connection);
            return posts;
        }
        public async Task<IEnumerable<string>> GetDisabledUsersAsync()
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            return await _repo.GetDisabledUsersAsync(connection);
        }
        public async Task DeletePostAsync(int postId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            await _repo.DeletePostAsync(postId, connection);

        }
        public async Task DisableUserAsync(int userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();

            try 
            {
                await _repo.DisableUserAsync(userId, connection, transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }
    }
}
