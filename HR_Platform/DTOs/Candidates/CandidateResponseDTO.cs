using HR_Platform.DTOs.Skills;

namespace HR_Platform.DTOs.Candidates
{
    public class CandidateResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Birthday { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<SkillResponseDTO> Skills { get; set; } = new List<SkillResponseDTO>();
    }
}