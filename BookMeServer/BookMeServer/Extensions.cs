using BookMeServer.Models;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookMeServer
{
    public static class Extensions
    {
        public static IServiceCollection SetAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JWT:SigningKey"]!))
                };
            });

            return services;
        }

        public static IServiceCollection ConfigureAngularCors(this IServiceCollection services)
        {

            services.AddCors(serviceProvider =>
            {
                serviceProvider.AddPolicy("angularCORS", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        return origin.Contains("localhost");
                    }).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });


            return services;
        }
        public static async Task<Notification> GetNotificationAsync(SqlConnection connection, int notificationId)
        {
            var notification = await connection.QueryFirstAsync<Notification>(
                @"SELECT
	                n.Id,
	                n.PostId,
	                n.CreatedAt,
	                n.ActorId,
	                u.ImageUrl,
	                u.Username,
	                0 AS IsRead,
	                n.Status,
	                n.Type
                FROM Notifications n
                LEFT JOIN Users u 
	                ON n.ActorId = u.Id
                WHERE n.Id = @notificationId"
                ,
                new { notificationId }
            );

            return notification;
        }
    }
}
