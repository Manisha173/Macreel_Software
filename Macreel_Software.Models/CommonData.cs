

namespace Macreel_Software.Models
{
    public class CommonData

    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int day { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public TimeSpan TotalHours { get; set; }
        public string FormattedDate { get; set; }
        public string FormattedInTime { get; set; }
        public string FormattedOutTime { get; set; }
        public string FormattedTotalHours { get; set; }
    }
}
