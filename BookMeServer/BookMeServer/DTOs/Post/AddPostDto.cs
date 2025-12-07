using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Post
{
    public class AddPostDto
    {
        [Required] public string PostImageUrl { get; set; }
        
        [Required]
        [MaxLength(300, ErrorMessage = "Max. length for description is 300 characters")]
        public string Description { get; set; }
        
        [Required]
        public string PublicId { get; set; }
    }
}
