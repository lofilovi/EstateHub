using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("inspections")]
    public class Inspection
    {
        [Key]
        public int InspectionId { get; set; }
        public string ApartmentNumber { get; set; } = "";
        public DateTime InspectionDate { get; set; }
        public string Inspector { get; set; } = "";
        public string Status { get; set; } = "Planned";
        public string Notes { get; set; } = "";
    }
}
