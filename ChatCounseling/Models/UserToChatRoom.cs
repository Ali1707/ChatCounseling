using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatCounseling.Models
{
    public class UserToChatRoom
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("ChatRoom")]
        public int ChatRoomId { get; set; }
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }


        public User User { get; set; }
        public ChatRoom ChatRoom { get; set; }
    }
}
