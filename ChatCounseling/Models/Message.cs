namespace ChatCounseling.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Body { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; }
    }
}