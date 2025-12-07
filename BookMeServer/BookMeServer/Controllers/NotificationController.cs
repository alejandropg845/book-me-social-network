
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("getNotifications")]
        [AllowAnonymous]
        public async Task<ActionResult> GetUserNotifications()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Ok(new Notification[] {});
            }

            var notifications = await _service.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("markNotisAsRead")]
        [Authorize]
        public async Task<ActionResult> MarkAllUserNotificationsAsRead()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _service.MarkAllNotificationsAsRead(userId);
            
            return Ok();
        }

        [HttpPut("markNotiAsRead/{notificationId:int}")]
        [Authorize]
        public async Task<ActionResult> MarkSingleNotificationAsRead([FromRoute] int notificationId)
        {

            await _service.MarkSingleNotificationAsRead(notificationId);

            return Ok();

        }
        [HttpPut("sendFollowRequest/{recipientId:int}")]
        [Authorize]
        public async Task<ActionResult> SendFollowRequest([FromRoute] int recipientId)
        {
            string actorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.SendFollowRequest(actorId, recipientId.ToString());

            if (result.UserSendsHimselfFR) return BadRequest(new { Message = "You can't send follow request to yourself" });

            if (!result.SenderExists) return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if (!result.ReceiverExists) return BadRequest(new { Message = "The user you sent the follow request has been banned" });

            if (result.AlreadyFriends) return BadRequest(new { Message = "You both are already friends" });

            if (result.FollowRequestAlreadyExists)
                return BadRequest(new { Message = "You already sent a follow request to this user" });

            if (result.IsBlockedByUser)
                return BadRequest(new { Message = "You must unlock this user first to send a follow request." });

            if (result.IsBlockedByOtherUser)
                return BadRequest(new { Message = "This user has blocked you." });

            if (result.ChatExists) return Forbid();

            string message;

            if (result.SenderIsAlreadyReceiver)
                message = "You accepted this follow request";
            else
                message = "Follow request sent";

                return Ok(new { Message = message });


        }

        [HttpDelete("rejectFR/{actorId:int}")]
        [Authorize]
        public async Task<ActionResult> RejectFollowRequest([FromRoute] int actorId)
        {
            string? recipientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (recipientId is null) return Unauthorized();

            await _service.RejectFRAsync(int.Parse(recipientId), actorId);

            return NoContent();
        }


        //[HttpPut("respondUserFollowRequest/{actorId:int}")]
        //[Authorize]
        //public async Task<ActionResult> RespondToUserFollowRequest([FromRoute] int actorId, [FromQuery] bool isAccepted, [FromQuery] int notificationId)
        //{
        //    if (!ModelState.IsValid) return BadRequest(ModelState);


        //    string userReceiverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        //    var result = await _service
        //        .RespondToUserFollowRequest(userReceiverId, actorId, notificationId, isAccepted);

        //    if (!result.SenderExists) return BadRequest(new { Message = "You can't do this action because this user has been banned" });

        //    if (!result.ReceiverExists) return Unauthorized(new { Message = "You have been banned for uploading explicit content" });

        //    if (!result.FollowRequestExists) return BadRequest(new { Message = "This follow request doesn't exist" });

        //    if (result.IsBlockedByUser)
        //        return BadRequest(new { Message = "You must unlock this user first to send a follow request." });

        //    if (result.IsBlockedByOtherUser)
        //        return BadRequest(new { Message = "This user has blocked you." });

        //    if (result.ChatExists)
        //        return Forbid();

        //    return Ok(new { Message = "Done" });

        //}
    }
}
