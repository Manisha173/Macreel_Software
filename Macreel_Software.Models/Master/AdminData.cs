using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace Macreel_Software.Models.Master
{
    public class AdminData
    {

    }

    public class employeeRegistration
    {
        public int Id { get; set; }

        public int? EmpRoleId { get; set; }
        public string? roleName { get; set; }
        public int? EmpCode { get; set; }

        public string? EmpName { get; set; }
        public string? Mobile { get; set; }

    
        public int? DepartmentId { get; set; }
        public string? departmentName { get; set; }
        public int? DesignationId { get; set; }
        public string? designationName { get; set; }

        public int? ReportingManagerId { get; set; }

        public string? EmailId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public int? Salary { get; set; }

        public string? Password { get; set; }

        public IFormFile? ProfilePic { get; set; }
        public IFormFile? AadharImg { get; set; }
        public IFormFile? AadharBackImg { get; set; }
        public IFormFile? PanImg { get; set; }
        public IFormFile? PanBackImg { get; set; }

        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? IfscCode { get; set; }
        public string? BankBranch { get; set; }

        public DateTime Dob { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatus { get; set; }

        
        public string? PresentAddress { get; set; }
        public int? StateId { get; set; }
        public string? stateName { get; set; }
        public int? CityId { get; set; }
        public string? cityName { get; set; }
        public string? Pincode { get; set; }

        public string? EmergencyContactPersonName { get; set; }
        public string? EmergencyContactNum { get; set; }

        public string? CompanyName { get; set; }
        public int? YearOfExperience { get; set; }
        public string? Technology { get; set; }
        public string? CompanyContactNo { get; set; }

        public string? addedBy { get; set; }

        public IFormFile? ExperienceCertificate { get; set; }
        public IFormFile? TenthCertificate { get; set; }
        public IFormFile? TwelthCertificate { get; set; }
        public IFormFile? GraduationCertificate { get; set; }
        public IFormFile? MastersCertificate { get; set; }

        public string? ProfilePicPath { get; set; }
        public string? AadharImgPath { get; set; }
        public string? AadharBackImgPath { get; set; }
        public string? PanImgPath { get; set; }
        public string? PanBackImgPath { get; set; }


        public string? ExperienceCertificatePath { get; set; }


        public string? TenthCertificatePath { get; set; }
        public string? TwelthCertificatePath { get; set; }
        public string? GraduationCertificatePath { get; set; }
        public string? MastersCertificatePath { get; set; }
        public string? ReportingManagerName { get; set; }


     
        public string? SkillIds { get; set; }

        public List<Skill> skill { get; set; } = new List<Skill>();
        //public List<int>? SkillIds { get; set; }

    }
    public class Skill
    {
        public int id { get; set; }
        public int techId { get; set; }
        public string skillName { get; set; }
    }
    public class ReportingManger
    {
        public int id { get; set; }
        public string ReportingManagerName { get; set; }
    }

    public class DbResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int? EmpId { get; set; }
    }

    public class Leave
    {
        public int Id { get; set; }
        public string leaveName { get; set; }
        public string description { get; set; }
    }

    public class AssignLeave
    {
        public int EmployeeId { get; set; }
        public string Leave { get; set; }     // "CL,SL,PL"
        public string LeaveNo { get; set; }   // "10,5,2"
    }

    public class AssignLeaveDetails
    {
        public int id { get; set; }
        public int? EmpId { get; set; }
        public int? NoOfLeave { get; set; }   // "10,5,2"
        public string Leave { get; set; }     // "CL,SL,PL"
        public string description { get; set; }     // "CL,SL,PL"
    }



    public class showLeave
    {
        public int id { get; set; }

        public string leaveType { get; set; }
        public int? noOfLeave { get; set; }
    }

    public class Attendance
    {

        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public string Status { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public decimal? TotalHours { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
    public class AttendanceUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }
    }
    public class EmpWorkingDetails
    {
        public int empId { get; set; }
        public string empCode { get; set; }
        public string empName { get; set; }
        public int totalWorkingDays { get; set; }
        public int presentDays { get; set; }
        public int absentDays { get; set; }
        public int lateEntries { get; set; }
        public int halfDays { get; set; }
        public decimal totalWorkingHours { get; set; }
    }

    public class project
    {
        public int id { get; set; }
        public string? category { get; set; }
        public string? projectTitle { get; set; }
        public string? description { get; set; }

        public bool web { get; set; }
        public bool app { get; set; }
        public bool androidApp { get; set; }
        public bool IOSApp { get; set; }

        public int? appTechnology { get; set; }
        public int? appEmpId { get; set; }
        public int? webTechnology { get; set; }
        public int? webEmpId { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? assignDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime? completionDate { get; set; }
        public IFormFile? sopDocument { get; set; }
        public IFormFile? technicalDocument { get; set; }

      

        public string? SEO { get; set; }
        public string? SMO { get; set; }
        public string? paidAds { get; set; }
        public string? GMB { get; set; }
        public string? sopDocumentPath { get; set; }
        public string? technicalDocumentPath { get; set; }
        public string? appTechnologyName { get; set; }
        public string? webTechnologyName { get; set; }
        public string? appEmpName { get; set; }
        public string? webEmpName { get; set; }
        public string? projectStatus { get; set; }
        public int? delayedDays { get; set; }

        public List<appProjectMember> appProjectMembers { get; set; } = new List<appProjectMember>();
        public List<webProjectMember> webProjectMembers { get; set; } = new List<webProjectMember>();
    }

    public class appProjectMember
    {
        public int? empId { get; set; }
        public string? EmpName { get; set; }
    }
    public class webProjectMember
    {
        public int? empId { get; set; }
        public string? EmpName { get; set; }
    }

    public class Taskassign
    {
        public int id { get; set; }
        public int? empId { get; set; }
        public string? empName { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? assignedBy { get; set; }
        public string? assignedByName { get; set; }

        public IFormFile? document1 { get; set; }
        public IFormFile? document2 { get; set; }
        public bool? autoReassign { get; set; }
        public bool? adminResponse { get; set; }
        public int? timePeriodDay { get; set; }
        public string? document1Path { get; set; }
        public string? document2Path { get; set; }
        public string? taskStatus { get; set; }
        public DateTime? assignedDate { get; set; }
    }


}
