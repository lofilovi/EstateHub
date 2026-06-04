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
        public int Rooms { get; set; }
        public int Floor { get; set; }
        public string Status { get; set; } = "Ledig";
        public DateTime? AvailableFrom { get; set; }
        public bool ElectricityIncluded { get; set; }
        public bool WaterIncluded { get; set; }
        public bool InternetIncluded { get; set; }
        public bool Balcony { get; set; }
        public bool Furnished { get; set; }
        public string ImageUrl { get; set; } = "";

        // --- NAVIGATION PROPERTY ---
        // Detta gör att vi kan skriva lägenhet.Property.Address
        [ForeignKey("PropertyID")]
        public virtual Property? Property { get; set; }

        // Lägg till detta i Models/Apartment.cs
        public int? TenantID { get; set; } // Nullable (?) för lägenheten kan vara tom

        [ForeignKey("TenantID")]
        public virtual Tenant? Tenant { get; set; }
    }
}
