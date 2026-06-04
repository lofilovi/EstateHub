using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("calendar_events")]
    public class CalendarEvent
    {
        [Key]
        public int CalendarEventId { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime StartsAt { get; set; }
    }
}
