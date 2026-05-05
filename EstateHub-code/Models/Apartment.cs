using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("apartments")]
    public class Apartment
    {
        public int ApartmentID { get; set; }
        public int PropertyID { get; set; } // Detta är den fysiska siffran i databasen

        public string ApartmentNumber { get; set; } = "";
        public double Size { get; set; }
        public decimal Rent { get; set; }

        // --- NAVIGATION PROPERTY ---
        // Detta gör att vi kan skriva lägenhet.Property.Address
        [ForeignKey("PropertyID")]
        public virtual Property? Property { get; set; }
    }
}