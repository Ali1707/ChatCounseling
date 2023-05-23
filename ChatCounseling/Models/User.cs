using System.ComponentModel.DataAnnotations;

namespace ChatCounseling.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}
