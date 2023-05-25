using System.ComponentModel.DataAnnotations;

namespace ChatCounseling.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;

        public bool CanSendMessage { get; set; }
        

        public List<Message>? Messages { get; set; } = new List<Message>();

    }
}
