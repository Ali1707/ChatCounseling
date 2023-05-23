
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatCounseling.Models
{
    public class ChatRoom
    {
        [Key]
        public int ChatRoomId { get; set; }
        public string? Creator { get; set; }
        public List<Message>? Messages { get; set; } = new List<Message>();
    }
}
