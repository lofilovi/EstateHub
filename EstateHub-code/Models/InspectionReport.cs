using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstateHub_code.Models
{
    [Table("inspection_reports")]
    public class InspectionReport
    {
        [Key]
        public int InspectionReportId { get; set; }
        public int InspectionId { get; set; }
        public string PropertyAddress { get; set; } = "";
        public string InspectionType { get; set; } = "Move-in";
        public DateTime InspectionDate { get; set; }
        public string InspectorName { get; set; } = "";
        public string PresentDuringInspection { get; set; } = "";
        public string WeatherConditions { get; set; } = "";
        public string OverallCondition { get; set; } = "Good";

        [Column(TypeName = "text")]
        public string Summary { get; set; } = "";

        [Column(TypeName = "longtext")]
        public string RoomItemsJson { get; set; } = "[]";

        [Column(TypeName = "longtext")]
        public string UtilitiesJson { get; set; } = "[]";

        [Column(TypeName = "longtext")]
        public string MeterReadingsJson { get; set; } = "[]";

        [Column(TypeName = "longtext")]
        public string IncludedItemsJson { get; set; } = "[]";

        [Column(TypeName = "longtext")]
        public string IssuesJson { get; set; } = "[]";

        [Column(TypeName = "longtext")]
        public string PhotosJson { get; set; } = "[]";

        public string TenantSignatureName { get; set; } = "";
        public string TenantSignatureDate { get; set; } = "";
        public string LandlordSignatureName { get; set; } = "";
        public string LandlordSignatureDate { get; set; } = "";
    }
}
