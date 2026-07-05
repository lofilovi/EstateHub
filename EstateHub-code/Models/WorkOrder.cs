using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("work_orders")]
    public class WorkOrder
    {
        [Key]
        public int WorkOrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string Supplier { get; set; } = "";
        public string Product { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
        public string? ApartmentNumber { get; set; }
    }
}
