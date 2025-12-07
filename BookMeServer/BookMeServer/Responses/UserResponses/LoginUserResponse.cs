using BookMeServer.Models;

namespace BookMeServer.Responses.UserResponses
{
    public class LoginUserResponse
    {
        public bool IsCorrect { get; set; }
        public User? User { get; set; }
        public bool IsDisabled { get; set; }
    }
}
