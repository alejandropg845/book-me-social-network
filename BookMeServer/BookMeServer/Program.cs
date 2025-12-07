
using BookMeServer;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Admins;
using BookMeServer.Interfaces.Repositories.Chats;
using BookMeServer.Interfaces.Repositories.Comments;
using BookMeServer.Interfaces.Repositories.Likes;
using BookMeServer.Interfaces.Repositories.Notifications;
using BookMeServer.Interfaces.Repositories.Posts;
using BookMeServer.Interfaces.Repositories.Replies;
using BookMeServer.Interfaces.Repositories.Users;
using BookMeServer.Interfaces.Services;
using BookMeServer.Middlewares;
using BookMeServer.Repositories;
using BookMeServer.Services.App;
using BookMeServer.Services.Auth;
using BookMeServer.Services.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//REPOS
builder.Services.AddScoped<IUsersWriteRepository, UsersRepository>();
builder.Services.AddScoped<IUsersReadRepository, UsersRepository>();
builder.Services.AddScoped<IUsersValidationsRepository, UsersRepository>();
builder.Services.AddScoped<IPostWriteRepository, PostRepository>();
builder.Services.AddScoped<IPostReadRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IChatWriteRepository, ChatRepository>();
builder.Services.AddScoped<IChatReadRepository, ChatRepository>();
builder.Services.AddScoped<INotificationsWriteRepository, NotificationsRepository>();
builder.Services.AddScoped<INotificationsReadRepository, NotificationsRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IRepliesRepository, RepliesRepository>();

//SERVICES
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IRepliesService, RepliesService>();
builder.Services.AddScoped<IUserService, UserService>();

//Services SINGLETON
builder.Services.AddSingleton<INotificationsService, NotificationsService>();
builder.Services.AddSingleton<IChatsConnectionsService, ChatConnectionsService>();


builder.Services.AddSignalR();

builder.Services.ConfigureAngularCors();


var config = builder.Configuration;

builder.Services.SetAuthentication(config);

var app = builder.Build();


app.UseCors("angularCORS");



app.MapHub<NotificationsHub>("/hub-notification");
app.MapHub<UserChatsHub>("/userChats-hub");

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandling>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
