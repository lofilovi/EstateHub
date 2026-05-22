using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("tenants")] // Matchar namnet i din SQL-query (litet t)
    public class Tenant
    {
        public int TenantID { get; set; }
        public string? FirstName { get; set; } // ? betyder att fältet får vara NULL
        public string? LastName { get; set; }
        public string? PersonalNumber { get; set; }
        public string? Address { get; set; }
        public string? MobilePhone { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
