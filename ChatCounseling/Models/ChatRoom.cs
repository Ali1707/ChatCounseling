namespace ChatCounseling.Models
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public int ApplicantId { get; set; }


        public List<Message> messages { get; set; }
        public User Applicant { get; set; }
    }
}
