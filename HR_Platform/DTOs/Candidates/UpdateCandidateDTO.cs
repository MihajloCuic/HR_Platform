using System.ComponentModel.DataAnnotations;

namespace HR_Platform.DTOs.Candidates
{
    public class UpdateCandidateDTO
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name can be between 2 and 100 characters")]
        public string? Name { get; set; }

        public DateOnly? Birthday { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot have more than 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot have more than 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}
