using System.ComponentModel.DataAnnotations;

namespace HR_Platform.DTOs.Skills
{
    public class AddSkillsDTO
    {
        [Required(ErrorMessage = "SkillIds are required")]
        public List<int> skillIds { get; set; } = new List<int>();
    }
}
