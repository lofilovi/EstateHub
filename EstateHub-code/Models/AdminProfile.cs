using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("admin_profile")]
    public class AdminProfile
    {
        [Key]
        public int AdminProfileId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "Administrator";
        public string Responsibilities { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }
}
