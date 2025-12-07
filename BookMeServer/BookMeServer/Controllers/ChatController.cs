using BookMeServer.DTOs.Chat;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _service;
        public ChatController(IChatService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetUserChats([FromQuery] string? keyword)
        {
            keyword ??= "%";

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var chats = await _service.GetUserChats(currentUserId, keyword);
            return Ok(chats);
        }

        [HttpGet("chatMessages/{chatId}")]
        [Authorize]
        public async Task<ActionResult> GetChatMessages([FromRoute] string chatId, [FromQuery] int number)
        {
            var messages = await _service.GetChatMessagesAsync(chatId, number);
            return Ok(messages);
        }

        [HttpPut("sendMarkedAsReadMessages")]
        [Authorize]
        public async Task SendMarkedAsReadMessages([FromBody] GetChatMessagesDto dto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _service.SendMarkedAsReadMessagesToOtherUser(dto, userId);
        }

        [HttpPost("sendMessage")]
        [Authorize]
        public async Task<ActionResult> SendMessage(SendMessageDto dto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            string senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.SendMessageAsync(dto, senderId);

            if(!result.SenderExists) 
                return Unauthorized
                (
                    new { Message = "You have been banned from BookMe for uploading explicit content" }
                );


            if(!result.ReceiverExists) 
                return BadRequest
                (
                    new 
                    { 
                        Message = "The other user has " +
                        "been banned or doesn't exist" 
                    }
                );

            if (!result.ChatExists) return NotFound(new { Message = "This chat doesn't exist" });

            if (result.IsBlockedByUser)
                return BadRequest(new { Message = "You must unlock this user first to send a message" });

            if (result.IsBlockedByOtherUser)
                return BadRequest(new { Message = "This user has blocked you" });

            return Ok();
        }

        [HttpPut("markChatMessages/{chatId}")]
        [Authorize]
        public async Task<ActionResult<Chat>> MarkAllUserChatMessagesAsRead([FromRoute] string chatId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var updatedChat = await _service.MarkAllUserChatMessagesAsReadAsync(chatId, userId);

            return Ok(updatedChat);

        }


    }

    
}
