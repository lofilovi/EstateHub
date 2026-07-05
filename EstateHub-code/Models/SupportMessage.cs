using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("support_messages")]
    public class SupportMessage
    {
        [Key]
        public int SupportMessageId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
