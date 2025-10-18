using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Platform.Models
{
    public class CandidateSkill
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; } = null!;

        [Required]
        public int SkillId { get; set; }

        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;
    }
}
