using System.ComponentModel.DataAnnotations;

namespace Login_DichVuNgoai.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public ICollection<ExternalLogin> ExternalLogin { get; set; }


    }
}
