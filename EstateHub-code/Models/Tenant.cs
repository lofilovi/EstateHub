using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("tenants")] // Matchar namnet i din SQL-query (litet t)
    public class Tenant
    {
        public int TenantID { get; set; } // Matchar exakt TenantID

        public string Name { get; set; } = ""; // Matchar Name

        public string Phone { get; set; } = ""; // Matchar Phone

        public string Email { get; set; } = ""; // Matchar Email
    }
}