using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("accounting_records")]
    public class AccountingRecord
    {
        [Key]
        public int AccountingRecordId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
    }
}
