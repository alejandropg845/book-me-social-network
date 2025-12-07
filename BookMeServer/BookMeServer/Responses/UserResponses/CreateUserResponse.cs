namespace BookMeServer.Responses.UserResponses
{
    public class CreateUserResponse
    {
        public bool UsernameExists { get; set; }
        public int UserId { get; set; }
        public string RoleName { get; set; }
    }
}
