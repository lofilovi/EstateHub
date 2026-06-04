using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("app_settings")]
    public class AppSetting
    {
        [Key]
        public int AppSettingId { get; set; }
        public bool DarkMode { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool AutoSaveReports { get; set; } = true;
    }
}
