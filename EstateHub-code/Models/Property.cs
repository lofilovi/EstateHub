namespace EstateHub_code.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Property")]
    public class Property
    {
        [Key]
        [Column("property_id")]
        public int PropertyId { get; set; }

        [Column("address")]
        public string Address { get; set; } = "";

        [Column("city")]
        public string City { get; set; } = "";

        [Column("postal_code")]
        public string PostalCode { get; set; } = "";
    }
}
