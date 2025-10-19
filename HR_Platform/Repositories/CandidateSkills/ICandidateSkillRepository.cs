using HR_Platform.Models;

namespace HR_Platform.Repositories.CandidateSkills
{
    public interface ICandidateSkillRepository
    {
        Task<CandidateSkill?> GetAsync(int candidateId, int skillId);
        Task AddRangeAsync(int candidateId, List<Skill> skills);
        Task RemoveAsync(CandidateSkill candidateSkill);
    }
}
