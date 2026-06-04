using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("customer_issues")]
    public class CustomerIssue
    {
        [Key]
        public int CustomerIssueId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Issue { get; set; } = "";
        public string Status { get; set; } = "Open";
        public string Priority { get; set; } = "Medium";
    }
}
